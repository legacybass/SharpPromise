using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharpPromise
{
	public static class PromiseExtensions
	{
		public static IPromise AsPromise(this Task task) => (IPromise)task;
#pragma warning disable CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
		public static Task AsTask(this IPromise promise) => (Task)promise;
#pragma warning restore CC0061 // Asynchronous method can be terminated with the 'Async' keyword.

		public static IPromise<T> AsPromise<T>(this Task<T> task) => (IPromise<T>)task;
#pragma warning disable CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
		public static Task<T> AsTask<T>(this IPromise<T> promise) => (Task<T>)promise;
#pragma warning restore CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
	}
}
