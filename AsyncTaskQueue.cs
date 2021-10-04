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
                        int i = highQueue.Count - 1;
                        while(i > -1)
                        {
                            if(highQueue[i].priority == Priority.Interrupt) { break; }
                            i--;
                        }
                        highQueue.Insert(++i, item);

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

        public AsyncTask Add(Task task, Priority priority = Priority.Default)
        {
            var asyncTask = new AsyncTask(task, AddToCurrentTasks, RemoveFromCurrentTasks, priority);
            Add(asyncTask);
            return asyncTask;
        }

        public bool Contains(AsyncTask task)
        {
            return defaultQueue.Contains(task) || highQueue.Contains(task) || curentTasks.Contains(task); 
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
                var nextTask = queue[0];
                queue.Remove(nextTask);
                return nextTask;
            }
            return null;
        }

        public void Remove(AsyncTask task)
        {
            if(curentTasks.Contains(task)) {
                task.Stop(); 
                return; 
            }

            if (task.priority == AsyncTask.Priority.Default)
            {
                if (defaultQueue.Contains(task)) { 
                    defaultQueue.Remove(task); 
                    return; 
                }
            }
            else
            {
                if (highQueue.Contains(task)) { 
                    highQueue.Remove(task);
                    return;
                }
            }

            throw new Exception(string.Format("[{0}] error: Missing {1} value with priority {2}", this, task, task.priority));
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

        private void AddToCurrentTasks(AsyncTask task)
        {
            if (curentTasks.Count < maxNumberOfThreads)
            {
                curentTasks.Insert(0, task);
            }
            else
            {
                throw new Exception(
                    string.Format("[{0}] error: the list of current tasks is full, wait until one of the current tasks is completed", this));
            }
        }

        private void RemoveFromCurrentTasks(AsyncTask task)
        {
            if (curentTasks.Contains(task)) { curentTasks.Remove(task); }
        }

        public class Exception : System.Exception 
        {
            public Exception(string messge) : base(messge) { }
        }
    }
}