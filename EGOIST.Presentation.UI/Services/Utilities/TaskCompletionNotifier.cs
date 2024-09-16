using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EGOIST.Presentation.UI.Services.Utilities;

/// <summary>
/// Watches a task and raises property-changed notifications when the task completes.
/// This class provides a thread-safe way to access the task's results and status.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
public sealed class TaskCompletionNotifier<TResult> : INotifyPropertyChanged
{
    private readonly Task<TResult> _task;
    private readonly object _lock = new();
    private TResult _result;
    private string _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskCompletionNotifier{TResult}"/> class.
    /// </summary>
    /// <param name="task">The task to watch. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="task"/> argument is null.</exception>
    public TaskCompletionNotifier(Task<TResult> task)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));

        if (!_task.IsCompleted)
        {
            var scheduler = SynchronizationContext.Current == null
                ? TaskScheduler.Current
                : TaskScheduler.FromCurrentSynchronizationContext();

            _task.ContinueWith(HandleTaskCompletion,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
        }
        else
        {
            // If the task is already completed, initialize the properties immediately
            InitializeResultAndErrorMessage();
        }
    }

    /// <summary>
    /// Gets the task being watched. This property never changes and is never <c>null</c>.
    /// </summary>
    public Task<TResult> Task => _task;
    
    /// <summary>
    /// Gets the result of the task. Returns the default value of <typeparamref name="TResult"/> 
    /// if the task is not completed successfully. This property is thread-safe.
    /// </summary>
    public TResult Result
    {
        get
        {
            lock (_lock)
            {
                return _result;
            }
        }
    }

    /// <summary>
    /// Gets the status of the task.
    /// </summary>
    public TaskStatus Status => _task.Status;

    /// <summary>
    /// Gets a value indicating whether the task has completed.
    /// </summary>
    public bool IsCompleted => _task.IsCompleted;

    /// <summary>
    /// Gets a value indicating whether the task has completed successfully.
    /// </summary>
    public bool IsSuccessfullyCompleted => _task.Status == TaskStatus.RanToCompletion;

    /// <summary>
    /// Gets a value indicating whether the task was canceled.
    /// </summary>
    public bool IsCanceled => _task.IsCanceled;

    /// <summary>
    /// Gets a value indicating whether the task faulted.
    /// </summary>
    public bool IsFaulted => _task.IsFaulted;

    /// <summary>
    /// Gets the aggregate exception that caused the task to fault. 
    /// Returns null if the task did not fault.
    /// </summary>
    public AggregateException? Exception => _task.Exception;

    /// <summary>
    /// Gets the inner exception of the aggregate exception that caused the task to fault. 
    /// Returns null if the task did not fault or the aggregate exception does not have an inner exception.
    /// </summary>
    public Exception? InnerException => Exception?.InnerException;

    /// <summary>
    /// Gets the error message of the inner exception. 
    /// Returns null if the task did not fault or the inner exception does not have an error message. 
    /// This property is thread-safe.
    /// </summary>
    public string ErrorMessage
    {
        get
        {
            lock (_lock)
            {
                return _errorMessage;
            }
        }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Handles the task completion and updates the relevant properties.
    /// </summary>
    /// <param name="task">The completed task.</param>
    private void HandleTaskCompletion(Task task)
    {
        InitializeResultAndErrorMessage();

        OnPropertyChanged(nameof(IsCompleted));

        if (task.IsCanceled)
        {
            OnPropertyChanged(nameof(IsCanceled));
        }
        else if (task.IsFaulted)
        {
            OnPropertyChanged(nameof(IsFaulted));
            OnPropertyChanged(nameof(ErrorMessage));
        }
        else
        {
            OnPropertyChanged(nameof(IsSuccessfullyCompleted));
            OnPropertyChanged(nameof(Result));
        }
    }

    /// <summary>
    /// Initializes the Result and ErrorMessage properties in a thread-safe manner.
    /// </summary>
    private void InitializeResultAndErrorMessage()
    {
        lock (_lock)
        {
            _result = _task.Status == TaskStatus.RanToCompletion ? _task.Result : default;
            _errorMessage = InnerException?.Message;
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}