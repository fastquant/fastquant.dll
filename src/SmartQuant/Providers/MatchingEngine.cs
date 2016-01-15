using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuant
{
    public class MatchingEngine : Provider, IExecutionProvider, IExecutionSimulator
    {
        public ICommissionProvider CommissionProvider { get; set; } = new CommissionProvider();

        public ISlippageProvider SlippageProvider { get; set; } = new SlippageProvider();

        public BarFilter BarFilter { get; } = new BarFilter();

        public bool FillOnQuote
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillOnTrade
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillOnBar
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillOnBarOpen
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillOnLevel2
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillMarketOnNext
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillLimitOnNext
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillStopOnNext
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillAtStopPrice
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillStopLimitOnNext
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool FillAtLimitPrice
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PartialFills
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Queued
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public MatchingEngine(Framework framework):base(framework)
        {
        }

        public override void Connect()
        {
            Status = ProviderStatus.Connected;
        }

        public override void Disconnect()
        {
            Status = ProviderStatus.Disconnected;
        }

        public void OnBid(Bid bid)
        {
            throw new NotImplementedException();
        }

        public void OnAsk(Ask ask)
        {
            throw new NotImplementedException();
        }

        public void OnLevel2(Level2Snapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public void OnLevel2(Level2Update update)
        {
            throw new NotImplementedException();
        }

        public void OnTrade(Trade trade)
        {
            throw new NotImplementedException();
        }

        public void OnBarOpen(Bar bar)
        {
            throw new NotImplementedException();
        }

        public void OnBar(Bar bar)
        {
            throw new NotImplementedException();
        }
    }
}
