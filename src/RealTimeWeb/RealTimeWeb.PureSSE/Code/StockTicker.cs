using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;
using RealTimeWeb.Core;

namespace RealTimeWeb.PureSSE.Code
{
    public class StockTicker
    {
		private static readonly ConcurrentBag<StreamWriter> _clients = new ConcurrentBag<StreamWriter>();
		public ConcurrentBag<StreamWriter> Clients
		{
			get { return _clients; }
		}
		/// <summary>
		/// The pattern used for publishing messages
		/// </summary>
		private const string eventPattern = "event: {0}\ndata: {1}\n\n";

        // Singleton instance
        private readonly static Lazy<StockTicker> _instance = new Lazy<StockTicker>(
            () => new StockTicker());

        private readonly object _marketStateLock = new object();
        private readonly object _updateStockPricesLock = new object();

        private readonly ConcurrentDictionary<string, Stock> _stocks = new ConcurrentDictionary<string, Stock>();
        
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
       
        private Timer _timer;
        private volatile bool _updatingStockPrices;
        private volatile MarketState _marketState;

        private StockTicker()
        {
            LoadDefaultStocks();
        }

        public static StockTicker Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public MarketState MarketState
        {
            get { return _marketState; }
            private set { _marketState = value; }
        }

        public IEnumerable<Stock> GetAllStocks()
        {
            return _stocks.Values;
        }

        public void OpenMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState != MarketState.Open)
                {
                    _timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);

                    MarketState = MarketState.Open;

                    BroadcastMarketStateChange(MarketState.Open);
                }
            }
        }

        public void CloseMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState == MarketState.Open)
                {
                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }

                    MarketState = MarketState.Closed;

                    BroadcastMarketStateChange(MarketState.Closed);
                }
            }
        }

        public void Reset()
        {
            lock (_marketStateLock)
            {
                if (MarketState != MarketState.Closed)
                {
                    throw new InvalidOperationException("Market must be closed before it can be reset.");
                }
                
                LoadDefaultStocks();
                BroadcastMarketReset();
            }
        }

        private void LoadDefaultStocks()
        {
            _stocks.Clear();
			var stocks = StockFixtures.GetDefaultStocks();
            stocks.ForEach(stock => _stocks.TryAdd(stock.Symbol, stock));
        }

        private void UpdateStockPrices(object state)
        {
            // This function must be re-entrant as it's running as a timer interval handler
            lock (_updateStockPricesLock)
            {
                if (!_updatingStockPrices)
                {
                    _updatingStockPrices = true;

                    foreach (var stock in _stocks.Values)
                    {
                        if (stock.TryUpdatePrice())
                        {
                            BroadcastStockPrice(stock);
                        }
                    }
                    _updatingStockPrices = false;
                }
            }
        }

        private void BroadcastMarketStateChange(MarketState marketState)
        {
            switch (marketState)
            {
                case MarketState.Open:
					Publish("marketOpened", "fdsafdsafdsfdfds");
                    break;
                case MarketState.Closed:
					Publish("marketClosed", "fdsafdsafdsafds");
                    break;
                default:
                    break;
            }
        }

		private void Publish(string eventName, string message)
		{
			foreach (var subscriber in _clients)
			{
				subscriber.Write(eventPattern, eventName, message);
				try
				{
					subscriber.Flush();
				}
				catch(Exception e)
				{
					MvcApplication.Logger.Log(e.ToString(), "EXCEPTION");
				}
			}
		}


        private void BroadcastMarketReset()
        {
			Publish("marketReset", string.Empty);
        }

        private void BroadcastStockPrice(Stock stock)
        {
			Publish("updateStockPrice", stock.ToJson());
        }

    }

    public enum MarketState
    {
        Closed,
        Open
    }
}