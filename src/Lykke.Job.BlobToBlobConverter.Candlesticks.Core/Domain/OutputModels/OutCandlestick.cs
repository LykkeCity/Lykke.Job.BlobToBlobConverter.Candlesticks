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

        public override string ToString()
        {
            return $"{nameof(AssetPairId)},{AssetPairId},{nameof(IsAsk)},{IsAsk},{nameof(High)},{High},{nameof(Low)},{Low}"
                + $",{nameof(Open)},{Open},{nameof(Close)},{Close},{nameof(Start)},{Start},{nameof(Finish)},{Finish}";
        }

        public static string GetColumns()
        {
            return $"{nameof(AssetPairId)},{nameof(IsAsk)},{nameof(High)},{nameof(Low)}"
                + $",{nameof(Open)},{nameof(Close)},{nameof(Start)},{nameof(Finish)}";
        }
    }
}
