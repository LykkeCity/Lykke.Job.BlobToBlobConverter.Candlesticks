using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.PeriodicalHandlers
{
    public class PeriodicalHandler : TimerPeriod
    {
        private readonly IBlobProcessor _blobProcessor;
        private readonly ILog _log;

        public PeriodicalHandler(
            IBlobProcessor blobProcessor,
            ILog log,
            TimeSpan processTimeout)
            : base((int)processTimeout.TotalMilliseconds, log)
        {
            _blobProcessor = blobProcessor;
            _log = log;
        }

        public override async Task Execute()
        {
            await _blobProcessor.ProcessAsync();
        }
    }
}
