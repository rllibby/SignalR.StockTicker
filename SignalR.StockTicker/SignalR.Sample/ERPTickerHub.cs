using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace Microsoft.AspNet.SignalR.StockTicker
{
    [HubName("erpTicker")]
    public class ERPTickerHub : Hub
    {
        private readonly ERPTicker _erpTicker;
        //private string _erpResponderId;

        public ERPTickerHub() : this(ERPTicker.Instance) { }

        public ERPTickerHub(ERPTicker erpTicker)
        {
            _erpTicker = erpTicker;
        }

        public IEnumerable<ErpKPI> GetAllKPIs(string channel)
        {
            return _erpTicker.GetAllKPIs(channel);
        }

        public void UpdateKPI(ErpKPI kpi)
        {
            _erpTicker.UpdateKPI(kpi);
        }

        public void AddTickerItem(string channel, string tickerText)
        {
            _erpTicker.AddTickerItem(channel, tickerText);
        }

        public void Reset()
        {
            _erpTicker.Reset();
        }

        //Below methods are used for the generic text request object
        // these should be refactored to use another hub

        // id will be the connectionId for the on-premises request handler
        public void RegisterRequestHandler(string id)
        {
            _erpTicker.RegisterRequestHandler(id);
        }
        public void SendRequest(string id, string requestString)
        {
            _erpTicker.SendRequest(id, requestString);
        }


        public void RequestResponse(string id, string response)
        {
            _erpTicker.RequestResponse(id, response);
        }

    }
}