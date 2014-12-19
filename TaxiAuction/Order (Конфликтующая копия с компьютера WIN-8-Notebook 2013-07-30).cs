using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TaxiAuction
{
    public class Order
    {
        public Order(IEnumerable<XElement> elements)
        {
            foreach (var cElement in elements)
            {
                Waypoints.Add(cElement.Value);
            }
        }
        public string UID;
        public string ID;
        public string From;
        public string To;
        public string Time;
        public string Price;
        public string Phone;
        public string InCarTime;
        public List<string> Waypoints;
    }
}
