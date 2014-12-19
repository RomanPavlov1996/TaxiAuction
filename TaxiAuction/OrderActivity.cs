using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using LegacyBar.Library.BarActions;
using LegacyBar.Library.BarBase;
using System;
using System.Threading;

namespace TaxiAuction
{
	[Activity( Label = "@string/ApplicationName", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleTop, Theme = "@style/AppTheme" )]
	public class OrderActivity : LegacyBarActivity
	{
		private TextView txtFrom;
		private TextView txtThru;
		private TextView txtTo;
		private TextView txtTime;
		private TextView txtPrice;
		private TextView lblThru;
		private Button btnTake;
		private Button btnArrived;
		private Button btnPersonInCar;
		private Button btnCompleted;
		private Button btnPhone;

		System.Timers.Timer tmrState = new System.Timers.Timer { AutoReset = true, Enabled = true, Interval = 30000 };

		protected override void OnCreate ( Bundle bundle )
		{
			base.OnCreate( bundle );

			SetContentView( Resource.Layout.Order );

			#region ActionBars settings
			//TopActionBar settings
			MenuId = Resource.Menu.ListMenu;
			LegacyBar = FindViewById<LegacyBar.Library.Bar.LegacyBar>( Resource.Id.ActionBar );
			LegacyBar.SetHomeLogo( Resource.Drawable.Icon );
			LegacyBar.Title = "Заказ";

			var itemActionBarAction = new MenuItemLegacyBarAction(
				this, Resource.Id.btnRefresh, Resource.Drawable.ic_refresh,
				Resource.String.Refresh )
			{
				ActionType = ActionType.Always
			};
			LegacyBar.AddAction( itemActionBarAction );

			itemActionBarAction = new MenuItemLegacyBarAction(
				this, Resource.Id.btnGetBalance, Resource.Drawable.ic_balance,
				Resource.String.Balance )
			{
				ActionType = ActionType.IfRoom
			};
			LegacyBar.AddAction( itemActionBarAction );


			//BottomActionBar settings
			var bottomActionBar = FindViewById<LegacyBar.Library.Bar.LegacyBar>( Resource.Id.bottomActionbar );

			var action = new MenuItemLegacyBarAction( this, Resource.Id.btnMagicCall, Resource.Drawable.ic_call,
													 Resource.String.CallMagicNumber )
			{
				ActionType = ActionType.Always
			};

			bottomActionBar.AddAction( action );

			action = new MenuItemLegacyBarAction(
				this, Resource.Id.btnSetArea, Resource.Drawable.ic_location,
				Resource.String.ChooseArea )
			{
				ActionType = ActionType.Always
			};
			bottomActionBar.AddAction( action );

			action = new MenuItemLegacyBarAction(
				this, Resource.Id.btnLogout, Resource.Drawable.ic_exit,
				Resource.String.Logout )
			{
				ActionType = ActionType.Always
			};
			bottomActionBar.AddAction( action );

			action = new MenuItemLegacyBarAction( this, Resource.Id.btnSettings, Resource.Drawable.ic_settings,
													 Resource.String.Settings )
			{
				ActionType = ActionType.Always
			};

			bottomActionBar.AddAction( action );
			#endregion

			if ( UserInfo.State == StateTypes.Free )
			{
				AddHomeAction( typeof( ListActivity ), Resource.Drawable.Icon );
			}

			txtFrom = FindViewById<TextView>( Resource.Id.txtFrom );
			txtThru = FindViewById<TextView>( Resource.Id.txtThru );
			txtTo = FindViewById<TextView>( Resource.Id.txtTo );
			txtTime = FindViewById<TextView>( Resource.Id.txtTime );
			txtPrice = FindViewById<TextView>( Resource.Id.txtPrice );
			lblThru = FindViewById<TextView>( Resource.Id.lblThru );
			btnPhone = FindViewById<Button>( Resource.Id.btnPhone );
			btnTake = FindViewById<Button>( Resource.Id.btnTake );
			btnArrived = FindViewById<Button>( Resource.Id.btnArrived );
			btnPersonInCar = FindViewById<Button>( Resource.Id.btnPersonInCar );
			btnCompleted = FindViewById<Button>( Resource.Id.btnCompleted );

			ThreadPool.QueueUserWorkItem( o => Refresh( ) );

			btnPhone.Click += ( o, e ) =>
				{
					var callIntent = new Intent( Intent.ActionCall );
					callIntent.SetData( Android.Net.Uri.Parse( "tel:" + UserInfo.CurrentOrder.Phone ) );
					StartActivity( callIntent );
				};

			tmrState.Elapsed += tmrState_Elapsed;
		}

