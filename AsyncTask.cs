using System.Threading;

namespace ru.mofrison.AsyncTasks
{
    public class AsyncTask
    {
        public enum Priority { Default, High, Interrupt }
        public delegate System.Threading.Tasks.Task Task(CancellationTokenSource cancellationToken);

        public readonly Priority priority;
        private readonly Task task;
        private CancellationTokenSource cancellationToken;
        private readonly System.Action<AsyncTask> onStart;
        private readonly System.Action<AsyncTask> onExit;

        public AsyncTask(Task task, System.Action<AsyncTask> onStart,  System.Action<AsyncTask> onExit, Priority priority) 
        { 
            this.task = task;
            this.priority = priority;
            this.onStart = onStart;
            this.onExit = onExit;
        }

        public async System.Threading.Tasks.Task Run()
        {
            onStart?.Invoke(this);
            cancellationToken = new CancellationTokenSource();
            await task(cancellationToken);
            cancellationToken.Dispose();
            onExit?.Invoke(this);
        }

        public void Stop()
        {
            cancellationToken.Cancel();
            cancellationToken.Dispose();
            onExit?.Invoke(this);
        }
    }
}