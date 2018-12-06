using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SharpPromise
{
	public class Promise : IPromise
	{
		public static IPromise Resolve() => new Promise(Task.CompletedTask);
		public static IPromise Reject(Exception e) => new Promise(Task.FromException(e));

		public static IPromise All(params IPromise[] promises)
		{
			if (promises == null || promises.Length == 0)
				return Resolve();

			var waiting = promises.Select(async p => await p);

			var task = Task.WhenAll(waiting);

			return new Promise(task);
		}

		public static IPromise All(IEnumerable<IPromise> promises) => All(promises.ToArray());

		public static IPromise All(params Task[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return Resolve();

			var task = Task.WhenAll(tasks);
			return new Promise(task);
		}

		public static IPromise All(IEnumerable<Task> tasks) => All(tasks.ToArray());

		public static implicit operator Task(Promise promise)
		{
			return promise.BackingTask;
		}

		public static implicit operator Promise(Task task)
		{
			return new Promise(task);
		}

		public PromiseState State
		{
			get
			{
				if(BackingTask.IsCompleted)
				{
					if (BackingTask.IsFaulted)
						return PromiseState.Rejected;

					return PromiseState.Fulfilled;
				}

				return PromiseState.Pending;
			}
		}
		protected virtual Task BackingTask { get; set; }

		public Promise(Action<Action> callback) :
			this(callback == null
				? null
				: ((Action<Action, Action>)((resolve, reject) => callback?.Invoke(resolve))))
		{
		}

		public Promise(Action<Action, Action> callback) :
			this(callback == null
				? null
				: ((Action<Action, Action<Exception>>)((resolve, reject) => callback?.Invoke(resolve, () => reject?.Invoke(new Exception())))))
		{
		}

		public Promise(Action<Action, Action<Exception>> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback), "Must provide a callback for a new promise.");

			var builder = new AsyncTaskMethodBuilder();

			BackingTask = builder.Task;

			try
			{
				callback(() => builder.SetResult(), ex => builder.SetException(ex));
			}
			catch (Exception e)
			{
				builder.SetException(e);
			}
		}

		public Promise(Task task)
		{
			BackingTask = task ?? throw new ArgumentNullException(nameof(task), "Must provide a task for a new promise.");
		}

		public IPromise Then(Action onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Action onFulfilled, Action onRejected) => Then(onFulfilled, e => onRejected?.Invoke());
		public IPromise Then(Action onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var resultTask = BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					var inner = task.Exception.InnerException;
					onRejected?.Invoke(inner);
					throw inner; // Apparently this needs to happen in order to get this task to fail.
				}
				else
					onFulfilled?.Invoke();
			});

			return new Promise(resultTask);
		}

		public IPromise<T> Then<T>(Func<T> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<T> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var resultTask = BackingTask.ContinueWith<T>(task =>
			{
				if (task.IsFaulted)
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					onRejected(task.Exception);
					throw task.Exception.InnerException;
				}

				return onFulfilled();
#pragma warning restore CC0031 // Check for null before calling a delegate
			});

			return new Promise<T>(resultTask);
		}

		public IPromise Then(Func<IPromise> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Func<IPromise> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise Then(Func<IPromise> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var completionSource = new TaskCompletionSource<int>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					onRejected(task.Exception);
					completionSource.SetException(task.Exception);
				}
				else
				{
					try
					{
						onFulfilled().Then(() => completionSource.SetResult(42))
						.Catch(ex => completionSource.SetException(ex));
#pragma warning restore CC0031 // Check for null before calling a delegate
					}
					catch (Exception ex)
					{
						completionSource.SetException(ex);
					}
				}
			});

			return new Promise(completionSource.Task);
		}

		public IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var completionSource = new TaskCompletionSource<T>();

			BackingTask.ContinueWith(task =>
			{
				if(task.IsFaulted)
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					onRejected(task.Exception);
					completionSource.SetException(task.Exception);
				}
				else
					onFulfilled().Then(result => completionSource.SetResult(result))
					.Catch(ex => completionSource.SetException(ex));
#pragma warning restore CC0031 // Check for null before calling a delegate
			});

			return new Promise<T>(completionSource.Task);
		}

		public IPromise<T> Then<T>(Func<Promise<T>> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action<Exception> onRejected) => Then((Func<IPromise<T>>)onFulfilled, onRejected);


		public IPromise Then(Func<Task> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Func<Task> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise Then(Func<Task> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var completionSource = new TaskCompletionSource<object>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					onRejected(task.Exception);
					completionSource.SetException(task.Exception);
				}
				else
				{
					onFulfilled().ContinueWith(t =>
#pragma warning restore CC0031 // Check for null before calling a delegate
					{
						if(t.IsFaulted)
						{
							completionSource.SetException(t.Exception);
						}
						else
						{
							completionSource.SetResult(null);
						}
					});
				}
			});

			return new Promise(completionSource.Task);
		}

		public IPromise<T> Then<T>(Func<Task<T>> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var completionSource = new TaskCompletionSource<T>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					onRejected(task.Exception);
					completionSource.SetException(task.Exception);
				}
				else
				{
					onFulfilled().ContinueWith(t =>
#pragma warning restore CC0031 // Check for null before calling a delegate
					{
						if (t.IsFaulted)
						{
							completionSource.SetException(t.Exception);
						}
						else
							completionSource.SetResult(t.Result);
					});
				}
			});

			return new Promise<T>(completionSource.Task);
		}

		public IPromise Catch(Action<Exception> onError)
		{
			var resultTask = BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					if(task.Exception is AggregateException ae)
					{
						onError?.Invoke(ae.InnerException);
					}
					else
						onError?.Invoke(task.Exception);
				}
			});

			return new Promise(resultTask);
		}

		public TaskAwaiter GetAwaiter() => BackingTask.GetAwaiter();

		protected static bool ValidCallbacks(object fulfilled, object rejected, string fulfilledName, string rejectedName)
		{
			if(fulfilled == null)
				throw new ArgumentNullException(fulfilledName, "Resolved callback cannot be null");
			if(rejected == null)
				throw new ArgumentNullException(rejectedName, "Rejected callback cannot be null");

			return true;
		}
	}
}