		void tmrState_Elapsed ( object sender, ElapsedEventArgs e )
		{
			ThreadPool.QueueUserWorkItem( o => CheckState( ) );
		}

		private void Refresh ( )
		{
			if ( UserInfo.State == StateTypes.ActiveOrder )
			{
				UserInfo.CurrentOrder = Requests.GetActiveOrder( UserInfo.UserId );
			}
			else if ( UserInfo.State == StateTypes.PersonInCar )
			{
				UserInfo.CurrentOrder = Requests.GetActiveOrderInCar( UserInfo.UserId );
			}
			else if ( UserInfo.State == StateTypes.Arrived )
			{
				UserInfo.CurrentOrder = Requests.GetActiveOrder( UserInfo.UserId );
			}

			RunOnUiThread( ( ) =>
			{
				if ( UserInfo.State == StateTypes.Free )
				{
					AddHomeAction( Finish, Resource.Drawable.Icon );
				}
				else
				{
					LegacyBar.ClearHomeAction( );
					LegacyBar.SetHomeLogo( Resource.Drawable.Icon );
				}

				txtFrom.Text = UserInfo.CurrentOrder.From;
				txtThru.Text = "";
				txtTo.Text = UserInfo.CurrentOrder.To;
				txtTime.Text = UserInfo.CurrentOrder.Time;
				txtPrice.Text = UserInfo.CurrentOrder.Price;

				if ( UserInfo.CurrentOrder.Waypoints.Count != 0 )
				{
					foreach ( var cWaypoint in UserInfo.CurrentOrder.Waypoints )
					{
						txtThru.Text += cWaypoint + "\n";
					}
					txtThru.Text = txtThru.Text.Remove( txtThru.Text.Length - 1 );
				}
				else
				{
					txtThru.Visibility = ViewStates.Gone;
					lblThru.Visibility = ViewStates.Gone;
				}

				btnPhone.Visibility = ViewStates.Gone;
				btnArrived.Visibility = ViewStates.Gone;
				btnPersonInCar.Visibility = ViewStates.Gone;
				btnCompleted.Visibility = ViewStates.Gone;
				btnTake.Visibility = ViewStates.Gone;

				switch ( UserInfo.State )
				{
					case StateTypes.Free:
						btnTake.Visibility = ViewStates.Visible;
						break;
					case StateTypes.ActiveOrder:
						btnPersonInCar.Visibility = ViewStates.Visible;
						btnArrived.Visibility = ViewStates.Visible;
						btnPhone.Visibility = ViewStates.Visible;
						break;
					case StateTypes.Arrived:
						btnPhone.Visibility = ViewStates.Visible;
						btnPersonInCar.Visibility = ViewStates.Visible;
						break;
					case StateTypes.PersonInCar:
						btnPhone.Visibility = ViewStates.Visible;
						btnCompleted.Visibility = ViewStates.Visible;
						break;
				}

				btnTake.Click += btnTake_Click;
				btnArrived.Click += btnArrived_Click;
				btnPersonInCar.Click += btnPersonInCar_Click;
				btnCompleted.Click += btnCompleted_Click;

				if ( UserInfo.CurrentOrder.Accepted == "0" )
				{
					var dialog = new AlertDialog.Builder( this );
					dialog.SetTitle( Resource.String.AcceptOrder );
					dialog.SetMessage( String.Format( "{0} -> {1}\nКогда: {2}\nЦена: {3}р.", UserInfo.CurrentOrder.From, UserInfo.CurrentOrder.To, UserInfo.CurrentOrder.Time, UserInfo.CurrentOrder.Price ) );
					dialog.SetPositiveButton( Resource.String.Accept, ( sender, args ) => ThreadPool.QueueUserWorkItem( o =>
						{
							Requests.AcceptOrder( UserInfo.UserId, UserInfo.CurrentOrder.ID );
							UserInfo.CurrentOrder.Accepted = "1";
						} ) );
					dialog.SetNegativeButton( Resource.String.Decline, ( sender, args ) => ThreadPool.QueueUserWorkItem( o =>
						{
							Requests.DeclineOrder( UserInfo.UserId, UserInfo.CurrentOrder.ID );
							StartActivity( typeof( ListActivity ) );
						} ) );
					dialog.Show( );
				}
			} );
		}

