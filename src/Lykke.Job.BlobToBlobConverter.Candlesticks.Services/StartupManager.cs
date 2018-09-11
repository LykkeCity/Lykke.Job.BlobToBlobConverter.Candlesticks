using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Services;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Services
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly List<IStartable> _startables = new List<IStartable>();

        public StartupManager(IEnumerable<IStartStop> startables)
        {
            _startables.AddRange(startables);
        }

        public Task StartAsync()
        {
            foreach (var startable in _startables)
            {
                startable.Start();
            }

            return Task.CompletedTask;
        }
    }
}
