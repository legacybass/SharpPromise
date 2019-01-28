using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPromise
{
	public class Promise : IPromise
	{
		/// <summary>
		/// Returns a promise that is already resolved.
		/// </summary>
		/// <remarks>This is useful for wrapping code in a promise without having to worry if the callback in
		/// the Promise constructor throws an exception.</remarks>
		/// <returns></returns>
		public static IPromise Resolve() => new Promise(Task.CompletedTask);
		/// <summary>
		/// Returns a promise that has already been rejected.
		/// </summary>
		/// <param name="e">The exception with which to reject the promise.</param>
		/// <returns></returns>
		public static IPromise Reject(Exception e) => new Promise(Task.FromException(e));

		/// <summary>
		/// Returns a promise that will resolve when all promises passed in are resolved.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="promises">Promises to wait on.</param>
		/// <returns></returns>
		public static IPromise All(params IPromise[] promises) => All(promises?.Select(async p => await p));

		/// <summary>
		/// Returns a promise that will resolve when all promises in the Enumerable are resolved.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="promises">Promises to wait on.</param>
		/// <returns></returns>
		public static IPromise All(IEnumerable<IPromise> promises) => All(promises?.ToArray());

		/// <summary>
		/// Returns a promise that will resolve when all passed in tasks are resolved.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="tasks">Tasks on which to wait.</param>
		/// <returns></returns>
		public static IPromise All(params Task[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return Resolve();

			var task = Task.WhenAll(tasks);
			return new Promise(task);
		}

		/// <summary>
		/// Returns a promise that will resolve when all tasks in the Enumerable are resolved.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public static IPromise All(IEnumerable<Task> tasks) => All(tasks?.ToArray());

		/// <summary>
		/// Returns a promise that will resolve when all promises passed in are resolved.
		/// The final promise will contain the results of the passed in promises. You will
		/// need to cast them to their final types.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="promises"></param>
		/// <returns></returns>
		public static IPromise<object[]> All(params IPromise<object>[] promises) => All(promises?.Select(async p => await p));

		/// <summary>
		/// Returns a promise that will resolve when all promises passed in are resolved.
		/// The final promise will contain the results of the passed in promises. You will
		/// need to cast them to their final types.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="promises"></param>
		/// <returns></returns>
		public static IPromise<object[]> All(IEnumerable<IPromise<object>> promises) => All(promises?.ToArray());

		/// <summary>
		/// Returns a promise that will resolve when all tasks passed in are resolved.
		/// The final promise will contain the results of the passed in tasks. You will
		/// need to cast them to their final types.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public static IPromise<object[]> All(params Task<object>[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return new Promise<object[]>(Task.FromResult(Enumerable.Empty<object>().ToArray()));

			var task = Task.WhenAll(tasks);
			return new Promise<object[]>(task);
		}

		/// <summary>
		/// Returns a promise that will resolve when all tasks in the Enumerable are resolved.
		/// The final promise will contain the results of the passed in tasks. You will
		/// need to cast them to their final types.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="Task{TResult}"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise<object[]> All(IEnumerable<Task<object>> tasks) => All(tasks?.ToArray());


		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="promises"><see cref="IPromise"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise Any(params IPromise[] promises) => Any(promises?.Select(async p => await p));

		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise Any(IEnumerable<IPromise> promises) => Any(promises?.ToArray());

		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise Any(params Task[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return Resolve();

			return new Promise(Task.WhenAny(tasks));
		}

		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="Task"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise Any(IEnumerable<Task> tasks) => Any(tasks?.ToArray());

		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="tasks"><see cref="Task{TResult}"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise<object> Any(params Task<object>[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return Promise<object>.Resolve(null);

			var task = Task.WhenAny(tasks);

			var builder = new TaskCompletionSource<object>();
			task.ContinueWith(t =>
			{
				if (t.IsCompleted)
					builder.SetResult(t.Result.Result);
				else
					builder.SetException(t.Exception);
			});

			return new Promise<object>(builder.Task);
		}
		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the tasks resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="Task{TResult}"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise<object> Any(IEnumerable<Task<object>> tasks) => Any(tasks?.ToArray());

		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="promises"><see cref="IPromise{T}"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise<object> Any(params IPromise<object>[] promises) => Any(promises?.Select(async p => await p));

		/// <summary>
		/// Returns a <see cref="IPromise"/> that resolves if any of the promises resolve. If none do, it
		/// resolves with the first to reject.
		/// </summary>
		/// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise{T}"/>s on which to wait</param>
		/// <returns></returns>
		public static IPromise<object> Any(IEnumerable<IPromise<object>> promises) => Any(promises?.ToArray());

		/// <summary>
		/// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the promises
		/// in the <see cref="IEnumerable{T}"/> resolves.
		/// </summary>
		/// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise Race(IEnumerable<IPromise> promises) => Race(promises?.ToArray());
		/// <summary>
		/// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
		/// </summary>
		/// <param name="promises"><see cref="IPromise"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise Race(params IPromise[] promises) => Race(promises.Select(async p => await p));
		/// <summary>
		/// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise Race(IEnumerable<Task> tasks) => Race(tasks?.ToArray());
		/// <summary>
		/// Returns a <see cref="IPromise"/> that is resolved as soon as any one of the passed in promises resolves.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise Race(params Task[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return Resolve();

			var task = new TaskCompletionSource<int>();

			foreach(var t in tasks)
			{
				t.ContinueWith(result =>
				{
					if(result.IsFaulted)
					{
						task.TrySetException(UnwrapException(result.Exception));
					}
					else
					{
						task.TrySetResult(42);
					}
				});
			}

			return new Promise(task.Task);
		}

		/// <summary>
		/// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
		/// </summary>
		/// <param name="promises"><see cref="IEnumerable{T}"/> of <see cref="IPromise{T}"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise<object> Race(IEnumerable<IPromise<object>> promises) => Race(promises?.ToArray());
		/// <summary>
		/// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
		/// </summary>
		/// <param name="promises"><see cref="IPromise{T}"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise<object> Race(params IPromise<object>[] promises) => Race(promises?.Select(async p => await p));
		/// <summary>
		/// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
		/// </summary>
		/// <param name="tasks"><see cref="IEnumerable{T}"/> of <see cref="Task{TResult}"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise<object> Race(IEnumerable<Task<object>> tasks) => Race(tasks?.ToArray());
		/// <summary>
		/// Returns a <see cref="IPromise{T}"/> that is resolved as soon as any one of the passed in promises resolves.
		/// </summary>
		/// <param name="tasks"><see cref="Task{TResult}"/>s on which to wait.</param>
		/// <returns></returns>
		public static IPromise<object> Race(params Task<object>[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return Promise<object>.Resolve(null);

			var task = new TaskCompletionSource<object>();

			foreach(var t in tasks)
			{
				t.ContinueWith(result =>
				{
					if (result.IsFaulted)
						task.TrySetException(UnwrapException(result.Exception));
					else
						task.TrySetResult(result.Result);
				});
			}

			return new Promise<object>(task.Task);
		}

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

		/// <summary>
		/// Creates a promise that can be resolved with the passed in callback
		/// </summary>
		/// <param name="callback">Callback that can use the first parameter to resolve the promise.</param>
		public Promise(Action<Action> callback) :
			this(callback == null
				? null
				: ((Action<Action, Action>)((resolve, reject) => callback?.Invoke(resolve))))
		{
		}

		/// <summary>
		/// Creates a promise that can be resolved or rejeted with the passed in callback.
		/// </summary>
		/// <param name="callback">Callback that can use the first parameter to resolve the promise,
		/// and the second parameter to reject the promise.</param>
		public Promise(Action<Action, Action> callback) :
			this(callback == null
				? null
				: ((Action<Action, Action<Exception>>)((resolve, reject) => callback?.Invoke(resolve, () => reject?.Invoke(new Exception())))))
		{
		}

		/// <summary>
		/// Creates a promise that can be resolved or rejeted with the passed in callback.
		/// </summary>
		/// <param name="callback">Callback that can use the first parameter to resolve the promise,
		/// and the second parameter to reject the promise with a given exception.</param>
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

		/// <summary>
		/// Creates a promise from the passed in task.
		/// </summary>
		/// <param name="task">Task from which to create the promise</param>
		public Promise(Task task)
		{
			BackingTask = task ?? throw new ArgumentNullException(nameof(task), "Must provide a task for a new promise.");
		}

		public IPromise Then(Action onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Action onFulfilled, Action onRejected) =>
			Then(onFulfilled, onRejected == null ? (Action<Exception>)null : e => onRejected.Invoke());
		public IPromise Then(Action onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Fulfilled callback cannot be null.");

			var result = new TaskCompletionSource<int>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					var inner = task.Exception.InnerException;

					if (inner is AggregateException i)
						inner = i.InnerException ?? i;

					if (onRejected != null)
					{
						onRejected(inner);
						result.SetResult(42);
					}
					else
						result.SetException(inner);
				}
				else
				{
					try
					{
						onFulfilled?.Invoke();
						result.SetResult(42);
					}
					catch (Exception e)
					{
						result.SetException(e);
					}
				}
			});

			return new Promise(result.Task);
		}

		public IPromise<T> Then<T>(Func<T> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected) =>
			Then(onFulfilled, onRejected == null ? (Action<Exception>)null : ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<T> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Fulfilled callback cannot be null.");

			var resultTask = new TaskCompletionSource<T>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					if (onRejected != null)
					{
						onRejected(task.Exception);
						resultTask.SetResult(default);
					}
					else
						resultTask.SetException(task.Exception);
				}
				else
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					try
					{
						var result = onFulfilled();
						resultTask.SetResult(result);
					}
					catch (Exception e)
					{
						resultTask.SetException(e);
					}
#pragma warning restore CC0031 // Check for null before calling a delegate
				}
			});

			return new Promise<T>(resultTask.Task);
		}

		public IPromise Then(Func<IPromise> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Func<IPromise> onFulfilled, Action onRejected) =>
			Then(onFulfilled, onRejected == null ? (Action<Exception>)null : ex => onRejected?.Invoke());
		public IPromise Then(Func<IPromise> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Fulfilled callback cannot be null.");

			var completionSource = new TaskCompletionSource<int>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					if(onRejected != null)
					{
						onRejected(task.Exception);
						completionSource.SetResult(0);
					}
					else
						completionSource.SetException(task.Exception);
				}
				else
				{
					try
					{
#pragma warning disable CC0031 // Check for null before calling a delegate
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
		public IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action onRejected) =>
			Then(onFulfilled, onRejected == null ? (Action<Exception>)null : ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Fulfilled callback cannot be null.");

			var completionSource = new TaskCompletionSource<T>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					if(onRejected != null)
					{
						try
						{
							onRejected(task.Exception);
							completionSource.SetResult(default);
						}
						catch (Exception e)
						{
							completionSource.SetException(e);
						}
					}
					else
						completionSource.SetException(task.Exception);
				}
				else
				{
					try
					{
						var prom = onFulfilled();

						if (prom == null)
							completionSource.SetResult(default);
						else
						{
							prom.Then(result => completionSource.SetResult(result))
							.Catch(ex => completionSource.SetException(ex));
						}
					}
					catch(Exception e)
					{
						completionSource.SetException(e);
					}
				}
			});

			return new Promise<T>(completionSource.Task);
		}

		public IPromise<T> Then<T>(Func<Promise<T>> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action onRejected) =>
			Then(onFulfilled, onRejected == null ? (Action<Exception>)null : ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action<Exception> onRejected) => Then((Func<IPromise<T>>)onFulfilled, onRejected);


		public IPromise Then(Func<Task> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Func<Task> onFulfilled, Action onRejected) =>
			Then(onFulfilled, onRejected == null ? (Action<Exception>)null : ex => onRejected?.Invoke());
		public IPromise Then(Func<Task> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Fulfilled callback cannot be null.");

			var completionSource = new TaskCompletionSource<object>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					if(onRejected != null)
					{
						onRejected(task.Exception);
						completionSource.SetResult(null);
					}
					else
						completionSource.SetException(task.Exception);
				}
				else
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					var result = onFulfilled();
#pragma warning restore CC0031 // Check for null before calling a delegate

					if (result == null)
						completionSource.SetResult(null);
					else
					{
						result.ContinueWith(t =>
						{
							if (t.IsFaulted)
							{
								completionSource.SetException(t.Exception);
							}
							else
							{
								completionSource.SetResult(null);
							}
						});
					}
				}
			});

			return new Promise(completionSource.Task);
		}

		public IPromise<T> Then<T>(Func<Task<T>> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action onRejected) =>
			Then(onFulfilled, onRejected == null ? (Action<Exception>)null : ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Fulfilled callback cannot be null.");

			var completionSource = new TaskCompletionSource<T>();

			BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					if(onRejected != null)
					{
						onRejected(task.Exception);
						completionSource.SetResult(default);
					}
					else
						completionSource.SetException(task.Exception);
				}
				else
				{
#pragma warning disable CC0031 // Check for null before calling a delegate
					var result = onFulfilled();
					if (result == null)
						completionSource.SetResult(default);
					else
					{
						result.ContinueWith(t =>
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
					Exception ex = task.Exception;

					while (ex is AggregateException ae)
						ex = ae.InnerException;

					onError?.Invoke(ex);
				}
			});

			return new Promise(resultTask);
		}

		public IPromise Finally(Action onFinal)
		{
			var result = BackingTask.ContinueWith(t =>
			{
				onFinal?.Invoke();
			});

			return new Promise(result);
		}

		public TaskAwaiter GetAwaiter() => BackingTask.GetAwaiter();

		protected static bool ValidCallbacks(object fulfilled, object rejected, string fulfilledName, string rejectedName)
		{
			if(fulfilled == null)
				throw new ArgumentNullException(fulfilledName, "Resolved callback cannot be null");

			return true;
		}

		protected static Exception UnwrapException(AggregateException aggregate)
		{
			Exception e = aggregate;

			while (e is AggregateException ae)
			{
				if(ae.InnerException == null)
					break;

				e = ae.InnerException;
			}

			return e;
		}
	}
}