		private void CheckState ( )
		{
			var state = Requests.GetState( UserInfo.UserId );

			if ( state == "-50" )
			{
				tmrState.Stop( );
				StartActivity( typeof( AuthActivity ) );
			}
		}

		void btnCompleted_Click ( object sender, EventArgs e )
		{
			LegacyBar.ProgressBarVisibility = ViewStates.Visible;

			ThreadPool.QueueUserWorkItem( o =>
			{
				var response = Requests.ReportCompleted( UserInfo.CurrentOrder.ID );

				RunOnUiThread( ( ) =>
					{
						if ( response == "1" )
						{
							Toast.MakeText( Application.Context, Resource.String.Order_Completed, ToastLength.Short ).Show( );
							UserInfo.State = StateTypes.Free;
							tmrState.Stop( );
							Finish( );
							StartActivity( typeof( ListActivity ) );
						}
						else
						{
							//Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
						}

						LegacyBar.ProgressBarVisibility = ViewStates.Gone;
					} );
			} );
		}

		void btnPersonInCar_Click ( object sender, EventArgs e )
		{
			LegacyBar.ProgressBarVisibility = ViewStates.Visible;

			ThreadPool.QueueUserWorkItem( o =>
			{
				var response = Requests.ReportPersonInCar( UserInfo.UserId, UserInfo.CurrentOrder.ID );

				RunOnUiThread( ( ) =>
						 {
							 if ( response == "1" )
							 {
								 UserInfo.State = StateTypes.PersonInCar;
								 Refresh( );
							 }
							 else
							 {
								 //Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
							 }

							 LegacyBar.ProgressBarVisibility = ViewStates.Gone;
						 } );
			} );
		}

		void btnArrived_Click ( object sender, EventArgs e )
		{
			LegacyBar.ProgressBarVisibility = ViewStates.Visible;

			ThreadPool.QueueUserWorkItem( o =>
			{
				var response = Requests.ReportArrived( UserInfo.UserId, UserInfo.CurrentOrder.ID );

				RunOnUiThread( ( ) =>
						 {
							 if ( response == "1" )
							 {
								 Toast.MakeText( Application.Context, Resource.String.Order_Notification_Sent, ToastLength.Short ).Show( );
								 UserInfo.State = StateTypes.Arrived;
								 Refresh( );
							 }
							 else
							 {
								 //Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
							 }

							 LegacyBar.ProgressBarVisibility = ViewStates.Gone;
						 } );
			} );
		}

