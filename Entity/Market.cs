﻿using System;
using System.Collections.Generic;

namespace Entity
{
    public partial class Market
    {
        public short Id { get; set; }
        public short TradeCurrencyId { get; set; }
        public short QuoteCurrencyId { get; set; }
        public DateTime TradingStart { get; set; }
        public DateTime TradingEnd { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public long? TradeMinQuantity { get; set; }
        public decimal? MinRate { get; set; }
        public decimal? MaxRate { get; set; }
        public bool NewOrderAllowed { get; set; }
        public bool CancelAllowed { get; set; }

        public virtual Currency QuoteCurrency { get; set; }
        public virtual Currency TradeCurrency { get; set; }
    }
}
