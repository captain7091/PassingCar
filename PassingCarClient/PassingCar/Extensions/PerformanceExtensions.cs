using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PassingCar.Extensions
{
    public static class PerformanceExtensions
    {
        /// <summary>
        /// Measures the execution time of an async operation
        /// </summary>
        public static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await operation();
                stopwatch.Stop();
                return (result, stopwatch.Elapsed);
            }
            catch
            {
                stopwatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Measures the execution time of an async operation without return value
        /// </summary>
        public static async Task<TimeSpan> MeasureAsync(Func<Task> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await operation();
                stopwatch.Stop();
                return stopwatch.Elapsed;
            }
            catch
            {
                stopwatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Logs performance metrics for debugging
        /// </summary>
        public static void LogPerformance(string operationName, TimeSpan duration)
        {
            if (duration.TotalMilliseconds > 1000) // Log if operation takes more than 1 second
            {
                Debug.WriteLine($"⚠️ SLOW OPERATION: {operationName} took {duration.TotalMilliseconds:F2}ms");
            }
            else if (duration.TotalMilliseconds > 500) // Log if operation takes more than 500ms
            {
                Debug.WriteLine($"⚡ MODERATE: {operationName} took {duration.TotalMilliseconds:F2}ms");
            }
            else
            {
                Debug.WriteLine($"✅ FAST: {operationName} took {duration.TotalMilliseconds:F2}ms");
            }
        }

        /// <summary>
        /// Executes an operation with performance logging
        /// </summary>
        public static async Task<T> ExecuteWithLoggingAsync<T>(string operationName, Func<Task<T>> operation)
        {
            var (result, duration) = await MeasureAsync(operation);
            LogPerformance(operationName, duration);
            return result;
        }

        /// <summary>
        /// Executes an operation with performance logging (no return value)
        /// </summary>
        public static async Task ExecuteWithLoggingAsync(string operationName, Func<Task> operation)
        {
            var duration = await MeasureAsync(operation);
            LogPerformance(operationName, duration);
        }

        /// <summary>
        /// Creates a timeout wrapper for operations
        /// </summary>
        public static async Task<T> WithTimeoutAsync<T>(this Task<T> task, TimeSpan timeout, string operationName = "Operation")
        {
            using var cts = new CancellationTokenSource(timeout);
            try
            {
                return await task;
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"{operationName} timed out after {timeout.TotalSeconds} seconds");
            }
        }

        /// <summary>
        /// Batches operations to avoid overwhelming the system
        /// </summary>
        public static async Task ProcessInBatchesAsync<T>(
            IEnumerable<T> items,
            Func<T, Task> processor,
            int batchSize = 10,
            int delayBetweenBatches = 0) // Default to 0 for instant processing
        {
            var batches = items.Chunk(batchSize);

            foreach (var batch in batches)
            {
                var tasks = batch.Select(processor);
                await Task.WhenAll(tasks);

                // Only delay if explicitly requested (for rate limiting)
                if (delayBetweenBatches > 0)
                {
                    await Task.Delay(delayBetweenBatches);
                }
            }
        }
    }

    /// <summary>
    /// Extension method for IEnumerable to create chunks
    /// </summary>
    public static class EnumerableExtensions
    {
        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> source, int size)
        {
            var list = source.ToList();
            for (int i = 0; i < list.Count; i += size)
            {
                yield return list.Skip(i).Take(size).ToArray();
            }
        }
    }
}
