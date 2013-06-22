using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.StockTicker;

namespace RealTimeWeb.Core
{
	public class StockFixtures
	{
		public static List<Stock> GetDefaultStocks()
		{
			var stocks = new List<Stock>
            {
                new Stock { Symbol = "MSFT", Price = 30.31m },
                new Stock { Symbol = "APPL", Price = 578.18m },
                new Stock { Symbol = "GOOG", Price = 570.30m }
            };
			return stocks;
		}
	}
}
