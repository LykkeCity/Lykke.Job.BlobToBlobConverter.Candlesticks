﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MessagePack;
using Common;
using Common.Log;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.OutputModels;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Services
{
    [UsedImplicitly]
    public class MessageProcessor : IMessageProcessor<CandlesUpdatedEvent>
    {
        private const string _mainContainer = "candles";
        private const int _maxStringFieldsLength = 255;

        private readonly ILog _log;

        public MessageProcessor(ILog log)
        {
            _log = log;
        }

        public Dictionary<string, string> GetMappingStructure()
        {
            var result = new Dictionary<string, string>
            {
                { _mainContainer, OutCandlestick.GetColumnsString() },
            };
            return result;
        }

        public bool TryDeserialize(byte[] data, out CandlesUpdatedEvent result)
        {
            try
            {
                result = MessagePackSerializer.Deserialize<CandlesUpdatedEvent>(data);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public async Task ProcessAsync(IEnumerable<CandlesUpdatedEvent> messages, Func<string, IEnumerable<string>, Task> processTask)
        {
            var candlesDict = new Dictionary<string, Dictionary<DateTime, OutCandlestick>>();

            foreach (var candleEvent in messages)
            {
                if (!IsValid(candleEvent))
                    _log.WriteWarning(nameof(MessageProcessor), nameof(Convert), $"CandleEvent {candleEvent.ToJson()} is invalid!");
                
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

                    string key = GetKey(candle);
                    var start = candle.CandleTimestamp;
                    if (candlesDict.ContainsKey(key))
                    {
                        var datesDict = candlesDict[key];
                        if (datesDict.ContainsKey(start))
                            datesDict[start] = Merge(datesDict[start], candlestick);
                        else
                            datesDict.Add(start, candlestick);
                    }
                    else
                    {
                        var datesDict = new Dictionary<DateTime, OutCandlestick>
                        {
                            { start, candlestick },
                        };
                        candlesDict.Add(key, datesDict);
                    }
                }
            }

            var list = candlesDict.Values
                .SelectMany(i => i.Values.Select(v => v.GetValuesString()))
                .ToList();

            if (list.Count > 0)
                await processTask(_mainContainer, list);
        }

        private string GetKey(CandleUpdate candle)
        {
            return $"{candle.AssetPairId}_{candle.PriceType}";
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

        private static bool IsValid(CandlesUpdatedEvent candleEvent)
        {
            return candleEvent.Candles.All(c =>
                !string.IsNullOrWhiteSpace(c.AssetPairId) && c.AssetPairId.Length <= _maxStringFieldsLength
                && c.High >= c.Low
                && c.CandleTimestamp <= c.ChangeTimestamp);
        }
    }
}
