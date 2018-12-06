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
		public static IPromise<T> Resolve(T arg) => new Promise<T>(Task<T>.FromResult(arg));
		public new static IPromise<T> Reject(Exception ex) => new Promise<T>(Task<T>.FromException<T>(ex));

		public static IPromise<T[]> All(params IPromise<T>[] promises)
		{
			if (promises == null || promises.Length == 0)
				return Promise<T[]>.Resolve(Enumerable.Empty<T>().ToArray());

			var waiting = promises.Select(async p => await p);

			var task = System.Threading.Tasks.Task<T>.WhenAll(waiting);

			return new Promise<T[]>(task);
		}

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
		public Promise(Action<Action<T>> callback) : this(callback == null ? null : (Action<Action<T>, Action>)((resolve, reject) => callback(resolve)))
#pragma warning restore CC0031 // Check for null before calling a delegate
		{
		}

#pragma warning disable CC0031 // Check for null before calling a delegate
		public Promise(Action<Action<T>, Action> callback) : this(callback == null ? null : (Action<Action<T>, Action<Exception>>)((resolve, reject) => callback(resolve, () => reject(new Exception()))))
#pragma warning restore CC0031 // Check for null before calling a delegate
		{
		}

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

		public Promise(Task<T> task) : base(Task<T>.CompletedTask)
		{
			Task = task ?? throw new ArgumentNullException(nameof(task), "Task cannot be null");
		}

		public IPromise Then(Action<T> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise Then(Action<T> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise Then(Action<T> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var resultTask = Task.ContinueWith(task =>
			{
				if (task.IsFaulted)
					onRejected?.Invoke(task.Exception);
				else
					onFulfilled?.Invoke(task.Result);
			});

			return new Promise(resultTask);
		}

		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled) => Then(onFulfilled, (Action)null);
		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Exception> onRejected)
		{
			ValidCallbacks(onFulfilled, onRejected, nameof(onFulfilled), nameof(onRejected));

			var resultTask = Task.ContinueWith<TResult>(task =>
			{
				if (task.IsFaulted)
				{
					onRejected?.Invoke(task.Exception);
				}
				else if (onFulfilled != null)
				{
					return onFulfilled(task.Result);
				}

				return default(TResult);
			});

			return new Promise<TResult>(resultTask);
		}

		TaskAwaiter<T> IPromise<T>.GetAwaiter() => Task.GetAwaiter();
	}
}
