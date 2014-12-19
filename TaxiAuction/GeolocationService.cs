using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Xamarin.Geolocation;

[assembly: UsesPermission( Name = "android.permission.INTERNET" )]
[assembly: UsesPermission( Name = "android.permission.WAKE_LOCK" )]
[assembly: UsesPermission( Name = "android.permission.ACCESS_COARSE_LOCATION" )]
[assembly: UsesPermission( Name = "android.permission.ACCESS_FINE_LOCATION" )]

namespace TaxiAuction
{
	[Service]
	[IntentFilter( new [ ] { "ru.nwdgroup.geolocationservice" } )]
	public class GeolocationService : Service
	{
		private const string TAG = "TaxiAuction-GLS";
		private Geolocator _geolocator;
		private string _userId;
		GeolocationServiceBinder _binder;

		private void Setup ( )
		{
			if ( this._geolocator != null )
				return;

			this._geolocator = new Geolocator( this ) { DesiredAccuracy = 50 };
			this._geolocator.PositionChanged += OnPositionChanged;
		}

		public void StartListening ( )
		{
			Setup( );

			if ( !_geolocator.IsGeolocationAvailable || !_geolocator.IsGeolocationEnabled )
			{
				Log.Error( TAG, "Geolocation is unavailable" );

				Toast.MakeText( this, "Geolocation is unavailable", ToastLength.Long ).Show( );
				return;
			}

			if ( !_geolocator.IsListening )
			{
				_geolocator.StartListening( 5000, 0 );
				Log.Debug( TAG, "Listening started" );
			}
		}

		public void StopListening ( )
		{
			Setup( );
			this._geolocator.StopListening( );
			Log.Debug( TAG, "Listening stopped" );
		}

		private void OnPositionChanged ( object sender, PositionEventArgs e )
		{
			Requests.SendLocation( _userId, e.Position.Latitude, e.Position.Longitude, e.Position.Accuracy, e.Position.Speed );
		}

		public override StartCommandResult OnStartCommand ( Android.Content.Intent intent, StartCommandFlags flags, int startId )
		{
			Log.Debug( TAG, "GeolocationService started" );

			this._userId = GetSharedPreferences( Application.Context.PackageName, FileCreationMode.Private ).GetString( "UserId", null );
			StartListening( );

			var ongoing = new Notification( Resource.Drawable.Icon, GetString( Resource.String.PendingIntentTitle ) );
			var pendingIntent = PendingIntent.GetActivity( this, 0, new Intent( this, typeof( AuthActivity ) ), 0 );
			ongoing.SetLatestEventInfo( this, GetString( Resource.String.ApplicationName ), GetString( Resource.String.PendingIntentTitle ), pendingIntent );

			StartForeground( ( int ) NotificationFlags.ForegroundService, ongoing );

			return StartCommandResult.Sticky;
		}

		public override void OnDestroy ( )
		{
			base.OnDestroy( );

			StopListening( );

			Log.Debug( TAG, "GeolocationService stopped" );
		}

		public override IBinder OnBind ( Intent intent )
		{
			_binder = new GeolocationServiceBinder( this );
			return _binder;
		}

		public class GeolocationServiceBinder : Binder
		{
			readonly GeolocationService _service;

			public GeolocationServiceBinder ( GeolocationService service )
			{
				this._service = service;
			}

			public GeolocationService GetGeolocationService ( )
			{
				return _service;
			}
		}
	}
}
