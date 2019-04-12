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

		IPromise Finally(Action onFinal);

		/// <summary>
		/// Returns a new promise that will be resolved when the passed in action is finished.
		/// </summary>
		/// <param name="onFulfilled">Action to be invoked on resolution</param>
		/// <returns></returns>
		IPromise Then(Action onFulfilled);
		/// <summary>
		/// Returns a new promise that will be resolved when one of the passed in actions is finished.
		/// </summary>
		/// <param name="onFulfilled">Action to be invoked when this promise is resolved</param>
		/// <param name="onRejected">Action to be invoked when this promise is rejected</param>
		/// <returns></returns>
		IPromise Then(Action onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a new promise that will be resolved when one of the passed in actions is finished.
		/// </summary>
		/// <param name="onFulfilled">Action to be invoked when this promise is resolved</param>
		/// <param name="onRejected">Action to be invoked when this promise is rejected</param>
		/// <returns></returns>
		IPromise Then(Action onFufilled, Action<Exception> onRejected);

		/// <summary>
		/// Returns a new promise that will be resolved with the return value of the passed
		/// in <see cref="Func{TResult}" />.
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<T> onFulfilled);
		/// <summary>
		/// Returns a promise that will be resolved with the return value of the passed in
		/// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<T> onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a promise that will be resolved with the return value of the passed in
		/// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<T> onFulfilled, Action<Exception> onRejected);

		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="IPromise{T}"/> returned from the passed in <see cref="Func{TResult}"/>
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="IPromise{T}"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="IPromise{T}"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<IPromise<T>> onFulfilled, Action<Exception> onRejected);

		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="IPromise"/> returned from the passed in <see cref="Func{TResult}"/>
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
		/// <returns></returns>
		IPromise Then(Func<IPromise> onFulfilled);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="IPromise"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise Then(Func<IPromise> onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="IPromise"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise Then(Func<IPromise> onFulfilled, Action<Exception> onRejected);

		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task"/> returned from the passed in <see cref="Func{TResult}"/>
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
		/// <returns></returns>
		IPromise Then(Func<Task> onFulfilled);

		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise Then(Func<Task> onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise Then(Func<Task> onFulfilled, Action<Exception> onRejected);

		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<Task<T>> onFulfilled);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<T> Then<T>(Func<Task<T>> onFulfilled, Action<Exception> onRejected);

		TaskAwaiter GetAwaiter();
	}

	public interface IPromise<T> : IPromise
	{
		new IPromise<T> Finally(Action onFinal);

		/// <summary>
		/// Returns a new promise that will be resolved when the passed in action is finished.
		/// </summary>
		/// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked on resolution</param>
		/// <returns></returns>
		IPromise Then(Action<T> onFulfilled);
		/// <summary>
		/// Returns a new promise that will be resolved when one of the passed in actions is finished.
		/// </summary>
		/// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked when this promise is resolved</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected</param>
		/// <returns></returns>
		IPromise Then(Action<T> onFulfilled, Action onRejected);

		/// <summary>
		/// Returns a new promise that will be resolved when one of the passed in actions is finished.
		/// </summary>
		/// <param name="onFulfilled"><see cref="Action{T}"/> to be invoked when this promise is resolved</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected</param>
		/// <returns></returns>
		IPromise Then(Action<T> onFulfilled, Action<Exception> onRejected);


		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved</param>
		/// <returns></returns>
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled);
		/// <summary>
		/// Returns a promise that will be resolved with the return value of the passed in
		/// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a promise that will be resolved with the return value of the passed in
		/// <see cref="Func{TResult}"/>, or <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="T">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<TResult> Then<TResult>(Func<T, TResult> onFulfilled, Action<Exception> onRejected);

		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>
		/// </summary>
		/// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved</param>
		/// <returns></returns>
		IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="TResult">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled, Action onRejected);
		/// <summary>
		/// Returns a new promise that will be resolved with resolved value of the
		/// <see cref="Task{TResult}"/> returned from the passed in <see cref="Func{TResult}"/>, or
		/// <paramref name="onRejected"/> called when rejected.
		/// </summary>
		/// <typeparam name="TResult">Return type of the <see cref="Func{TResult}"/></typeparam>
		/// <param name="onFulfilled"><see cref="Func{T, TResult}"/> to be invoked when this promise is resolved.</param>
		/// <param name="onRejected"><see cref="Action{T}"/> to be invoked when this promise is rejected.</param>
		/// <returns></returns>
		IPromise<TResult> Then<TResult>(Func<T, Task<TResult>> onFulfilled, Action<Exception> onRejected);

		new TaskAwaiter<T> GetAwaiter();
	}
}
