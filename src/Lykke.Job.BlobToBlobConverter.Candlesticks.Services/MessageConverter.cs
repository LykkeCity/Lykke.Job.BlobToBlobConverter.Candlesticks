using System;
using System.Linq;
using System.Collections.Generic;
using Common;
using Common.Log;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.InputModels;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.OutputModels;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Services
{
    public class MessageConverter : IMessageConverter
    {
        private const string _mainContainer = "candles";

        private readonly ILog _log;

        public MessageConverter(ILog log)
        {
            _log = log;
        }

        public Dictionary<string, List<string>> Convert(List<string> messages)
        {
            var result = new Dictionary<string, List<string>>
            {
                { _mainContainer, new List<string>() },
            };

            var candlesDict = new Dictionary<DateTime, OutCandlestick>();

            foreach (var message in messages)
            {
                var candlestick = message.DeserializeJson<InCandlestick>();
                if (!candlestick.IsValid())
                    _log.WriteWarning(nameof(MessageConverter), nameof(Convert), $"Candlestick {candlestick.ToJson()} is invalid!");

                var candle = new OutCandlestick
                {
                    AssetPair = candlestick.AssetPair,
                    IsAsk = candlestick.IsAsk,
                    High = (decimal)candlestick.High,
                    Low = (decimal)candlestick.Low,
                    Open = (decimal)candlestick.Open,
                    Close = (decimal)candlestick.Close,
                    Start = candlestick.Start,
                    Finish = candlestick.Finish,
                };

                var start = candle.Start.RoundToMinute();
                if (candlesDict.ContainsKey(start))
                    candlesDict[start] = Merge(candlesDict[start], candle);
                else
                    candlesDict.Add(start, candle);
            }

            result[_mainContainer] = new List<string>(candlesDict.Values.Select(i => i.ToJson()));

            return result;
        }

        private OutCandlestick Merge(OutCandlestick oldItem, OutCandlestick newItem)
        {
            if (newItem.High > oldItem.High)
                oldItem.High = newItem.High;
            if (newItem.Low < oldItem.Low)
                oldItem.Low = newItem.Low;
            if (newItem.Start > oldItem.Start)
            {
                oldItem.Close = newItem.Close;
                oldItem.Finish = newItem.Finish;
            }
            else if (newItem.Start < oldItem.Start)
            {
                oldItem.Open = newItem.Open;
                oldItem.Start = newItem.Start;
            }
            return oldItem;
        }
    }
}
