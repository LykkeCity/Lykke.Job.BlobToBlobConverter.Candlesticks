using System;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.OutputModels
{
    public class OutCandlestick
    {
        public string AssetPair { get; set; }

        public bool IsAsk { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public DateTime Start { get; set; }

        public DateTime Finish { get; set; }
    }
}
