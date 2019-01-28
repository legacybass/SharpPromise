using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharpPromise
{
	public static class PromiseExtensions
	{
		public static IPromise AsPromise(this Task task) => (Promise)task;
#pragma warning disable CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
		public static Task AsTask(this Promise promise) => promise;
#pragma warning restore CC0061 // Asynchronous method can be terminated with the 'Async' keyword.

		public static IPromise<T> AsPromise<T>(this Task<T> task) => (Promise<T>)task;
#pragma warning disable CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
		public static Task<T> AsTask<T>(this Promise<T> promise) => promise;
#pragma warning restore CC0061 // Asynchronous method can be terminated with the 'Async' keyword.

		public static Task<T> AsTask<T>(this IPromise<T> promise)
		{
			if (promise is Promise<T> p)
				return p;

			throw new InvalidCastException("Cannot cast from IPromise<T> to Task");
		}

		public static Task AsTask(this IPromise promise)
		{
			if (promise is Promise p)
				return p;

			throw new InvalidCastException("Cannot cast from IPromise to Task");
		}
	}
}
