using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Services;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Services;
using Lykke.Job.BlobToBlobConverter.Candlesticks.PeriodicalHandlers;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Services;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Settings;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Modules
{
    public class JobModule : Module
    {
        private readonly BlobToBlobConverterCandlesticksSettings _settings;
        private readonly ILog _log;

        public JobModule(BlobToBlobConverterCandlesticksSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterResourcesMonitoring(_log);

            builder.RegisterType<BlobReader>()
                .As<IBlobReader>()
                .SingleInstance()
                .WithParameter("container", _settings.InputContainer)
                .WithParameter("blobConnectionString", _settings.InputBlobConnString);

            builder.RegisterType<BlobSaver>()
                .As<IBlobSaver>()
                .SingleInstance()
                .WithParameter("blobConnectionString", _settings.OutputBlobConnString)
                .WithParameter("rootContainer", _settings.InputContainer);

            builder.RegisterType<MessageProcessor>()
                .As<IMessageProcessor>()
                .SingleInstance();

            builder.RegisterType<StructureBuilder>()
                .As<IStructureBuilder>()
                .SingleInstance();

            builder.RegisterType<BlobProcessor>()
                .As<IBlobProcessor>()
                .SingleInstance();

            builder.RegisterType<PeriodicalHandler>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.BlobScanPeriod));
        }
    }
}
