using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ReactivePatterns
{
    public static class TaskCompletionSourceTest
    {
        static Stopwatch _watch = new Stopwatch();

        private static TaskCompletionSource<object> _taskCompletionSource = new TaskCompletionSource<object>();

        public static async Task Run()
        {
            _watch.Start();
            
            var newTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));

                Console.WriteLine(_watch.ElapsedMilliseconds + " Signaling completion");

                _taskCompletionSource.SetResult(null);
            });
            
            Console.WriteLine(_watch.ElapsedMilliseconds + " awaiting completion");

            await _taskCompletionSource.Task;

            Console.WriteLine(_watch.ElapsedMilliseconds + " all completed");
        }
    }
}