using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaxiAuction;

namespace TaxiAuction.Test
{
	[TestClass( )]
	public class RequestsTests
	{
		[TestMethod( )]
		public void CheckUpdatesTest ( )
		{
			Assert.AreEqual(Requests.CheckUpdates(), "2");
		}
	}
}
