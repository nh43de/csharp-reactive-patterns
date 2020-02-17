using System.Threading.Tasks;
using MyApplication;

namespace ReactivePatterns
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
            typeof(Program).Bootstrap();
        }

        public static void Run_ObservableRefsCount()
        {
            ObservableRefsCount.Run().Wait();
        }

        public static void Run_MultiAwait()
        {
            CustomAwaiter.Run().Wait();
        }


    }
}
