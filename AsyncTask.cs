using System.Threading;

namespace ru.mofrison.AsyncTaskQueue
{
    public class AsyncTask
    {
        public enum Priority { Default, High, Interrupt }
        public delegate System.Threading.Tasks.Task Task(CancellationTokenSource cancellationToken);

        public readonly Priority priority;
        private readonly Task task;
        private CancellationTokenSource cancellationToken;
        private readonly System.Action<AsyncTask> onFinish;

        public AsyncTask(Task task, System.Action<AsyncTask> onFinish, Priority priority) 
        { 
            this.task = task;
            this.priority = priority;
            this.onFinish = onFinish;
        }

        public async System.Threading.Tasks.Task Start()
        {
            cancellationToken = new CancellationTokenSource();
            await task(cancellationToken);
            cancellationToken.Dispose();
            onFinish?.Invoke(this);
        }

        public void Stop()
        {
            cancellationToken.Cancel();
            cancellationToken.Dispose();
            onFinish?.Invoke(this);
        }
    }
}