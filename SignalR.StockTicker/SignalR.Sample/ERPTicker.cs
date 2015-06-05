using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR.Hubs;

namespace Microsoft.AspNet.SignalR.StockTicker
{
    public class ERPTicker
    {
        // Singleton instance holds ticker information in memory
        private readonly static Lazy<ERPTicker> _instance = new Lazy<ERPTicker>(
            () => new ERPTicker(GlobalHost.ConnectionManager.GetHubContext<ERPTickerHub>().Clients));

        private readonly object _updateKPIsLock = new object();

        private readonly ConcurrentDictionary<string, ErpKPI> _KPIs = new ConcurrentDictionary<string, ErpKPI>();
        /*
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        private readonly Random _updateOrNotRandom = new Random();
        */
        private volatile bool _updatingKPIs;
        
        private string _erpResponderId;

        private ERPTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            LoadDefaultKPIs();
        }

        public static ERPTicker Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public IEnumerable<ErpKPI> GetAllKPIs(string channel)
        {
            List<ErpKPI> result = new List<ErpKPI>();

            foreach (ErpKPI kpi in _KPIs.Values)
            {
                if (kpi.Channel == channel)
                {
                   result.Add(kpi);
                }
            }

            //return _KPIs.Values;
            return result;
        }

        
        private void LoadDefaultKPIs()
        {
            _KPIs.Clear();

            var KPIs = new List<ErpKPI>
            {
                new ErpKPI { Channel = "Sales", Type = "Quotes", Amount = 0m },
                new ErpKPI { Channel = "Sales",Type = "Orders", Amount = 0m },
                new ErpKPI { Channel = "Sales",Type = "Invoices", Amount = 0m },
                new ErpKPI { Channel = "CashFlow",Type = "Cash Receipts", Amount = 0m },
                new ErpKPI { Channel = "CashFlow",Type = "Manual Checks", Amount = 0m },
                new ErpKPI { Channel = "CashFlow",Type = "AP Checks", Amount = 0m },
                new ErpKPI { Channel = "Expense",Type = "AP Invoices", Amount = 0m },
                new ErpKPI { Channel = "Expense",Type = "PO Receipts", Amount = 0m },
                new ErpKPI { Channel = "Expense",Type = "Purchase Orders", Amount = 0m },
            };

            KPIs.ForEach(KPI => _KPIs.TryAdd(KPI.Type, KPI));
                   
        }

/*
        public void UpdateSalesKPI(ErpKPI kpi)
        {
            lock (_updateKPIsLock)
            {
                if (!_updatingKPIs)
                {
                    ErpKPI newKPI;
                    _KPIs.TryGetValue(kpi.Type, out newKPI);
                    newKPI.Amount = kpi.Amount;


                    BroadcastSalesKPI(newKPI);
                }
                _updatingKPIs = false;
            }
        }

        public void UpdateCashFlowKPI(ErpKPI kpi)
        {
            lock (_updateKPIsLock)
            {
                if (!_updatingKPIs)
                {
                    ErpKPI newKPI;
                    _KPIs.TryGetValue(kpi.Type, out newKPI);
                    newKPI.Amount = kpi.Amount;


                    BroadcastCashFlowKPI(newKPI);
                }
                _updatingKPIs = false;
            }
        }
*/
        public void UpdateKPI(ErpKPI kpi)
        {
            lock (_updateKPIsLock)
            {
                if (!_updatingKPIs)
                {
                    ErpKPI newKPI;
                    if (_KPIs.TryGetValue(kpi.Type, out newKPI))
                    {
                        newKPI.Amount = kpi.Amount;
                        BroadcastKPI(newKPI);
                    }
                    else
                    {
                        //add new kpi type
                        newKPI = new ErpKPI { Channel = kpi.Channel, Type = kpi.Type, Amount = 0m };
                        newKPI.Amount = kpi.Amount; //perhaps need to broadcast to Add so javascript can append?
                        _KPIs.TryAdd(newKPI.Type, newKPI);
                        BroadcastNewKPI(newKPI);

                    }

                }
                _updatingKPIs = false;
            }
        }
        
        public void AddTickerItem(string channel, string tickerText)
        {
            Clients.AllExcept(_erpResponderId).addTickerItem(channel, tickerText);
        }

        public void Reset()
        {
            Clients.All.reset();
            lock (_updateKPIsLock)
            {
                LoadDefaultKPIs();
                foreach(KeyValuePair<string, ErpKPI> kpi in _KPIs)
                {
                    BroadcastKPI(kpi.Value);
                }
                
            }
        }

        private void BroadcastKPI(ErpKPI kpi)
        {
            switch (kpi.Channel)
            {
                case "Sales":
                    BroadcastSalesKPI(kpi);
                    break;

                case "CashFlow":
                    BroadcastCashFlowKPI(kpi);
                    break;

                case "Expense":
                    BroadcastExpenseKPI(kpi);
                    break;

                default:
                    break;
            }
        }

        private void BroadcastNewKPI(ErpKPI kpi)
        {
            switch (kpi.Channel)
            {
                case "Sales":
                    Clients.All.addSalesKPI(kpi);
                    break;

                case "CashFlow":
                    Clients.All.addCashFlowKPI(kpi);
                    break;

                case "Expense":
                    Clients.All.addExpenseKPI(kpi);
                    break;

                default:
                    break;
            }
        }

        private void BroadcastSalesKPI(ErpKPI kpi)
        {
            Clients.All.updateSalesKPI(kpi);
        }

        private void BroadcastCashFlowKPI(ErpKPI kpi)
        {
            Clients.All.updateCashFlowKPI(kpi);
        }

        private void BroadcastExpenseKPI(ErpKPI kpi)
        {
            Clients.All.updateExpenseKPI(kpi);
        }

        //could/should be in a different singleton/hub
        public void RegisterRequestHandler(string id)
        {
            _erpResponderId = id;
            // need some way to secure this to really know it is the client.  Perhaps a secret that only the .dll knows??
        }
        public void SendRequest(string id, string requestString)
        {
            if (_erpResponderId != null)
            {
                //this will fire off an event on the client to handle the request
                var erpRequest = new ERPRequest();
                erpRequest.CallerId = id;
                erpRequest.RequestString = requestString;
                Clients.Client(_erpResponderId).IncomingRequest(erpRequest); //_erpResponderId is the connectionID of the on-premises SignalRClient for the responder set somewhere

            }
        }


        public void RequestResponse(string id, string response)
        {
            Clients.Client(id).AddResponse(response); //send response to connection that made request
        }


    }


}