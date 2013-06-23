using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RealTimeWeb.PureSSE.Code;

namespace RealTimeWeb.PureSSE.Controllers
{
    public class MarketController : Controller
    {
		[HttpPost]
		public void Open()
		{
			StockTicker.Instance.OpenMarket();
		}

		[HttpPost]
		public void Close()
		{
			StockTicker.Instance.CloseMarket();
		}

		[HttpPost]
		public void Reset()
		{
			StockTicker.Instance.Reset();
		}
		[HttpGet]
		public string State()
		{
			return StockTicker.Instance.MarketState.ToString();
		}
    }
}
