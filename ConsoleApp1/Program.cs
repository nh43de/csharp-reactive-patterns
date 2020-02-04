﻿using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ReactivePatterns.Console
{
    public class Program
    {
        /// <summary>
        /// Use .Publish() and .RefCount() to provide a caching mechanism.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            //First wait - you will not see any logging to console
            await Task.Delay(TimeSpan.FromSeconds(5));

            var tradeListener = new TradeListener();

            //listen for data
            var subscription = tradeListener
                .DataFeed
                .Subscribe(p => { System.Console.WriteLine("Got a new " + p); });

            await Task.Delay(TimeSpan.FromSeconds(7));

            //disposing cancels further updates, and our underlying listener will not longer listen either
            //This is because we used RefCount to end our observable sequence when the last observer disconnects.
            System.Console.WriteLine("Disposing subscription");
            subscription.Dispose();

            await Task.Delay(TimeSpan.FromSeconds(7));

            //listen for data
            subscription = tradeListener
                .DataFeed
                .Subscribe(p => { System.Console.WriteLine("Got a new2 " + p); });

            await Task.Delay(TimeSpan.FromSeconds(7));

            //disposing cancels further updates, and our underlying listener will not longer listen either
            //This is because we used RefCount to end our observable sequence when the last observer disconnects.
            System.Console.WriteLine("Disposing subscription");
            subscription.Dispose();
            
            await Task.Delay(TimeSpan.FromSeconds(6));
        }
    }

    
    /// <summary>
    /// Our listener... provides only start(), stop(), and the raw feed.
    /// You can think of this class as a background service that continuously fetches data.. ie. a Websocket or manual fetching.
    /// </summary>
    internal class TradeListener 
    {
        /// <summary>
        /// When subscribed to, we will automatically start fetching data, and when we unsubscribe our data fetching will stop.
        /// </summary>
        public IObservable<int> DataFeed { get; }

        /// <summary>
        /// Latest is our underlying observable.
        /// </summary>
        private readonly Subject<int> _latest = new Subject<int>();

        private IDisposable _timer;

        public TradeListener()
        {
            //We use .Publish() to convert our subject into a connectable observable.
            //We use .RefCount() to automatically call .OnCompleted() to signal a sequence end.
            var feed = _latest.Publish().RefCount();

            //create an observable that is a subscription to our listener's observable
            DataFeed = Observable
                .Create<int>(observer =>
                {
                    //when our observable is subscribed to, we start the listener and
                    //relay the listener to our sequence.
                    Start();

                    var s = feed.Subscribe(observer);

                    return s;
                })
                .Finally(() =>
                {
                    //stop once we receive end of the signal
                    Stop();
                    //also would call dispose here to dispose any resources.
                });
        }

        /// <summary>
        /// Start listening. This timer observable is not really necessary for our demo, a timer class would also work. We use this timer to simulate async fetches on an interval.
        /// </summary>
        private void Start()
        {
            System.Console.WriteLine("Starting...");

            _timer = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(p =>
                {
                    OnFetchLatestData().Wait(); 
                    //we block here and run synchronously, this guarantees our interval between events (fun: see what happens without .Wait() )
                });
        }

        private void Stop()
        {
            _timer.Dispose();
            
            //log it
            System.Console.WriteLine("Stopped");
        }


        //our dummy counter - our observable will simply emit 1,2,3...etc. instead of real data.
        private int _counter = 0;

        /// <summary>
        /// This triggers an update on our observable.
        /// </summary>
        /// <returns></returns>
        private async Task OnFetchLatestData()
        {
            var newCounter = this._counter++;

            System.Console.WriteLine("Fetching " + newCounter);
            await Task.Delay(2000); //simulate a delay
            System.Console.WriteLine("Listener got " + newCounter);
            _latest.OnNext(newCounter); //push our new value out
        }
    }

}
