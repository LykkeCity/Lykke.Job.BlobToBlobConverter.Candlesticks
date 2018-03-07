using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Common;
using Common.Log;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
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

        public Dictionary<string, List<string>> Convert(IEnumerable<byte[]> messages)
        {
            var result = new Dictionary<string, List<string>>
            {
                { _mainContainer, new List<string>() },
            };

            var candlesDict = new Dictionary<DateTime, OutCandlestick>();

            foreach (var message in messages)
            {
                var candleEvent = MessagePackSerializer.Deserialize<CandlesUpdatedEvent>(message);

                foreach (var candle in candleEvent.Candles)
                {
                    if (candle.TimeInterval != CandleTimeInterval.Minute)
                        continue;

                    if (candle.PriceType != CandlePriceType.Ask && candle.PriceType != CandlePriceType.Bid)
                        continue;

                    var candlestick = new OutCandlestick
                    {
                        AssetPairId = candle.AssetPairId,
                        IsAsk = candle.PriceType == CandlePriceType.Ask,
                        High = (decimal)candle.High,
                        Low = (decimal)candle.Low,
                        Open = (decimal)candle.Open,
                        Close = (decimal)candle.Close,
                        Start = DateTimeConverter.Convert(candle.CandleTimestamp),
                        Finish = DateTimeConverter.Convert(candle.ChangeTimestamp),
                    };

                    var start = candle.CandleTimestamp;
                    if (candlesDict.ContainsKey(start))
                        candlesDict[start] = Merge(candlesDict[start], candlestick);
                    else
                        candlesDict.Add(start, candlestick);
                }
            }

            result[_mainContainer] = new List<string>(candlesDict.Values.Select(i => i.ToString()));

            return result;
        }

        private OutCandlestick Merge(OutCandlestick oldItem, OutCandlestick newItem)
        {
            if (newItem.High > oldItem.High)
                oldItem.High = newItem.High;
            if (newItem.Low < oldItem.Low)
                oldItem.Low = newItem.Low;
            if (newItem.Start.CompareTo(oldItem.Start) > 0)
            {
                oldItem.Close = newItem.Close;
                oldItem.Finish = newItem.Finish;
            }
            else if (newItem.Start.CompareTo(oldItem.Start) < 0)
            {
                oldItem.Open = newItem.Open;
                oldItem.Start = newItem.Start;
            }
            return oldItem;
        }
    }
}
