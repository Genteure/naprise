using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise
{
    public class CompositeNotifier : INotifier
    {
        private readonly IReadOnlyList<INotifier> notifiers;

        public CompositeNotifier(IReadOnlyList<INotifier> notifiers)
        {
            this.notifiers = notifiers ?? throw new ArgumentNullException(nameof(notifiers));
        }

        public async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exceptions = new List<Exception>();
            var tasks = new Task[this.notifiers.Count];

            for (var i = 0; i < this.notifiers.Count; i++)
            {
                var notifier = this.notifiers[i];
                tasks[i] = Task.Run(() => notifier.NotifyAsync(message, cancellationToken));
            }

            // make sure all tasks are completed and every exception is collected
            for (var i = 0; i < tasks.Length; i++)
            {
                try
                {
                    await tasks[i];
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException("One or more notifications failed.", exceptions);
        }
    }
}
