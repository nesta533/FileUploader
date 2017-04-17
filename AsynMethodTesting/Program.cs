using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Logger;

namespace AsynMethodTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Go().Wait();
            Console.WriteLine("main exit()");
        }

        public static async Task Go()
        {
            TaskLogger.LogLevel = TaskLogger.TaskLogLevel.Pending;
            var tasks = new List<Task>
            {
                Task.Delay(2000).Log("2s op"),
                Task.Delay(5000).Log("5s op"),
                Task.Delay(6000).Log("6s op")
            };

            try
            {
                await Task.WhenAll(tasks).WithCancellation(new CancellationTokenSource(3000).Token);
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }

            foreach (var op in TaskLogger.GetLogEntries().OrderBy(tle => tle.LogTime))
            {
                Console.WriteLine(op);
            }

        }

        public async Task MyMethod()
        {
            //throw new InvalidOperationException();
           await Task.Delay(1000);
        }

        public void CallMethod()
        {
            MyMethod().ContinueWith( t => Console.WriteLine(t.ToString()));
        }


    }
}
