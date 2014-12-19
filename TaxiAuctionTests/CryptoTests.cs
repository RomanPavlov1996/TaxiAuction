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
	public class CryptoTests
	{
		[TestMethod( )]
		public void GetMD5Test ( )
		{
			Assert.IsNotNull(Crypto.GetMD5("abc"));
		}
	}
}
