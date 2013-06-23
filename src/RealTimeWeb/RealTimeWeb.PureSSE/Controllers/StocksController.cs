using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RealTimeWeb.Core;
using RealTimeWeb.PureSSE.Code;

namespace RealTimeWeb.PureSSE.Controllers
{
    public class StocksController : ApiController
    {
        // GET api/stocks
        public IEnumerable<Stock> Get()
        {
			return StockTicker.Instance.GetAllStocks();
        }
    }
}
