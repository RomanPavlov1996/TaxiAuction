using System;
using System.Collections.Generic;
using System.Text;
using Android.Widget;
using Android.Views;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace TaxiAuction
{
	public class OrdersListAdapter : BaseAdapter<Order>
	{
		List<Order> items;
		Activity context;
		public OrdersListAdapter ( Activity context, List<Order> items )
			: base( )
		{
			this.context = context;
			this.items = items;
		}
		public override long GetItemId ( int position )
		{
			return position;
		}
		public override Order this [ int position ]
		{
			get { return items [ position ]; }
		}
		public override int Count
		{
			get { return items.Count; }
		}
		public override View GetView ( int position, View convertView, ViewGroup parent )
		{
			var item = items [ position ];
			View view = convertView;
			if ( view == null ) // no view to re-use, create new
				view = context.LayoutInflater.Inflate( Resource.Layout.ListItem, null );
			view.FindViewById<TextView>( Resource.Id.lblID ).Text = "#" + item.UID;
			view.FindViewById<TextView>( Resource.Id.lblFrom ).Text = item.From;
			view.FindViewById<TextView>( Resource.Id.lblTo ).Text = item.To;
			view.FindViewById<TextView>( Resource.Id.lblTime ).Text = item.Time;
			view.FindViewById<TextView>( Resource.Id.lblPrice ).Text = item.Price + " руб.";
			view.Tag = item.ID;

			return view;
		}
	}
}
