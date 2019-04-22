using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JS.Tools.Tpl
{
    /// <summary>
    /// This class keeps track of multiple in-progress/scheduled async operations, with a throttler to limit how many can be in-progress at once. It is not designed to be thead-safe for concurrent access. 
    /// </summary>
    public class ParallelAsyncTaskRunner : IDisposable
    {
        private readonly SemaphoreSlim _queueSemaphore;
        private readonly SemaphoreSlim _throttleSemaphore;
        private readonly bool _dispatchToThreadPool;

        private Stack<Task> _tasks;

        public int TaskCount => _tasks.Count;
    
        public ParallelAsyncTaskRunner(int maxQueuedTasks, int maxRunningTasks, bool dispatchInitialTaskToTheadPool)
        {
            _tasks = new Stack<Task>();
            _queueSemaphore = new SemaphoreSlim(maxQueuedTasks);
            _throttleSemaphore = new SemaphoreSlim(maxRunningTasks);
            _dispatchToThreadPool = dispatchInitialTaskToTheadPool;
        }

        /// <summary>
        /// Waits Async if the queue if full, until we can schedule more Tasks. Then starts the task and returns.
        /// If the queue is not full, will start task and then return immediately.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task QueueWaitAndStartTaskAsync(Func<Task> func)
        {
            await _queueSemaphore.WaitAsync().ConfigureAwait(false);
            var task = _dispatchToThreadPool ? Task.Run(async () => await InternalStartTaskAsync(func)) : InternalStartTaskAsync(func);
            _tasks.Push(task);
        }

        private async Task InternalStartTaskAsync(Func<Task> awaitableFunction)
        {
            await _throttleSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await awaitableFunction().ConfigureAwait(false);
            }
            finally
            {
                _throttleSemaphore.Release();
                _queueSemaphore.Release();
            }
        }

        public Task WhenAllAndReset()
        {
            var tempTasks = _tasks;
            _tasks = new Stack<Task>();
            return Task.WhenAll(tempTasks);
        }

        public Task WhenAll()
        {
            return Task.WhenAll(_tasks);
        }

        public void Dispose()
        {
            _queueSemaphore.Dispose();
            _throttleSemaphore.Dispose();
        }
    }
}
