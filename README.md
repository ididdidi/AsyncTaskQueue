# AsyncTaskQueue
Asynchronous Task Queue module for C#. The module is designed to order asynchronous tasks in a queue based on their priority.

## Priority types
* Default - The type with the lowest priority. Tasks are added to the end of the queue and retrieved after all higher priority tasks have been submitted for execution. 
* High - A relatively high priority type. These tasks are added to the queue for tasks with the same priority. 
* Interrupt - Highest priority. Such tasks go to the very beginning of the queue and interrupt the execution of a task with a lower priority if there is no free space in the execution queue.
**Warning: The progress of an interrupted task is lost and it is ranked first in the queue of tasks with the same priority.**

## AsyncTask
The class whose instances are the basic elements of the asynchronous task queue.
This class encapsulates a delegate that takes a single `CancellationTokenSource` argumenite and returns a `System.Threading.Tasks.Task instance`.
`CancellationTokenSource` is required to interrupt the execution of the task (Interruption must be provided by the author of the method that is passed as a delegate). 
In addition, the class contains Priority for storing the priority of the task, `CancellationTokenSource` for the ability to interrupt the task execution and `System.Action<AsyncTask>` in order to inform the parent about the termination of the task execution. 

| Method	| Description |
| --------- | ----------- |
| AsyncTask	| The constructor accepts a waiting delegate with a single argument, `CancellationTokenSource`, `System.Action<AsyncTask>` and `AsyncTask.Priority`. Serves to create and initialize an instance AsyncTask. |
| Run		| Starts the execution of a task. Supports waiting and returns `System.Threading.Tasks.Task`. |
| Stop		| Stops the execution of a task. |

## AsyncTaskQueue
Implements a priority queue for asynchronous tasks.
This queue is implemented according to the Fi-Fo principle, taking into account the priority.
The queue supports the execution of several tasks in parallel. The number of threads is specified when creating a queue instance. 

| Field				 | Description |
| ------------------ | ----------- |
| Count 		 	 | The number of tasks currently in the queue.	|
| NumberOfThreads	 | The number of threads used to execute tasks.	|
| MaxNumberOfThreads | The maximum number of threads to run tasks.	|
| NextIsRedy		 | The Ready flag for next item(takes into account the number of free threads). |

| Method				 | Description |
| ---------------------- | ----------- |
| AsyncTaskQueue		 | The constructor accepts the maximum number of threads to run tasks argument |
| Add					 | Adds an asynchronous task to the queue. |
| GetNext				 | Returns a next task from the queue according to priority. Returns `null` if the queue is empty. |
| Clear					 | Removes all tasks from the queue and interrupts current tasks. |

## Installation
To add this module to your project, run the command:
```bash
git submodule add https://github.com/mofrison/AsyncTaskQueue
```

## Usage
Simple example of use:

```csharp
AsyncTaskQueue taskQueue = new AsyncTaskQueue();

for(int i=0; i<5; i++)
{
    taskQueue.Add(async (cancellationToken) => {
        await Task.Run(() => {
            int j = 10;
            while (j-- > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine(string.Format("Canceled task, TaskQueue.Count: {0}", taskQueue.Count));
                    return;
                }
                Thread.Sleep(200);
            }
            Console.WriteLine(string.Format("Finished task, TaskQueue.Count: {0}", taskQueue.Count));
        });
    });
}

var asyncTask = taskQueue.GetNext();
while (asyncTask != null)
{
    await asyncTask.Run();
    try
    {
        asyncTask = taskQueue.GetNext();
    }
    catch (AsyncTaskQueue.Exception e) 
    {
        Console.WriteLine(e.Message);
    }
}
```

