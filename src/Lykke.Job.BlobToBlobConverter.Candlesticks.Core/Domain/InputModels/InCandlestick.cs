using System;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.InputModels
{
    public class InCandlestick
    {
        private static int _maxStringFieldsLength = 255;

        public string AssetPair { get; set; }

        public bool IsAsk { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Open { get; set; }

        public double Close { get; set; }

        public DateTime Start { get; set; }

        public DateTime Finish { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(AssetPair) && AssetPair.Length <= _maxStringFieldsLength;
        }
    }
}
