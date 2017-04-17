using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Logger
{
    public static class TaskLogger
    {
        public enum TaskLogLevel { None, Pending }
        public static TaskLogLevel LogLevel { get; set; }

        public sealed class TaskLogEntry
        {
            public Task Task { get; internal set; }
            public String Tag { get; internal set; }
            public DateTime LogTime { get; internal set; }
            public String CallerMemberName { get; internal set; }
            public String CallerFilePath { get; internal set; }
            public Int32 CallerLineNumber { get; internal set; }
            public override string ToString()
            {
                return String.Format("LogTime={0}, Tag={1}, Memeber={2}, File={3}({4})", LogTime, Tag ?? "none", CallerMemberName,
                    CallerFilePath, CallerLineNumber);
            }
        }

        private static readonly ConcurrentDictionary<Task, TaskLogEntry> s_log = new ConcurrentDictionary<Task, TaskLogEntry>();
        public static IEnumerable<TaskLogEntry> GetLogEntries() { return s_log.Values;  }

        private static Task<TResult> Log<TResult>(this Task<TResult> task, String tag = null, [CallerMemberName] String callerMemberName = null,
            [CallerFilePath] String callerFilePath = null, [CallerLineNumber]Int32 callerLineNumber = -1)
        {
            return (Task<TResult>)Log((Task)task, tag, callerMemberName, callerFilePath, callerLineNumber);
        }

        public static Task Log(this Task task, String tag = null, [CallerMemberName] String callerMemberName = null,
            [CallerFilePath] String callerFilePath = null, [CallerLineNumber]Int32 callerLineNumber = -1)
        {
            if (LogLevel == TaskLogLevel.None)
            {
                return task;
            }

            var logEntry = new TaskLogEntry
            {
                Task = task,
                Tag = tag,
                LogTime = DateTime.Now,
                CallerMemberName = callerMemberName,
                CallerFilePath = callerFilePath,
                CallerLineNumber = callerLineNumber
            };

            s_log[task] = logEntry;
            task.ContinueWith(t => {TaskLogEntry entry; s_log.TryRemove(t, out entry);},TaskContinuationOptions.ExecuteSynchronously );

            return task;
        }
    }

    public static class Cancellation
    {

        private struct Void { } // Because there isn't a non-generic TaskCompletionSource class.

        public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> orignalTask, CancellationToken ct)
        {
            // Create a Task that completes when the CancellationToken is canceled
            var cancelTask = new TaskCompletionSource<Void>();

            // When the CancellationToken is cancelled, complete the Task
            using (ct.Register(t => ((TaskCompletionSource<Void>)t).TrySetResult(new Void()), cancelTask))
            {

                // Create another Task that completes when the original Task or when the CancellationToken's Task
                Task any = await Task.WhenAny(orignalTask, cancelTask.Task);

                // If any Task completes due to CancellationToken, throw OperationCanceledException         
                if (any == cancelTask.Task) ct.ThrowIfCancellationRequested();
            }

            // await original task (synchronously); if it failed, awaiting it 
            // throws 1st inner exception instead of AggregateException
            return await orignalTask;
        }

        public static async Task WithCancellation(this Task task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<Void>();
            using (ct.Register(t => ((TaskCompletionSource<Void>)t).TrySetResult(default(Void)), tcs))
            {
                if (await Task.WhenAny(task, tcs.Task) == tcs.Task) ct.ThrowIfCancellationRequested();
            }
            await task;          // If failure, ensures 1st inner exception gets thrown instead of AggregateException
        }

    }

}
