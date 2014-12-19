using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace TaxiAuction
{
    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleTask, Theme = "@style/AppTheme")]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            AddPreferencesFromResource(Resource.Xml.Preferences);
        }

		public override bool OnKeyDown ( Keycode keyCode, KeyEvent e )
		{
			if ( keyCode == Keycode.Back )
			{
				if ( PreferenceManager.GetDefaultSharedPreferences( this ).GetBoolean( "UseGPS", true ) )
				{
					StartService( new Intent( "ru.nwdgroup.geolocationservice" ) );
					Requests.SetMode( UserInfo.UserId, "1" );
					Finish( );
				}
				else
				{
					StopService( new Intent( "ru.nwdgroup.geolocationservice" ) );

					var dialog = new AlertDialog.Builder( this );

					dialog.SetTitle( Resource.String.ChooseArea );
					dialog.SetCancelable(true);
					dialog.SetAdapter( UserInfo.AreasArrayAdapter, ( sender, args ) => ThreadPool.QueueUserWorkItem( o =>
					{
						Requests.SetArea( UserInfo.UserId, UserInfo.AreasId [ args.Which ] );
						Requests.SetMode(UserInfo.UserId, "2");
						RunOnUiThread( Finish );
					} ) );

					dialog.Show( );
				}
				return true;
			}

			return base.OnKeyDown( keyCode, e );
		}
    }
}