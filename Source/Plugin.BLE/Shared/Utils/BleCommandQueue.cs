using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Utils
{
    public class BleCommandQueue
    {
        public Queue<IBleCommand> CommandQueue { get; set; }

        private object _lock = new object();
        private IBleCommand _currentCommand;

        public BleCommandQueue()
        {

        }

        public Task<T> EnqueueAsync<T>(Func<Task<T>> bleCommand, int timeOutInSeconds = 10)
        {
            var command = new BleCommand<T>(bleCommand, timeOutInSeconds);
            lock (_lock)
            {
                CommandQueue.Enqueue(command);
            }

            TryExecuteNext();
            return command.ExecutingTask;
        }

        public void CancelPending()
        {
            lock (_lock)
            {
                foreach (var command in CommandQueue)
                {
                    command.Cancel();
                }
                CommandQueue.Clear();
            }
        }

        private async void TryExecuteNext()
        {
            lock (_lock)
            {
                if (_currentCommand != null || !CommandQueue.Any())
                {
                    return;
                }

                _currentCommand = CommandQueue.Dequeue();
            }

            await _currentCommand.ExecuteAsync();

            lock (_lock)
            {
                _currentCommand = null;
            }

            TryExecuteNext();

        }
    }


    public interface IBleCommand
    {
        Task ExecuteAsync();
        void Cancel();
        bool IsExecuting { get; }
        int TimeoutInMiliSeconds { get; }
    }

    public class BleCommand<T> : IBleCommand
    {
        private Func<Task<T>> _taskSource;
        private TaskCompletionSource<T> _taskCompletionSource;

        public int TimeoutInMiliSeconds { get; }

        public BleCommand(Func<Task<T>> taskSource, int timeoutInSeconds)
        {
            _taskSource = taskSource;
            TimeoutInMiliSeconds = timeoutInSeconds;
            _taskCompletionSource = new TaskCompletionSource<T>();
        }

        public Task<T> ExecutingTask => _taskCompletionSource.Task;

        public bool IsExecuting { get; private set; }

        public async Task ExecuteAsync()
        {
            try
            {
                IsExecuting = true;
                var source = _taskSource();
                if (source != await Task.WhenAny(source, Task.Delay(TimeoutInMiliSeconds)))
                {
                    throw new TimeoutException("Timed out while executing ble task.");
                }

                _taskCompletionSource.TrySetResult(await source);
            }
            catch (TaskCanceledException)
            {
                _taskCompletionSource.TrySetCanceled();
            }
            catch (Exception ex)
            {
                _taskCompletionSource.TrySetException(ex);
            }
            finally
            {
                IsExecuting = false;
            }

        }

        public void Cancel()
        {
            _taskCompletionSource.TrySetCanceled();
        }
    }
}

