using System.Threading.Tasks;
using Bootstrapping;

namespace ReactivePatterns
{
    public class Program
    {
        static void Main(string[] args)
        {
            typeof(Program).Bootstrap();
        }



        public static void Run_ObservableRefsCount()
        {
            ObservableRefsCount.Run().Wait();
        }

        public static void Run_TaskCompletionSourceTest()
        {
            TaskCompletionSourceTest.Run().Wait();
        }

        public static void Run_ObservableRateLimiter()
        {
            ObservableRateLimiter.Run().Wait();
        }
    }
}
