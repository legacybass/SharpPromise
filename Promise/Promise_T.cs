using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpPromise
{

	public class Promise<T> : Promise, IPromise<T>
	{
		/// <summary>
		/// Returns a promise that is resolved with the <paramref name="arg"/> value.
		/// </summary>
		/// <param name="arg">Value to use to resolve this promise</param>
		/// <returns></returns>
		public static IPromise<T> Resolve(T arg) => new Promise<T>(Task<T>.FromResult(arg));
		/// <summary>
		/// Returns a promise that is rejected with the <paramref name="ex"/> exception.
		/// </summary>
		/// <param name="ex">Exception used to reject this promise.</param>
		/// <returns></returns>
		public new static IPromise<T> Reject(Exception ex) => new Promise<T>(Task<T>.FromException<T>(ex));

		/// <summary>
		/// Returns a promise that will resolve when all promises passed in are resolved.
		/// The final promise will contain the results of the passed in promises.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="promises"></param>
		/// <returns></returns>
		public static IPromise<T[]> All(params IPromise<T>[] promises)
		{
			if (promises == null || promises.Length == 0)
				return Promise<T[]>.Resolve(Enumerable.Empty<T>().ToArray());

			var waiting = promises.Select(async p => await p);

			var task = System.Threading.Tasks.Task<T>.WhenAll(waiting);

			return new Promise<T[]>(task);
		}

		/// <summary>
		/// Returns a promise that will resolve when all passed in tasks are resolved.
		/// If any is rejected, it will stop waiting and reject the final promise.
		/// </summary>
		/// <param name="tasks">Tasks on which to wait.</param>
		/// <returns></returns>
		public static IPromise<T[]> All(params Task<T>[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return Promise<T[]>.Resolve(Enumerable.Empty<T>().ToArray());

			var t = Task<T>.WhenAll(tasks);
			return new Promise<T[]>(t);
		}

		public static implicit operator Task<T>(Promise<T> promise) => promise.Task;
		public static implicit operator Promise<T>(Task<T> task) => new Promise<T>(task);

		protected Task<T> Task { get; set; }
		protected override Task BackingTask { get => Task; }

#pragma warning disable CC0031 // Check for null before calling a delegate
		/// <summary>
		/// Creates a promise that can be resolved with the passed in <see cref="Action{T}"/>.
		/// The value passed to the <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
		/// </summary>
		/// <param name="callback">Callback that can use the first parameter to resolve the promise.</param>
		public Promise(Action<Action<T>> callback) : this(callback == null ? null : (Action<Action<T>, Action>)((resolve, reject) => callback(resolve)))
#pragma warning restore CC0031 // Check for null before calling a delegate
		{
		}

#pragma warning disable CC0031 // Check for null before calling a delegate
		/// <summary>
		/// Creates a promise that can be resolved or rejected with the passed in <see cref="Action{T1, T2}"/>.
		/// The value passed to the resolve <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
		/// </summary>
		/// <param name="callback">Callback that can use the first parameter to resolve the promise,
		/// and the second parameter to reject the promise.</param>
		public Promise(Action<Action<T>, Action> callback) : this(callback == null ? null : (Action<Action<T>, Action<Exception>>)((resolve, reject) => callback(resolve, () => reject(new Exception()))))
#pragma warning restore CC0031 // Check for null before calling a delegate
		{
		}

		/// <summary>
		/// Creates a promise that can be resolved or rejected with the passed in <see cref="Action{T1, T2}"/>.
		/// The value passed to the resolve <see cref="Action{T}"/> will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
		/// The value passed to the reject <see cref="Action{T}"/> will be used as the parameter to any
		/// <see cref="Then(Action{T}, Action{Exception})"/> calls, or any <see cref="IPromise.Catch(Action{Exception})"/>
		/// calls.
		/// </summary>
		/// <param name="callback">Callback that can use the first parameter to resolve the promise,
		/// and the second parameter to reject the promise with a given exception.</param>
		public Promise(Action<Action<T>, Action<Exception>> callback) : base(Task<T>.CompletedTask)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback), "Must provide a callback for a new promise.");

			var builder = new AsyncTaskMethodBuilder<T>();
			Task = builder.Task;

			try
			{
				callback(arg => builder.SetResult(arg), ex => builder.SetException(ex));
			}
			catch (Exception e)
			{
				builder.SetException(e);
			}
		}

		/// <summary>
		/// Creates a promise from the passed in task. The resolve parameter for the <see cref="Task"/>
		/// will be used as the parameter to any <see cref="Then(Action{T})"/> calls.
		/// </summary>
		/// <param name="task"><see cref="Task{TResult}"/> from which to create the promise</param>
		public Promise(Task<T> task) : base(Task<T>.CompletedTask)
		{
			Task = task ?? throw new ArgumentNullException(nameof(task), "Task cannot be null");
		}

		public IPromise Then(Action<T> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Action<T> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise Then(Action<T> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var resultTask = new TaskCompletionSource<int>();

			Task.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					try
					{
						onRejected?.Invoke(task.Exception);
					}
					catch (Exception e)
					{
						resultTask.SetException(e);
					}
				}
				else
				{
					try
					{
						onFulfilled?.Invoke(task.Result);
						resultTask.SetResult(42);
					}
					catch (Exception e)
					{
						resultTask.SetException(e);
					}
				}
			});

			return new Promise(resultTask.Task);
		}

		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var resultTask = new TaskCompletionSource<TResult>();

			Task.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					try
					{
						onRejected?.Invoke(task.Exception);
					}
					catch (Exception e)
					{
						resultTask.SetException(e);
					}
				}
				else if (onFulfilled != null)
				{
					try
					{
						var result = onFulfilled(task.Result);
						resultTask.SetResult(result);
					}
					catch (Exception e)
					{
						resultTask.SetException(e);
					}
				}

				return default(TResult);
			});

			return new Promise<TResult>(resultTask.Task);
		}

		public IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var completionSource = new TaskCompletionSource<TResult>();

