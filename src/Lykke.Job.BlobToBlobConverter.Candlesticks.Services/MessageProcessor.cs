using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Common;
using Common.Log;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.OutputModels;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Services
{
    [UsedImplicitly]
    public class MessageProcessor : IMessageProcessor
    {
        private const int _maxStringFieldsLength = 255;
        private const int _maxBatchCount = 500000;

        private readonly ILog _log;

        private Dictionary<string, Dictionary<DateTime, OutCandlestick>> _candlesDict;
        private Func<string, List<string>, Task> _messagesHandler;

        public MessageProcessor(ILog log)
        {
            _log = log;
        }

        public void StartBlobProcessing(Func<string, List<string>, Task> messagesHandler)
        {
            _candlesDict = new Dictionary<string, Dictionary<DateTime, OutCandlestick>>();
            _messagesHandler = messagesHandler;
        }

        public async Task FinishBlobProcessingAsync()
        {
            await SaveAndClearCandlesAsync(true);
        }

        public async Task<bool> TryProcessMessageAsync(byte[] data)
        {
            var result = MessagePackDeserializer.TryDeserialize(
                data,
                _log,
                out CandlesUpdatedEvent candlesEvent);
            if (!result)
                return false;

            if (!IsValid(candlesEvent))
                _log.WriteWarning(nameof(TryProcessMessageAsync), nameof(Convert), $"CandleEvent {candlesEvent.ToJson()} is invalid!");

            ProcessCandles(candlesEvent);

            if (_candlesDict.Sum(i => i.Value.Count) >= _maxBatchCount)
                await SaveAndClearCandlesAsync(false);

            return true;
        }

        private async Task SaveAndClearCandlesAsync(bool saveAll)
        {
            var list = new List<string>();
            foreach (var pairCandlesDict in _candlesDict.Values)
            {
                var keys = pairCandlesDict.Keys.OrderBy(i => i).ToList();
                int maxCount = saveAll ? keys.Count : keys.Count - 1;
                for (int i = 0; i < maxCount; i++)
                {
                    list.Add(pairCandlesDict[keys[i]].GetValuesString());
                    if (!saveAll)
                        pairCandlesDict.Remove(keys[i]);
                }
            }

            if (saveAll)
                _candlesDict.Clear();

            if (list.Count > 0)
                await _messagesHandler(StructureBuilder.MainContainer, list);
        }

        public void ProcessCandles(CandlesUpdatedEvent candleEvent)
        {
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
                if (_candlesDict.ContainsKey(key))
                {
                    var datesDict = _candlesDict[key];
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
                    _candlesDict.Add(key, datesDict);
                }
            }
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
