using System;
using System.Collections.Generic;

namespace JS.Tools
{
    public class Resiliancy
    {
        /// <summary>
        /// Performs an operations returning a type of T. Should the operation throw an exception, it will be 
        /// retried after the retry interval. The backoff is exponentional on each retry.
        /// e.g. string res = RetryHelper{string}( () => myFunc(myParam1, myParam2), 5, 500).Value;
        /// </summary>
        /// <typeparam name="T">Return type of function to add retry semantics to.</typeparam>
        /// <param name="func">The function delegate.</param>
        /// <param name="maxAttempts">Number of times to retry the operation.</param>
        /// <param name="baseMillisecondsInterval">The interval of the first retry, will double upon each attempt.</param>
        public TResult RetryHelper<TResult>(Func<TResult> func, int maxAttempts = 2, int baseMillisecondsInterval = 5000)
        {
            int retryInterval = baseMillisecondsInterval;
            var exceptions = new List<Exception>();
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return func();
                }
                catch (OperationCanceledException) // If operation has been cancelled, abort retry and let the exception "bubble up" to the caller
                {
                    throw;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
                if (attempt != maxAttempts)  // skip wait if we arn't going to try again anyway
                {
                    System.Threading.Thread.Sleep(retryInterval);
                    retryInterval = retryInterval * 2;
                }
            }
            throw new AggregateException(string.Format("Exhausted retries (Attempts={0})", maxAttempts), exceptions);
        }
    }
}
