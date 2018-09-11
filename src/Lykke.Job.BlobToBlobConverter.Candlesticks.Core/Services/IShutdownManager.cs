using System.Threading.Tasks;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
