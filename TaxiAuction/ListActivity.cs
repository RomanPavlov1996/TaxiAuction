using System.Linq;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using LegacyBar.Library.BarActions;
using LegacyBar.Library.BarBase;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TaxiAuction
{
	[Activity( Label = "@string/ApplicationName", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleTask, Theme = "@style/AppTheme" )]
	public class ListActivity : LegacyBarActivity
	{
		List<Order> OrdersList = new List<Order>( );

		ListView lstOrders;
		TextView txtNoOrders;

		private System.Timers.Timer tmrState = new System.Timers.Timer { AutoReset = true, Enabled = true, Interval = 30000 };

		protected override void OnCreate ( Bundle bundle )
		{
			base.OnCreate( bundle );
			SetContentView( Resource.Layout.List );

			tmrState.Elapsed += tmrState_Elapsed;
			tmrState.Start( );

			#region ActionBars settings
			//TopActionBar settings
			MenuId = Resource.Menu.ListMenu;
			LegacyBar = FindViewById<LegacyBar.Library.Bar.LegacyBar>( Resource.Id.ActionBar );
			LegacyBar.SetHomeLogo( Resource.Drawable.Icon );
			LegacyBar.Title = "Баланс: " + UserInfo.Balance + " р.";

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

			lstOrders = FindViewById<ListView>( Resource.Id.lstOrders );
			txtNoOrders = FindViewById<TextView>( Resource.Id.txtNoOrders );

			lstOrders.ItemClick += lstOrders_ItemClick;

			LegacyBar.ProgressBarVisibility = ViewStates.Visible;
			ThreadPool.QueueUserWorkItem( o => Refresh( ) );
		}

		void tmrState_Elapsed ( object sender, ElapsedEventArgs e )
		{
			RunOnUiThread( ( ) =>
				{
					LegacyBar.ProgressBarVisibility = ViewStates.Visible;
				} );
			ThreadPool.QueueUserWorkItem( o => Refresh( ) );
		}

		public Order GetOrderById ( string id )
		{
			return OrdersList.FirstOrDefault( currentOrder => currentOrder.ID == id );
		}

		private void Refresh ( )
		{
			var state = Requests.GetState( UserInfo.UserId );

			if ( state == "-50" )
			{
				OrdersList = Requests.GetOrdersList( UserInfo.UserId );
				UserInfo.Balance = Requests.GetBalance(UserInfo.UserId);

				RunOnUiThread( ( ) =>
					{
						if ( OrdersList != null )
						{
							lstOrders.Visibility = ViewStates.Visible;
							txtNoOrders.Visibility = ViewStates.Gone;
							lstOrders.Adapter = new OrdersListAdapter( this, OrdersList );
						}
						else
						{
							lstOrders.Visibility = ViewStates.Gone;
							txtNoOrders.Visibility = ViewStates.Visible;
						}

						LegacyBar.ProgressBarVisibility = ViewStates.Gone;
						LegacyBar.Title = "Баланс: " + UserInfo.Balance;
					} );
			}
			else
			{
				tmrState.Stop( );
				RunOnUiThread(() => StartActivity(typeof (AuthActivity)));
			}
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

		private void lstOrders_ItemClick ( object sender, AdapterView.ItemClickEventArgs e )
		{
			var selectedOrder = GetOrderById( e.View.Tag.ToString( ) );
			if ( selectedOrder != null )
			{
				UserInfo.CurrentOrder = selectedOrder;
				UserInfo.MustExit = false;
				tmrState.Stop( );
				StartActivity( typeof( OrderActivity ) );
			}
			else
			{
				Toast.MakeText( Application.Context, Resource.String.Error, ToastLength.Short ).Show( );
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
					ShowSetAreaDialog();
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

		public override bool OnKeyDown ( Keycode keyCode, KeyEvent e )
		{
			if ( keyCode == Keycode.Back )
			{
				UserInfo.MustExit = true;
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

			tmrState.Start( );
			LegacyBar.ProgressBarVisibility = ViewStates.Visible;
			ThreadPool.QueueUserWorkItem( o => Refresh( ) );
		}
	}
}

