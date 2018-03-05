namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.OutputModels
{
    public class OutCandlestick
    {
        public string AssetPairId { get; set; }

        public bool IsAsk { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public string Start { get; set; }

        public string Finish { get; set; }
    }
}
