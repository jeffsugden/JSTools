using System.Threading;
using System.Threading.Tasks;

namespace JS.Tools.Tpl
{
    /// Async Pause Code Stuffs
    /// https://blogs.msdn.microsoft.com/pfxteam/2013/01/13/cooperatively-pausing-async-methods/
    /// 

    public class PauseTokenSource
    {
        private volatile TaskCompletionSource<bool> m_paused;
        internal static readonly Task s_completedTask = Task.FromResult(true);
        public bool IsPaused
        {
            get { return m_paused != null; }
            set
            {
                if (value)
                {
                    Interlocked.CompareExchange(ref m_paused, new TaskCompletionSource<bool>(), null);
                }
                else
                {
                    while (true)
                    {
                        var tcs = m_paused;
                        if (tcs == null) return;
                        if (Interlocked.CompareExchange(ref m_paused, null, tcs) == tcs)
                        {
                            tcs.SetResult(true);
                            break;
                        }
                    }
                }
            }
        }
        public PauseToken Token { get { return new PauseToken(this); } }

        internal Task WaitWhilePausedAsync(CancellationToken cancellationToken)
        {
            var cur = m_paused;
            //return cur != null ? cur.Task : s_completedTask;
            if (cur == null || cancellationToken.IsCancellationRequested) // when not paused or cancellation has occured, keep going 
            {
                return s_completedTask;
            }
            else // when paused, register with cancellation to allow cancel to abort pause
            {
                var cancellableSource = new TaskCompletionSource<bool>();
                var ctr = cancellationToken.Register(() => cancellableSource.SetResult(true));
                return Task.WhenAny(cur.Task, cancellableSource.Task).ContinueWith((t) => ctr.Dispose(), TaskContinuationOptions.ExecuteSynchronously);
            }
        }
    }
    public struct PauseToken
    {
        private readonly PauseTokenSource m_source;
        internal PauseToken(PauseTokenSource source) { m_source = source; }
        public bool IsPaused { get { return m_source != null && m_source.IsPaused; } }
        public Task WaitWhilePausedAsync(CancellationToken cancellationToken)
        {
            return IsPaused ?
                m_source.WaitWhilePausedAsync(cancellationToken) :
                PauseTokenSource.s_completedTask;
        }
    }
}
