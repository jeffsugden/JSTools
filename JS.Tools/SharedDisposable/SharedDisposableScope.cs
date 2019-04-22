using System;
using System.Collections.Generic;
using System.Linq;

namespace JS.Tools.SharedDisposable
{
    /// <summary>
    /// Managing object for handling coordination of object disposal for <see cref="SharedDisposable&lt;TDisposable&gt;"/> wrappers.
    /// </summary>
    public static class SharedDisposableScope
    {
        /// <summary>
        /// Creates a new shared disposable scope for a disposable object.
        /// </summary>
        /// <typeparam name="T">The disposable object type</typeparam>
        /// <param name="item">The disposable object to manage in the shared scope</param>
        /// <returns></returns>
        public static SharedDisposableScope<T> Create<T>(T item) where T : class, IDisposable
        {
            return new SharedDisposableScope<T>(item);
        }

        /// <summary>
        /// Creates a new shared disposable scope for a disposable object.
        /// </summary>
        /// <typeparam name="T">The disposable object type</typeparam>
        /// <param name="item">The disposable object to manage in the shared scope</param>
        /// <param name="disposeActionOverride">When not null, this action is executed instead of calling Dispose on <paramref name="item"/> when the scope completes</param>
        /// <param name="scopeHeldDisposables">When not null, a list of any other Disposable references that should be held for the lifetime of this scope. They will be Disposed() when the scope is complete.</param>
        /// <returns></returns>
        public static SharedDisposableScope<T> Create<T>(T item, Action<T> disposeActionOverride, IReadOnlyCollection<IDisposable> scopeHeldDisposables) where T : class, IDisposable
        {
            return new SharedDisposableScope<T>(item, disposeActionOverride, scopeHeldDisposables);
        }
    }

    /// <summary>
    /// Managing object for handling coordination of object disposal for <see cref="SharedDisposable&lt;TDisposable&gt;"/> wrappers.
    /// </summary>
    /// <typeparam name="TDisposable"></typeparam>
    public sealed class SharedDisposableScope<TDisposable> where TDisposable : class, IDisposable
    {
        private TDisposable _item;
        private int _refCount;

        private readonly object _padlock = new object();
        private readonly Action<TDisposable> _disposeActionOverride;
        private readonly IReadOnlyCollection<IDisposable> _scopeHeldDisposables;

        private sealed class InternalScopeSharedDisposable : SharedDisposable<TDisposable>
        {
            private readonly SharedDisposableScope<TDisposable> _scope;
            private TDisposable _item;

            internal InternalScopeSharedDisposable(SharedDisposableScope<TDisposable> scope)
            {
                _scope = scope ?? throw new ArgumentNullException(nameof(scope));
                _item = scope._item;
            }

            public override TDisposable Item => _item ?? throw new ObjectDisposedException(typeof(TDisposable).FullName);

            public override bool IsDisposed => _item == null;

            public override void Dispose()
            {
                if (IsDisposed)
                    return;

                _scope.DisposeItem();
                _item = null;
            }

            public override SharedDisposable<TDisposable> GetHandle()
            {
                if (IsDisposed) throw new ObjectDisposedException(typeof(TDisposable).FullName);
                return _scope.GetHandle();
            }
        }

        public SharedDisposableScope(TDisposable item, Action<TDisposable> disposeActionOverride, IReadOnlyCollection<IDisposable> scopeHeldDisposables) : this(item)
        {
            _disposeActionOverride = disposeActionOverride;
            _scopeHeldDisposables = scopeHeldDisposables;
        }

        public SharedDisposableScope(TDisposable item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _refCount = 0;
        }

        /// <summary>
        /// Get's a new independent <see cref="SharedDisposable&lt;TDisposable&gt; "/> wrapper to provide to a new owning consumer.
        /// Throws <exception cref="ObjectDisposedException"></exception> if the scope has already been disposed.
        /// </summary>
        /// <returns></returns>
        public SharedDisposable<TDisposable> GetHandle()
        {
            lock (_padlock)
            {
                if (_item == null) throw new ObjectDisposedException(typeof(TDisposable).Name);
                _refCount++;
            }

            return new InternalScopeSharedDisposable(this);
        }

        private void DisposeItem()
        {
            if (_item == null)
                return; // already disposed.
            lock (_padlock)
            {
                if (_refCount == 1)
                {
                    if (_disposeActionOverride != null)
                    {
                        _disposeActionOverride(_item);
                    }
                    else
                    {
                        _item.Dispose();
                    }
                    _item = null;
                    _refCount = 0;

                    foreach (var item in _scopeHeldDisposables ?? Enumerable.Empty<IDisposable>())
                    {
                        item.Dispose();
                    }
                }
                else
                {
                    _refCount--;
                }
            }
        }
    }
}
