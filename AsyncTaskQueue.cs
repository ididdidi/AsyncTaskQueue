using System.Collections.Generic;
using static ru.mofrison.AsyncTasks.AsyncTask;

namespace ru.mofrison.AsyncTasks
{
    public class AsyncTaskQueue
    {
        private readonly List<AsyncTask> defaultQueue = new List<AsyncTask>();
        private readonly List<AsyncTask> highQueue = new List<AsyncTask>();
        private readonly List<AsyncTask> curentTasks = new List<AsyncTask>();
        private readonly int maxNumberOfThreads;

        public int Count => defaultQueue.Count + highQueue.Count;
        public int NumberOfThreads => curentTasks.Count;
        public int MaxNumberOfThreads => maxNumberOfThreads;
        public bool NextIsRedy => Count > 0 && curentTasks.Count < maxNumberOfThreads;

        public AsyncTaskQueue(int maxNumberOfThreads = 1)
        {
            if(maxNumberOfThreads < 1)
            {
                throw new System.ArgumentException(
                    string.Format("[{0}] error: maximum size of current tasks = {1}. Please set it to a value greater than 1 ", this, maxNumberOfThreads));
            }
            this.maxNumberOfThreads = maxNumberOfThreads;
        }

        public void Add(AsyncTask item)
        {
            switch (item.priority)
            {
                case AsyncTask.Priority.Default:
                    {
                        defaultQueue.Add(item);
                        break;
                    }
                case AsyncTask.Priority.High:
                    {
                        highQueue.Add(item);
                        break;
                    }
                case AsyncTask.Priority.Interrupt:
                    {
                        highQueue.Insert(0, item);
                        if (curentTasks.Count == maxNumberOfThreads)
                        {
                            switch (curentTasks[0].priority)
                            {
                                case AsyncTask.Priority.Default:
                                    {
                                        defaultQueue.Insert(0, curentTasks[0]);
                                        break;
                                    }
                                case AsyncTask.Priority.High:
                                    {
                                        highQueue.Insert(1, curentTasks[0]);
                                        break;
                                    }
                                case AsyncTask.Priority.Interrupt:
                                    {
                                        return;
                                    }
                            }
                            curentTasks[0].Stop();
                        }
                        break;
                    }
            }
        }

        public void Add(Task task, Priority priority = Priority.Default)
        {
            Add(new AsyncTask(task, RemoveFromCurrentTasks, priority));
        }

        public AsyncTask GetNext()
        {
            List<AsyncTask> queue;
            if (highQueue.Count > 0)
            {
                queue = highQueue;
            }
            else
            {
                queue = defaultQueue;
            }

            if (queue.Count > 0)
            {
                if(curentTasks.Count < maxNumberOfThreads)
                {
                    curentTasks.Insert(0, queue[0]);
                    queue.Remove(curentTasks[0]);
                    return curentTasks[0];
                }
                else
                {
                    throw new Exception(
                        string.Format("[{0}] error: the list of current tasks is full, wait until one of the current tasks is completed", this));
                }
            }
            return null;
        }

        private void RemoveFromCurrentTasks(AsyncTask task)
        {
            if (curentTasks.Contains(task)) { curentTasks.Remove(task); }
        }

        public void Clear()
        {
            defaultQueue.Clear();
            highQueue.Clear();
            while(curentTasks.Count > 0)
            {
                curentTasks[0].Stop();
            }
        }
        
        public class Exception : System.Exception 
        {
            public Exception(string messge) : base(messge) { }
        }
    }
}