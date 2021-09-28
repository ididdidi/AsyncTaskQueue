using System.Collections.Generic;
using static ru.mofrison.AsyncTaskQueue.AsyncTask;

namespace ru.mofrison.AsyncTaskQueue
{
    public class AsyncTaskQueue
    {
        private readonly List<AsyncTask> defaultQueue = new List<AsyncTask>();
        private readonly List<AsyncTask> highQueue = new List<AsyncTask>();
        private readonly List<AsyncTask> curentTasks = new List<AsyncTask>();
        private readonly ushort maxSixeCurentList;

        public int Count => defaultQueue.Count + highQueue.Count; 
        public ushort MaxSixeCurentList => maxSixeCurentList;
        public bool NextIsRedy => Count > 0 && curentTasks.Count < maxSixeCurentList;

        public AsyncTaskQueue(ushort maxSixeCurentList = 1)
        {
            if(maxSixeCurentList < 1)
            {
                throw new System.ArgumentException(
                    string.Format("[{0}] error: maximum size of current tasks = {1}. Please set it to a value greater than 1 ", this, maxSixeCurentList));
            }
            this.maxSixeCurentList = maxSixeCurentList;
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
                        if (curentTasks.Count == maxSixeCurentList)
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
                if(curentTasks.Count < maxSixeCurentList)
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

        public void RemoveFromCurrentTasks(AsyncTask task)
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
                curentTasks.Remove(curentTasks[0]);
            }
        }
        
        public class Exception : System.Exception 
        {
            public Exception(string messge) : base(messge) { }
        }
    }
}