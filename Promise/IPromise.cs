using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpPromise
{
	public enum PromiseState
	{
		Pending,
		Fulfilled,
		Rejected
	}

	public interface IPromise
	{
		PromiseState State { get; }
		IPromise Catch(Action<Exception> onError);

		IPromise Then(Action onFulfilled);
		IPromise Then(Action onFulfilled, Action onRejected);
		IPromise Then(Action onFufilled, Action<Exception> onRejected);

		IPromise<T> Then<T>(Func<T> onFulfilled);
		IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected);
		IPromise<T> Then<T>(Func<T> onFulfilled, Action<Exception> onRejected);

		IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled);
		IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action onRejected);
		IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Exception> onRejected);

		IPromise Then(Func<IPromise> onFulfilled);
		IPromise Then(Func<IPromise> onFulfilled, Action onRejected);
		IPromise Then(Func<IPromise> onFulfilled, Action<Exception> onRejected);

		IPromise Then(Func<Task> onFulfilled);
		IPromise Then(Func<Task> onFulfilled, Action onRejected);
		IPromise Then(Func<Task> onFulfilled, Action<Exception> onRejected);

		IPromise<T> Then<T>(Func<Task<T>> onFulfilled);
		IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action onRejected);
		IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action<Exception> onRejected);

		TaskAwaiter GetAwaiter();
	}

	public interface IPromise<T> : IPromise
	{
		IPromise Then(Action<T> onFulfilled);
		IPromise Then(Action<T> onFulfilled, Action onRejected);
		IPromise Then(Action<T> onFulfilled, Action<Exception> onRejected);

		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled);
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected);
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Exception> onRejected);

		IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled);
		IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled, Action onRejected);
		IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled, Action<Exception> onRejected);

		new TaskAwaiter<T> GetAwaiter();
	}
}