		void btnTake_Click ( object sender, EventArgs e )
		{
			LegacyBar.ProgressBarVisibility = ViewStates.Visible;

			ThreadPool.QueueUserWorkItem( o =>
			{
				var response = Requests.TakeOrder( UserInfo.UserId, UserInfo.CurrentOrder.ID );

				RunOnUiThread( ( ) =>
						 {
							 if ( response == "1" )
							 {
								 UserInfo.State = StateTypes.ActiveOrder;
								 Refresh( );
								 tmrState.Start( );
							 }
							 else
							 {
								 //Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
							 }

							 LegacyBar.ProgressBarVisibility = ViewStates.Gone;
						 } );
			} );
		}

		public override bool OnKeyDown ( Keycode keyCode, KeyEvent e )
		{
			if ( keyCode == Keycode.Back )
			{
				if ( UserInfo.State != StateTypes.Free )
				{
					UserInfo.MustExit = true;
				}
				Finish( );
				return true;
			}

			return base.OnKeyDown( keyCode, e );
		}

		protected override void OnResume ( )
		{
			base.OnResume( );
			if ( UserInfo.MustExit )
			{
				Finish( );
			}
		}

		public override bool OnOptionsItemSelected ( IMenuItem item )
		{
			switch ( item.ItemId )
			{
				case Resource.Id.btnGetBalance:
					LegacyBar.ProgressBarVisibility = ViewStates.Visible;
					ThreadPool.QueueUserWorkItem( o => GetBalance( ) );
					return true;
				case Resource.Id.btnLogout:
					LegacyBar.ProgressBarVisibility = ViewStates.Visible;
					StopService( new Intent( "ru.nwdgroup.geolocationservice" ) );
					GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).Edit( ).Remove( "UserId" ).Commit( );
					UserInfo.MustExit = true;
					ThreadPool.QueueUserWorkItem( o => Logout( ) );
					return true;
				case Resource.Id.btnRefresh:
					LegacyBar.ProgressBarVisibility = ViewStates.Visible;
					ThreadPool.QueueUserWorkItem( o => Refresh( ) );
					return true;
				case Resource.Id.btnMagicCall:
					var callIntent = new Intent( Intent.ActionCall );
					callIntent.SetData( Android.Net.Uri.Parse( "tel:" + UserInfo.MagicNumber ) );
					StartActivity( callIntent );
					return true;
				case Resource.Id.btnSettings:
					var settingsIntent = new Intent( this, typeof( SettingsActivity ) );
					StartActivityForResult( settingsIntent, 0 );
					return true;
				case Resource.Id.btnSetArea:
					ShowSetAreaDialog( );
					return true;
				default:
					return base.OnOptionsItemSelected( item );
			}
		}
		
		private void ShowSetAreaDialog ( )
		{
			var dialog = new AlertDialog.Builder( this );

			dialog.SetTitle( Resource.String.ChooseArea );
			dialog.SetCancelable( true );
			dialog.SetAdapter( UserInfo.AreasArrayAdapter, ( sender, args ) => ThreadPool.QueueUserWorkItem( o =>
			{
				Requests.SetArea( UserInfo.UserId, UserInfo.AreasId [ args.Which ] );
				PreferenceManager.GetDefaultSharedPreferences( this ).Edit( ).PutBoolean( "UseGPS", false ).Commit( );
				Requests.SetMode( UserInfo.UserId, "2" );
				StopService( new Intent( "ru.nwdgroup.geolocationservice" ) );
			} ) );

			dialog.Show( );
		}

		private void Logout ( )
		{
			Requests.Logout( UserInfo.UserId );
			Finish( );
		}

		private void GetBalance ( )
		{
			var balance = Requests.GetBalance( UserInfo.UserId );

			RunOnUiThread( ( ) =>
			{
				if ( balance != null )
				{
					Toast.MakeText( Application.Context, String.Format( "Ваш баланс: {0} руб.", balance ), ToastLength.Long ).Show( );
				}
				else
				{
					Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Long ).Show( );
				}

				LegacyBar.ProgressBarVisibility = ViewStates.Gone;
			} );
		}
	}
}