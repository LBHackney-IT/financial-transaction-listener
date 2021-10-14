using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialTransactionListener.V1.Infrastructure
{
    public static class RetryService
    {
        /// <summary>
        /// Async performing repetitive logic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">Task object</param>
        /// <param name="maxAttemptCount">Count of attempts</param>
        /// <param name="delay">Delay between several attempts</param>
        public static async Task<T> DoAsync<T>(
            Task<T> action,
            int maxAttemptCount,
            TimeSpan delay)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        await Task.Delay(delay);
                    }

                    return await action.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            // Hanna Holasava
            // When mistake throws then a sns message automatically
            // go to dead letter queue
            // https://aws.amazon.com/ru/blogs/compute/using-amazon-sqs-dead-letter-queues-to-replay-messages/
            throw new AggregateException(exceptions);
        }
    }
}
