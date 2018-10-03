using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SharpPromise
{
	public class Promise : IPromise
	{
		public static IPromise Resolve() => new Promise(Task.CompletedTask);
		public static IPromise Reject(Exception e) => new Promise(Task.FromException(e));

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
			if (task == null)
				throw new ArgumentNullException(nameof(task), "Must provide a task for a new promise.");

			BackingTask = task;
		}

		public IPromise Then(Action onFullfilled) => Then(onFullfilled, (Action)null);
		public IPromise Then(Action onFullfilled, Action onRejected) => Then(onFullfilled, e => onRejected?.Invoke());
		public IPromise Then(Action onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Resolved callback cannot be null");
			if (onRejected == null)
				throw new ArgumentNullException(nameof(onRejected), "Rejected callback cannot be null");

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

		public IPromise<T> Then<T>(Func<T> onFullfilled) => Then(onFullfilled, (Action)null);
		public IPromise<T> Then<T>(Func<T> onFullfilled, Action onRejected) => Then(onFullfilled, ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<T> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Resolved callback cannot be null");
			if (onRejected == null)
				throw new ArgumentNullException(nameof(onRejected), "Rejected callback cannot be null");

			var resultTask = BackingTask.ContinueWith<T>(task =>
			{
				if (task.IsFaulted)
				{
					onRejected(task.Exception);
					throw task.Exception.InnerException;
				}

				return onFulfilled();
			});

			return new Promise<T>(resultTask);
		}

		public IPromise<T> Then<T>(Func<IPromise<T>> onFullfilled) => Then(onFullfilled, (Action)null);
		public IPromise<T> Then<T>(Func<IPromise<T>> onFullfilled, Action onRejected) => Then(onFullfilled, ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Resolved callback cannot be null");
			if (onRejected == null)
				throw new ArgumentNullException(nameof(onRejected), "Rejected callback cannot be null");

			var completionSource = new TaskCompletionSource<T>();

			BackingTask.ContinueWith(task =>
			{
				if(task.IsFaulted)
				{
					onRejected(task.Exception);
					completionSource.SetException(task.Exception);
				}
				else
					onFulfilled().Then(result => completionSource.SetResult(result));
			});

			return new Promise<T>(completionSource.Task);
		}

		public IPromise<T> Then<T>(Func<Promise<T>> onFullfilled) => Then(onFullfilled, (Action)null);
		public IPromise<T> Then<T>(Func<Promise<T>> onFullfilled, Action onRejected) => Then(onFullfilled, ex => onRejected?.Invoke());
		public IPromise<T> Then<T>(Func<Promise<T>> onFulfilled, Action<Exception> onRejected) => Then((Func<IPromise<T>>)onFulfilled, onRejected);

		public IPromise Catch(Action<Exception> onError)
		{
			var resultTask = BackingTask.ContinueWith(task =>
			{
				if (task.IsFaulted)
					onError?.Invoke(task.Exception);
			});

			return new Promise(resultTask);
		}

		public TaskAwaiter GetAwaiter() => BackingTask.GetAwaiter();
	}

	public class Promise<T> : Promise, IPromise<T>
	{
		public static IPromise<T> Resolve(T arg) => new Promise<T>(Task<T>.FromResult(arg));
		public new static IPromise<T> Reject(Exception ex) => new Promise<T>(Task<T>.FromException<T>(ex));
		public static implicit operator Task<T>(Promise<T> promise) => promise.Task;
		public static implicit operator Promise<T>(Task<T> task) => new Promise<T>(task);

		protected Task<T> Task { get; set; }
		protected override Task BackingTask { get => Task; }

		public Promise(Action<Action<T>> callback) : this(callback == null ? null : (Action<Action<T>, Action>)((resolve, reject) => callback(resolve)))
		{
		}

		public Promise(Action<Action<T>, Action> callback) : this(callback == null ? null : (Action<Action<T>, Action<Exception>>)((resolve, reject) => callback(resolve, () => reject(new Exception()))))
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
			catch(Exception e)
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
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Resolved callback cannot be null.");
			if (onRejected == null)
				throw new ArgumentNullException(nameof(onRejected), "Rejected callback cannot be null.");

			var resultTask = Task.ContinueWith(task =>
			{
				if (task.IsFaulted)
					onRejected?.Invoke(task.Exception);
				else
					onFulfilled?.Invoke(task.Result);
			});

			return new Promise(resultTask);
		}

		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFullfilled) => Then(onFullfilled, (Action)null);
		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected) => Then(onFulfilled, ex => onRejected?.Invoke());
		public IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Exception> onRejected)
		{
			if (onFulfilled == null)
				throw new ArgumentNullException(nameof(onFulfilled), "Resolved callback cannot be null");
			if (onRejected == null)
				throw new ArgumentNullException(nameof(onRejected), "Rejected callback cannot be null");

			var resultTask = Task.ContinueWith<TResult>(task =>
			{
				if(task.IsFaulted)
				{
					onRejected?.Invoke(task.Exception);
				}
				else if(onFulfilled != null)
				{
					return onFulfilled(task.Result);
				}

				return default(TResult);
			});

			return new Promise<TResult>(resultTask);
		}
	}
}