#pragma warning disable CC0031 // Check for null before calling a delegate
			Task.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					onRejected(task.Exception);
					completionSource.SetException(task.Exception);
				}
				else
				{
					onFulfilled(task.Result).ContinueWith(t =>
					{
						if (t.IsFaulted)
						{
							completionSource.SetException(t.Exception);
						}
						else
						{
							completionSource.SetResult(t.Result);
						}
					});
				}
			});
#pragma warning restore CC0031 // Check for null before calling a delegate

			return new Promise<TResult>(completionSource.Task);
		}

		public IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<TResult> Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var completionSource = new TaskCompletionSource<TResult>();

#pragma warning disable CC0031 // Check for null before calling a delegate
			Task.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					try
					{
						onRejected(task.Exception);
						completionSource.SetException(task.Exception);
					}
					catch (Exception e)
					{
						completionSource.SetException(e);
					}
				}
				else
				{
					try
					{
						var prom = onFulfilled(task.Result);

						if (prom != null)
						{
							prom.Then(result => completionSource.SetResult(result))
							.Catch(ex => completionSource.SetException(ex));
						}
						else
							completionSource.SetResult(default);
					}
					catch (Exception e)
					{
						completionSource.SetException(e);
					}
				}
			});
#pragma warning restore CC0031 // Check for null before calling a delegate

			return new Promise<TResult>(completionSource.Task);
		}

		public new IPromise<T> Finally(Action onFinal)
		{
			var waiter = new AsyncTaskMethodBuilder<T>();

			Task.ContinueWith(t =>
			{
				onFinal?.Invoke();

				if (t.IsFaulted)
				{
					Exception ex = t.Exception;
					while(ex is AggregateException ae)
					{
						ex = ae.InnerException;
					}

					waiter.SetException(ex);
				}
				else
					waiter.SetResult(t.Result);
			});

			return new Promise<T>(waiter.Task);
		}

		TaskAwaiter<T> IPromise<T>.GetAwaiter() => Task.GetAwaiter();
	}
}
