using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace Lancher;

public class Looper {

    struct TaskItem {
        public object? state;
        public Func<object?, ValueTask>? runTask;
        public SendOrPostCallback? d;
        public Action<object?> action;

        public TaskItem(object? state, Func<object?, ValueTask>? runTask, SendOrPostCallback? d, Action<object?> action)
        {
            this.state = state;
            this.runTask = runTask;
            this.d = d;
            this.action = action;
        }
    }
    
    readonly ILogger? logger;

    static readonly Action<object?> dummyAction = _ => { };

    readonly BlockingCollection<TaskItem> callbacks = new();

    public Looper(ILogger? logger) {
        this.logger = logger;
    }

    public void RunCallback(object? state, Action<object?> action) {
        callbacks.Add(new TaskItem(state, null, null, action));
    }

    public void RunTask(object? state, Func<object?, ValueTask> runTask) {
        callbacks.Add(new TaskItem(state, runTask, null, dummyAction));
    }

    async void RunTaskInternal(TaskItem item) {
        try {
            await item!.runTask!.Invoke(item!.state);
        }
        catch (Exception ex) {
            logger?.LogError(ex, "Error in task");
        }
    }

    public void Start(CancellationToken token) {
        var prevContext = SynchronizationContext.Current;
        try {
            SynchronizationContext.SetSynchronizationContext(new LooperSynchronisedContext(this));
            foreach (var item in callbacks.GetConsumingEnumerable(token)) {
                try {
                    if (item.runTask != null) {
                        RunTaskInternal(item);
                    }
                    else if (item.d != null) {
                        item.d(item.state);
                    }
                    else {
                        item.action(item.state);
                    }
                }
                catch (Exception ex) {
                    logger?.LogError(ex, "Error in callback");
                }
            }
        }
        finally {
            SynchronizationContext.SetSynchronizationContext(prevContext);
        }
    }

    class LooperSynchronisedContext : SynchronizationContext {
        readonly Looper looper;
        
        public LooperSynchronisedContext(Looper looper) {
            this.looper = looper;
        }

        public override void Post(SendOrPostCallback d, object? state) {
            looper.callbacks.Add(new TaskItem(state, null, d, Looper.dummyAction));
        }
    }
}