using System.Threading;
using System.Threading.Tasks;

namespace Naprise
{
    public interface INotifier
    {
        Task NotifyAsync(Message message, CancellationToken cancellationToken = default);
    }
}
