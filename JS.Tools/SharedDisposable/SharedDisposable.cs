using System;

namespace JS.Tools.SharedDisposable
{
    /// <summary>
    /// Wrapper around a Disposable object that may have more than one owning consumer. Used in conjunction with <see cref="SharedDisposableScope"/>.
    /// Each consumers gets their own instance of <see cref="SharedDisposable&lt;TDisposable&gt; "/> and disposes of it when done.
    /// </summary>
    /// <typeparam name="TDisposable"></typeparam>
    public abstract class SharedDisposable<TDisposable> : IDisposable where TDisposable : class, IDisposable
    {
        /// <summary>
        /// The actual Disposable object. Throws <exception cref="ObjectDisposedException"></exception> if this wrapper has already been disposed.
        /// </summary>
        public abstract TDisposable Item { get; }

        /// <summary>
        /// True if this wrapper has already been disposed. False otherwise
        /// </summary>
        public abstract bool IsDisposed { get; }
    
        public abstract void Dispose();

        /// <summary>
        /// Get's a new independent <see cref="SharedDisposable&lt;TDisposable&gt; "/> wrapper to provide to a new owning consumer.
        /// Throws <exception cref="ObjectDisposedException"></exception> if this wrapper has already been disposed.
        /// </summary>
        /// <returns></returns>
        public abstract SharedDisposable<TDisposable> GetHandle();
    }
}
