using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ReactivePatterns
{
    /// <summary>
    /// This shows how to use .RefCount to cache a result
    /// </summary>
    public static class ObservableRefsCount
    {
        public static async Task Run()
        {
            Console.WriteLine("Starting program...");
            var feed = CreateCachingObservable();

            await Task.Delay(TimeSpan.FromSeconds(3));

            //listen for data
            Console.WriteLine("Starting listener 1");
            var listener1 = feed
                .Subscribe(p => { Console.WriteLine("Listener 1:  Got a new " + p); });
            await Task.Delay(TimeSpan.FromSeconds(5));

            Console.WriteLine("Starting listener 2");
            var listener2 = feed
                .Subscribe(p => { Console.WriteLine("Listener 2:  Got a new " + p); });
            await Task.Delay(TimeSpan.FromSeconds(5));

            Console.WriteLine("Disposing listener 1");
            listener1.Dispose();
            //disposing cancels further updates, and our underlying listener will not longer listen either
            //This is because we used RefCount to end our observable sequence when the last observer disconnects.

            await Task.Delay(TimeSpan.FromSeconds(6));

            //listen for data
            Console.WriteLine("Starting listener 1 again");
            listener1 = feed
                .Subscribe(p => { Console.WriteLine("Listener 1 (2): Got a new " + p); });

            await Task.Delay(TimeSpan.FromSeconds(5));

            //disposing cancels further updates, and our underlying listener will not longer listen either
            //This is because we used RefCount to end our observable sequence when the last observer disconnects.
            Console.WriteLine("Disposing both listeners");
            listener1.Dispose();
            listener2.Dispose();

            await Task.Delay(TimeSpan.FromSeconds(5));

            Console.WriteLine("Starting listener 1 again");
            listener1 = feed
                .Subscribe(p => { Console.WriteLine("Listener 1 (3): Got a new " + p); });

            await Task.Delay(TimeSpan.FromSeconds(4));

            Console.WriteLine("Disposing listener 1");
            listener1.Dispose();

            await Task.Delay(TimeSpan.FromSeconds(4));
        }

        public static IObservable<int> CreateCachingObservable() //singleton reference
        {
            var tradeListener = new TradeListener(); //singleton reference

            //We use .Publish() to convert our subject into a connectable observable.
            //We use .RefCount() to automatically call .OnCompleted() to signal a sequence end.
            var feed = tradeListener.DataFeed;

            //create an observable that is a subscription to our listener's observable
            var r = Observable
                .Create<int>(observer =>
                {
                    //when our observable is subscribed to, we start the listener and
                    //relay the listener to our sequence.
                    tradeListener.Start();

                    var s = feed.Subscribe(observer);

                    return s;
                })
                .Finally(() =>
                {
                    //stop once we receive end of the signal
                    tradeListener.Stop();
                    //stop would dispose any resources as needed
                })
                .Replay(1).RefCount()
                ;

            //TODO: what happens when if need to dispose TradeListener? (then dispose underlying on stop(), alternatively ? how do we guarantee that tradeListener eventually gets garbage collected?)
            return r;
        }


        /// <summary>
        /// Our listener... provides only start(), stop(), and the raw feed.
        /// You can think of this class as a background service that continuously fetches data.. ie. a Websocket or manual fetching.
        /// </summary>
        internal class TradeListener
        {
            public TradeListener()
            {
                Console.WriteLine("Underlying resource instantiated (should only happen once)...");
            }

            /// <summary>
            /// When subscribed to, we will automatically start fetching data, and when we unsubscribe our data fetching will stop.
            /// </summary>
            public IObservable<int> DataFeed => _latest.AsObservable();

            /// <summary>
            /// Latest is our underlying observable.
            /// </summary>
            private readonly Subject<int> _latest = new Subject<int>();

            private IDisposable _timer;

            /// <summary>
            /// Start listening. This timer observable is not really necessary for our demo, a timer class would also work. We use this timer to simulate async fetches on an interval.
            /// </summary>
            public void Start()
            {
                Console.WriteLine("Starting underlying connection...");

                _timer = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Subscribe(p =>
                    {
                        OnFetchLatestData().Wait();
                        //we block here and run synchronously, this guarantees our interval between events (fun: see what happens without .Wait() )
                    });
            }

            public void Stop()
            {
                _timer.Dispose();

                //log it
                Console.WriteLine("Stopped underlying connection.");
            }


            //our dummy counter - our observable will simply emit 1,2,3...etc. instead of real data.
            private int _counter = 0;

            /// <summary>
            /// This triggers an update on our observable.
            /// </summary>
            /// <returns></returns>
            private async Task OnFetchLatestData()
            {
                var newCounter = _counter++;

                Console.WriteLine("Fetching " + newCounter);
                await Task.Delay(2000); //simulate a delay
                
                _latest.OnNext(newCounter); //push our new value out
                Console.WriteLine("Fetched " + newCounter);
            }
        }
    }
}