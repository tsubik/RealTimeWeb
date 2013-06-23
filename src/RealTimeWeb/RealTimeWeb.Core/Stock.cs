using System;
using System.Web.Script.Serialization;

namespace RealTimeWeb.Core
{
    public class Stock
    {
		private static readonly Random _updateOrNotRandom = new Random();
		// Stock can go up or down by a percentage of this factor on each change
		private readonly double _rangePercent = 0.002;

        private decimal _price;

        public string Symbol { get; set; }
        
        public decimal DayOpen { get; private set; }
        
        public decimal DayLow { get; private set; }
        
        public decimal DayHigh { get; private set; }

        public decimal LastChange { get; private set; }

        public decimal Change
        {
            get
            {
                return Price - DayOpen;
            }
        }

        public double PercentChange
        {
            get
            {
                return (double)Math.Round(Change / Price, 4);
            }
        }

        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price == value)
                {
                    return;
                }
                
                LastChange = value - _price;
                _price = value;
                
                if (DayOpen == 0)
                {
                    DayOpen = _price;
                }
                if (_price < DayLow || DayLow == 0)
                {
                    DayLow = _price;
                }
                if (_price > DayHigh)
                {
                    DayHigh = _price;
                }
            }
        }

		public bool TryUpdatePrice()
		{
			// Randomly choose whether to udpate this stock or not
			var r = _updateOrNotRandom.NextDouble();
			if (r > 0.1)
			{
				return false;
			}

			// Update the stock price by a random factor of the range percent
			var random = new Random((int)Math.Floor(Price));
			var percentChange = random.NextDouble() * _rangePercent;
			var pos = random.NextDouble() > 0.51;
			var change = Math.Round(Price * (decimal)percentChange, 2);
			change = pos ? change : -change;

			Price += change;
			return true;
		}

		public string ToJson()
		{
			return new JavaScriptSerializer().Serialize(this);
		}
    }
}
