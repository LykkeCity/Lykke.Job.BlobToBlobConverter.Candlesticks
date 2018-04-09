using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Common;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Services;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Services
{
    [UsedImplicitly]
    public class ShutdownManager : IShutdownManager
    {
        private readonly List<IStopable> _items = new List<IStopable>();

        public void Register(IStopable stopable)
        {
            _items.Add(stopable);
        }

        public async Task StopAsync()
        {
            foreach (var item in _items)
            {
                item.Stop();
            }

            await Task.CompletedTask;
        }
    }
}
