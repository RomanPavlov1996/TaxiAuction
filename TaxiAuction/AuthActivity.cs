using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using Java.Util;
using LegacyBar.Library.BarBase;
using PushSharp.Client;
using System;
using System.Threading;
using Xamarin.Geolocation;
using System.Net;

namespace TaxiAuction
{
	[Activity( Label = "@string/ApplicationName", Icon = "@drawable/Icon", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Theme = "@style/AppTheme" )]
	public class AuthActivity : LegacyBarActivity
	{
		private const string SharedSecret = "ISyHRpXlLt";
		private EditText txtLicense, txtPassword;
		private TextView lblError;
		private Button btnLogin;
		private bool activityIsVisible;

		protected override void OnCreate ( Bundle bundle )
		{
			base.OnCreate( bundle );

			UserInfo.UserId = GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).GetString( "UserId", null );
			UserInfo.License = GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).GetString( "License", null );
			UserInfo.PushId = GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).GetString( "PushId", null );

			if ( UserInfo.UserId != null )
			{
				CheckState( UserInfo.UserId );
			}
			else
			{
				Start( );
			}
		}

		private void Start ( )
		{
			//Check to ensure everything's setup right
			PushClient.CheckDevice( Application.Context );
			PushClient.CheckManifest( Application.Context );

			PushClient.Register( Application.Context, PushHandlerBroadcastReceiver.SENDER_IDS );

			SetContentView( Resource.Layout.Auth );
			activityIsVisible = true;

			MenuId = Resource.Menu.EmptyMenu;
			LegacyBar = FindViewById<LegacyBar.Library.Bar.LegacyBar>( Resource.Id.ActionBar );
			LegacyBar.SetHomeLogo( Resource.Drawable.Icon );

			txtLicense = FindViewById<EditText>( Resource.Id.txtLicense );
			txtPassword = FindViewById<EditText>( Resource.Id.txtPassword );
			lblError = FindViewById<TextView>( Resource.Id.lblError );
			btnLogin = FindViewById<Button>( Resource.Id.btnLogin );

			txtLicense.Text = GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).GetString( "License", String.Empty );

			var encryptedPassword = GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).GetString( "Password", null );
			txtPassword.Text = encryptedPassword != null
								   ? Crypto.DecryptStringAES( encryptedPassword, SharedSecret )
								   : String.Empty;

			lblError.Text = String.Empty;
			btnLogin.Click += btnLogin_Click;

			LegacyBar.ProgressBarVisibility = ViewStates.Visible;
			ThreadPool.QueueUserWorkItem( o => GetAreas( ) );
		}

		private void GetAreas ( )
		{
			UserInfo.AreasArrayAdapter = Requests.GetAreas( Application.Context );

			RunOnUiThread( ( ) =>
			{
				LegacyBar.ProgressBarVisibility = ViewStates.Gone;
			} );
		}

		private void btnLogin_Click ( object sender, EventArgs e )
		{
			if ( !new Geolocator( this ).IsGeolocationEnabled )
			{
				Toast.MakeText( this, Resource.String.Error_GPS, ToastLength.Long ).Show( );
			}

			if ( UserInfo.PushId == null )
			{
				Toast.MakeText( this, Resource.String.Error_Push, ToastLength.Long ).Show( );
			}

			LegacyBar.ProgressBarVisibility = ViewStates.Visible;
			ThreadPool.QueueUserWorkItem( o => Login( ) );
		}

		private void Login ( )
		{
			var authResponse = Requests.Authorize( txtLicense.Text, txtPassword.Text, UserInfo.PushId );

			int intResponse;
			var responseIsParsed = int.TryParse( authResponse, out intResponse );

			if ( responseIsParsed && intResponse > 0 )
			{
				UserInfo.UserId = authResponse;
				UserInfo.License = txtLicense.Text;

				var editor = GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).Edit( );
				editor.PutString( "UserId", UserInfo.UserId );
				editor.PutString( "PushId", UserInfo.PushId );
				editor.PutString( "License", txtLicense.Text );
				editor.PutString( "Password", Crypto.EncryptStringAES( txtPassword.Text, SharedSecret ) );
				editor.Commit( );

				UserInfo.MagicNumber = Requests.GetMagicNumber( );
				UserInfo.Balance = Requests.GetBalance(UserInfo.UserId);

				if ( Requests.GetMode( UserInfo.UserId ) == "2" )
				{
					PreferenceManager.GetDefaultSharedPreferences( this ).Edit( ).PutBoolean( "UseGPS", false ).Commit( );
					UserInfo.Area = Requests.GetArea( UserInfo.UserId );
				}
				else
				{
					PreferenceManager.GetDefaultSharedPreferences( this ).Edit( ).PutBoolean( "UseGPS", true ).Commit( );
				}

				RunOnUiThread( ( ) => CheckState( UserInfo.UserId ) );
			}
			else
			{
				RunOnUiThread( ( ) =>
					{
						LegacyBar.ProgressBarVisibility = ViewStates.Gone;

						switch ( authResponse )
						{
							case "-1": lblError.SetText( Resource.String.Error_1 );
								break;
							case "-2": lblError.SetText( Resource.String.Error_2 );
								break;
							case "-3": lblError.SetText( Resource.String.Error_3 );
								break;
							case "-4": lblError.SetText( Resource.String.Error_4 );
								break;
							case "-5": lblError.SetText( Resource.String.Error_5 );
								break;
							default: lblError.SetText( Resource.String.Error );
								break;
						}
					} );
			}
		}

		private void CheckState ( string authResponse )
		{
			if ( PreferenceManager.GetDefaultSharedPreferences( this ).GetBoolean( "UseGPS", true ) )
			{
				StartService( new Intent( "ru.nwdgroup.geolocationservice" ) );
				Continue( authResponse );
			}
			else
			{
				StopService( new Intent( "ru.nwdgroup.geolocationservice" ) );
				Continue( authResponse );
			}
		}

		private void Continue ( string authResponse )
		{
			ThreadPool.QueueUserWorkItem( o =>
				{
					var state = Requests.GetState( authResponse );

					RunOnUiThread( ( ) =>
					{
						switch ( state )
						{
							case "-50":
								UserInfo.State = StateTypes.Free;
								StartActivity( typeof( ListActivity ) );
								break;
							case "-51":
								UserInfo.State = StateTypes.ActiveOrder;
								var activeOrder = Requests.GetActiveOrder( UserInfo.UserId );
								if ( activeOrder != null )
								{
									UserInfo.CurrentOrder = activeOrder;
									StartActivity( typeof( OrderActivity ) );
								}
								else
								{
									Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
								}
								break;
							case "-52":
								UserInfo.State = StateTypes.PersonInCar;
								var activeOrderInCar = Requests.GetActiveOrderInCar( UserInfo.UserId );
								if ( activeOrderInCar != null )
								{
									UserInfo.CurrentOrder = activeOrderInCar;
									StartActivity( typeof( OrderActivity ) );
								}
								else
								{
									Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
								}
								break;
							case "-53":
								UserInfo.State = StateTypes.Arrived;
								var activeOrderArrived = Requests.GetActiveOrder( UserInfo.UserId );
								if ( activeOrderArrived != null )
								{
									UserInfo.CurrentOrder = activeOrderArrived;
									StartActivity( typeof( OrderActivity ) );
								}
								else
								{
									Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
								}
								break;
							default:
								if ( !activityIsVisible )
								{
									Start( );
								}
								Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
								break;
						}
					} );
				} );
		}

		protected override void OnResume ( )
		{
			base.OnResume( );

			if ( UserInfo.MustExit )
			{
				UserInfo.MustExit = false;
				Finish( );
			}
		}
	}
}