using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

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
		IPromise Then(Action onFullfilled);
		IPromise Then(Action onFullfilled, Action onRejected);
		IPromise Then(Action onFulfilled, Action<Exception> onRejected);
		IPromise<T> Then<T>(Func<T> onFullfilled);
		IPromise<T> Then<T>(Func<T> onFullfilled, Action onRejected);
		IPromise<T> Then<T>(Func<T> onFulfilled, Action<Exception> onRejected);
		TaskAwaiter GetAwaiter();
	}

	public interface IPromise<T> : IPromise
	{
		IPromise Then(Action<T> onFulfilled);
		IPromise Then(Action<T> onFulfilled, Action<Exception> onRejected);
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFullfilled);
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected);
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Exception> onRejected);
	}
}
