using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Common;
using Common.Log;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.PeriodicalHandlers
{
    [UsedImplicitly]
    public class PeriodicalHandler : TimerPeriod
    {
        private readonly IBlobProcessor _blobProcessor;

        public PeriodicalHandler(
            IBlobProcessor blobProcessor,
            ILog log,
            TimeSpan processTimeout)
            : base((int)processTimeout.TotalMilliseconds, log)
        {
            _blobProcessor = blobProcessor;
        }

        public override async Task Execute()
        {
            await _blobProcessor.ProcessAsync();
        }
    }
}
