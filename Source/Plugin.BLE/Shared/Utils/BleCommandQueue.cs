using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Utils
{
    /// <summary>
    /// A BLE command queue.
    /// </summary>
    public class BleCommandQueue
    {
        /// <summary>
        /// The actual queue of BLE commands.
        /// </summary>
        public Queue<IBleCommand> CommandQueue { get; set; }

        private object _lock = new object();
        private IBleCommand _currentCommand;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BleCommandQueue()
        {
        }

        /// <summary>
        /// Enqueue a command with a given timeout.
        /// </summary>
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

        /// <summary>
        /// Cancel all pending commands.
        /// </summary>
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

        /// <summary>
        /// Try to execute the next command in the queue.
        /// </summary>
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


    /// <summary>
    /// BLE command interface.
    /// </summary>
    public interface IBleCommand
    {
        /// <summary>
        /// Execute the command.
        /// </summary>
        Task ExecuteAsync();
        /// <summary>
        /// Cancel the command.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Indicates whether the command is currently executing.
        /// </summary>
        bool IsExecuting { get; }
        /// <summary>
        /// Timeout of the command in milliseconds.
        /// </summary>
        int TimeoutInMiliSeconds { get; }
    }

    /// <summary>
    /// A BLE command.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BleCommand<T> : IBleCommand
    {
        private Func<Task<T>> _taskSource;
        private TaskCompletionSource<T> _taskCompletionSource;

        /// <summary>
        /// Timeout of the command in milliseconds.
        /// </summary>
        public int TimeoutInMiliSeconds { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public BleCommand(Func<Task<T>> taskSource, int timeoutInSeconds)
        {
            _taskSource = taskSource;
            TimeoutInMiliSeconds = timeoutInSeconds;
            _taskCompletionSource = new TaskCompletionSource<T>();
        }

        /// <summary>
        /// The executing task.
        /// </summary>
        public Task<T> ExecutingTask => _taskCompletionSource.Task;

        /// <summary>
        /// Indicates whether the command is currently executing.
        /// </summary>
        public bool IsExecuting { get; private set; }

        /// <summary>
        /// Execute the command.
        /// </summary>
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

        /// <summary>
        /// Cancel the command.
        /// </summary>
        public void Cancel()
        {
            _taskCompletionSource.TrySetCanceled();
        }
    }
}

