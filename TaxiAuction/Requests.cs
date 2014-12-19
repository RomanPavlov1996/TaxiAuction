using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Android.Content;
using Android.Locations;
using Android.Widget;

namespace TaxiAuction
{
	public enum StateTypes
	{
		Free,
		ActiveOrder,
		Arrived,
		PersonInCar
	};

	public class Requests
	{
		const string ApiUrl = "http://api.taxiauction.ru/api/";

		private static string Get(string url, string data)
		{
			try
			{
				var req = WebRequest.Create(url + "?" + data);
				var resp = req.GetResponse();
				var stream = resp.GetResponseStream();
				var sr = new StreamReader(stream);
				var Out = sr.ReadToEnd();
				sr.Close();
				return Out;
			}
			catch
			{
				return "0";
			}
		}

		public static string Authorize(string login, string password, string pushId)
		{
			try
			{
				const string requestUrl = ApiUrl + "Auth";
				var passwordHash = Crypto.GetMD5( password );
				var data = String.Format( "login={0}&pass={1}&PushId={2}", login, passwordHash, pushId );
				var response = Get( requestUrl, data );
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static string GetState(string userId)
		{
			try
			{
				const string requestUrl = ApiUrl + "GetState";
				var data = String.Format("user_id={0}", userId);
				var response = Get(requestUrl, data);
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static string GetMagicNumber ( )
		{
			try
			{
				const string requestUrl = ApiUrl + "GetMagicNumber";
				var response = Get( requestUrl, null );
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static string Logout ( string userId )
		{
			try
			{
				const string requestUrl = ApiUrl + "Logout";
				var data = String.Format( "user_id={0}", userId );
				var response = Get( requestUrl, data );
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static string SendLocation(
			string userId,
			double latitude,
			double longitude,
			double accuracy,
			double speed)
		{
			try
			{
				const string requestUrl = ApiUrl + "GPS";
				var data = String.Format("user_id={0}&lat={1}&lon={2}&acc={3}&spd={4}", userId, latitude, longitude, accuracy,
				                            speed);
				Get(requestUrl, data);
				var response = Get(requestUrl, data);
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static List<Order> GetOrdersList(string userId)
		{
			try
			{
				const string requestUrl = ApiUrl + "GetMainList";
				var data = String.Format("user_id={0}", userId);
				var response = Get(requestUrl, data);

				var ordersXDocument = XDocument.Parse(response);

			    var ordersList = new List<Order>();

                foreach (var cElement in ordersXDocument.Root.Elements())
                {
                    var cOrder = new Order()
                        {
                            UID = cElement.Element("uID").Value,
                            ID = cElement.Element("id").Value,
                            From = cElement.Element("from").Value,
                            To = cElement.Element("to").Value,
                            Time = cElement.Element("time").Value,
                            Price = cElement.Element("price").Value,
                            Phone = cElement.Element("phone").Value,
                            InCarTime = cElement.Element("incartime").Value
                        };

                    if (cElement.Element("waypoints").HasElements)
                    {
                        foreach (var cWaypoint in cElement.Element("waypoints").Elements())
                        {
                            cOrder.Waypoints.Add(cWaypoint.Value);
                        }
                    }

                    ordersList.Add(cOrder);
                }

			    return ordersList;
			}
			catch
			{
				return null;
			}
		}

		public static string TakeOrder(string userId, string orderId)
		{
			try
			{
				const string requestUrl = ApiUrl + "TakeOrder";
				var data = String.Format("user_id={0}&order_id={1}", userId, orderId);
				var response = Get(requestUrl, data);
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static string ReportArrived(string userId, string orderId)
		{
			try
			{
				const string requestUrl = ApiUrl + "ReportOnClientAddress";
				var data = String.Format("user_id={0}&order_id={1}", userId, orderId);
				var response = Get(requestUrl, data);
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static string ReportPersonInCar(string userId, string orderId)
		{
			try
			{
				const string requestUrl = ApiUrl + "ReportInCar";
				var data = String.Format("user_id={0}&order_id={1}", userId, orderId);
				var response = Get(requestUrl, data);
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static string ReportCompleted(string orderId)
		{
			try
			{
				const string requestUrl = ApiUrl + "ReportOrderCompleted";
				var data = String.Format("order_id={0}", orderId);
				var response = Get(requestUrl, data);
				return response;
			}
			catch
			{
				return "0";
			}
		}

		public static Order GetActiveOrder(string userId)
		{
			try
			{
				const string requestUrl = ApiUrl + "GetOrderActive";
				var data = String.Format("user_id={0}", userId);
				var response = Get(requestUrl, data);

				var ordersXDocument = XDocument.Parse(response);
				var orderElement = ordersXDocument.Root;

				var order = new Order
					{
						UID = orderElement.Element("uID").Value,
						ID = orderElement.Element("id").Value,
						From = orderElement.Element("from").Value,
						To = orderElement.Element("to").Value,
						Time = orderElement.Element("time").Value,
						Price = orderElement.Element("price").Value,
						Phone = orderElement.Element("phone").Value,
						InCarTime = orderElement.Element( "incartime" ).Value,
						Accepted = orderElement.Element( "accepted" ).Value
					};

				return order;
			}
			catch
			{
				return null;
			}
		}

		public static Order GetActiveOrderInCar(string userId)
		{
			try
			{
				const string requestUrl = ApiUrl + "GetOrderActiveInCar";
				var data = String.Format("user_id={0}", userId);
				var response = Get(requestUrl, data);

				var ordersXDocument = XDocument.Parse(response);
				var orderElement = ordersXDocument.Root;

				var order = new Order
					{
						UID = orderElement.Element("uID").Value,
						ID = orderElement.Element("id").Value,
						From = orderElement.Element("from").Value,
						To = orderElement.Element("to").Value,
						Time = orderElement.Element("time").Value,
						Price = orderElement.Element("price").Value,
						Phone = orderElement.Element("phone").Value,
						InCarTime = orderElement.Element( "incartime" ).Value,
						Accepted = orderElement.Element( "accepted" ).Value
					};

				return order;
			}
			catch
			{
				return null;
			}
		}

        public static string GetBalance(string userId)
        {
            try
            {
                const string requestUrl = ApiUrl + "GetBalance";
                var data = String.Format("user_id={0}", userId);
                var response = Get(requestUrl, data);
                return response;
            }
            catch
            {
                return "0";
            }
        }

        public static string SetArea(string userId, int areaId)
        {
            try
            {
                const string requestUrl = ApiUrl + "setArea"; 
                var data = String.Format("user_id={0}&areaId={1}", userId, areaId);
                var response = Get(requestUrl, data);
                return response;
            }
            catch
            {
                return "0";
            }
        }

        public static ArrayAdapter<String> GetAreas(Context context)
        {
            try
            {
                const string requestUrl = ApiUrl + "GetAreas";
                var response = Get(requestUrl, "");

                var ordersXDocument = XDocument.Parse(response);

                var areas = new ArrayAdapter<String>(context, Android.Resource.Layout.SelectDialogSingleChoice);

                foreach (var cElement in ordersXDocument.Root.Elements())
                {
	                areas.Add(cElement.Element("title").Value);

					UserInfo.AreasId.Add(int.Parse(cElement.Element("ID").Value));
                }

                return areas;
            }
            catch
            {
                return null;
            }
        }

		public static string AcceptOrder ( string driverId, string orderId )
		{
			try
			{
				const string requestUrl = ApiUrl + "acceptOrder";
				var data = String.Format( "orderId={0}&driverId={1}", orderId, driverId );
				var response = Get( requestUrl, data );

				return "1";
			}
			catch
			{
				return "0";
			}
		}

		public static string DeclineOrder ( string driverId, string orderId )
		{
			try
			{
				const string requestUrl = ApiUrl + "declineOrder";
				var data = String.Format( "orderId={0}&driverId={1}", orderId, driverId );
				var response = Get( requestUrl, data );

				return "1";
			}
			catch
			{
				return "0";
			}
		}

		public static string GetMode ( string userId )
		{
			try
			{
				const string requestUrl = ApiUrl + "getMode";
				var data = String.Format( "driverId={0}", userId );
				var response = Get( requestUrl, data );
				return response;
			}
			catch
			{
				return "1";
			}
		}

		public static string SetMode ( string userId, string mode )
		{
			try
			{
				const string requestUrl = ApiUrl + "setMode";
				var data = String.Format( "driverId={0}&mode={1}", userId, mode );
				var response = Get( requestUrl, data );
				return response;
			}
			catch
			{
				return "1";
			}
		}

		public static string GetArea ( string userId )
		{
			try
			{
				const string requestUrl = ApiUrl + "getArea";
				var data = String.Format( "driverId={0}", userId );
				var response = Get( requestUrl, data );
				return response;
			}
			catch
			{
				return "0";
			}
		}
	}
}