using System;
using System.Threading;

namespace AzureSolutions.Util
{
        /// <summary>
        /// Generic wrapper for <see cref="LazyInitializer.EnsureInitialized{T}(ref T)"/> 
        /// Based on https://github.com/dotnet/corefx/issues/32337#issuecomment-498962755
        /// </summary>
        /// <typeparam name="T">type of object that is lazily constructed</typeparam>
        public class LazyInitializer<T>
        {
            // This implementation is intended as a replacement for System.Lazy<T>
            // Provides a facility for one time, thread-safe, object initialization.

            // The problem with Lazy<T> is unsafe for any non-trivial initialization as it CACHES EXCEPTIONS.
            // That is, if one call to valueFactory throws exception, all subsequent calls will also fail (will not invoke 
            // valueFactory multiple times)
            // The second is to avoid using explicit locking. Locking can be expensive when overused, and naive techniques like 
            // double-checked locking can be tricky and even might not work on some systems 
            // (https://csharpindepth.com/Articles/Singleton#dcl).

            // This implementation only proxies initialization to LazyInitializer

            private T _value;
            private object _syncLock = new object();
            private bool _initialized = false;
            private readonly Func<T> _factory;

            public LazyInitializer(Func<T> valueFactory)
            {
                _factory = valueFactory;
            }

         public T Value =>
                LazyInitializer.EnsureInitialized(ref _value, ref _initialized, ref _syncLock, _factory);
        }
}
