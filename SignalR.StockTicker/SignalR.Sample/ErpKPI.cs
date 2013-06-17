using System;

namespace Microsoft.AspNet.SignalR.StockTicker
{

    //Stock
    public class ErpKPI
    {
        private decimal _amount;

        public string Channel { get; set; }

        public string Type { get; set; }
        
        public decimal Total { get; private set; }
        
        public int NumberOf { get; private set; }

        public decimal Last { get; private set; }
        
        public decimal Largest { get; private set; }

        public decimal Smallest { get; private set; }

        public decimal Average { get; private set; }

        public ErpKPI()
        {
            NumberOf = -1; //initialize NumberOf
        }


        public decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
                Last = _amount;
                Largest = Math.Max(Largest, _amount);
                Smallest = Smallest == 0 ? _amount : Math.Min(Smallest, _amount);
                Total += _amount;
                NumberOf += 1;
                Average = Total / Math.Max(1,NumberOf);
            }
        }
    }

    public class ERPRequest
    {
        public string CallerId { get; set; }
        public string RequestString { get; set; }
    }
}
