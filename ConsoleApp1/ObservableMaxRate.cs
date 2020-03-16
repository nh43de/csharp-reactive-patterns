using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace ReactivePatterns
{
    /// <summary>
    /// Inspired from http://www.zerobugbuild.com/?p=323
    /// Rate limits observable stream to a max rate.
    /// </summary>
    public static class ObservableRateLimiter
    {
        public static async Task Run()
        {
            Console.WriteLine("Starting program...");

            var sourceObservable = Observable.Interval(TimeSpan.FromSeconds(0.5)).Publish();

            var rateLimitedObservable = sourceObservable.RateLimit(TimeSpan.FromSeconds(1)).Subscribe(r =>
            {
                Console.WriteLine("Listener:  Got a new " + r);
            });

            Console.WriteLine("Starting underlying subscription");

            var sourceSubscription = sourceObservable.Connect();

            await Task.Delay(TimeSpan.FromSeconds(5));

            Console.WriteLine("Killing underlying subscription");

            sourceSubscription.Dispose();

            Console.WriteLine("Waiting for rate limited items to come in");

            await Task.Delay(TimeSpan.FromSeconds(12));

            rateLimitedObservable.Dispose();
        }

        public static IObservable<T> RateLimit<T>(this IObservable<T> input, TimeSpan interval)
        {
            var paced = input
                .Select(i => Observable.Empty<T>()
                .Delay(interval)
                .StartWith(i)).Concat();

            return paced;
        }

    }
}