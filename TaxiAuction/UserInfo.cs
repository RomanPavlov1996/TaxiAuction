using System;
using System.Collections.Generic;
using System.Text;
using Android.Widget;

namespace TaxiAuction
{
	public static class UserInfo
	{
		public static string UserId;
		public static string License;
		public static string PushId;
		public static StateTypes State;
		public static Order CurrentOrder;
		public static bool MustExit;
        public static string MagicNumber;
	    public static bool UseMockLocation;
	    public static ArrayAdapter<String> AreasArrayAdapter;
		public static List<int> AreasId = new List<int>();
		public static string Area;
		public static string Balance;
	}
}
