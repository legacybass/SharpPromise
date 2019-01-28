using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SharpPromise;
using Shouldly;
using System.Threading.Tasks;

namespace Promise.Tests
{
	[TestClass]
	public class PromiseExtensionsTests
	{
		[TestMethod, TestCategory("Extensions")]
		public void AsTask_CastsToTaskFromPromise()
		{
			var promise = SharpPromise.Promise.Resolve();
			var task = promise.AsTask();

			task.ShouldNotBeNull();
		}

		[TestMethod, TestCategory("Extensions")]
		public void AsTask_CastsToGenericTaskFromGenericPromise()
		{
			var promise = Promise<int>.Resolve(42);
			var task = promise.AsTask();

			task.ShouldNotBeNull();
		}

		[TestMethod, TestCategory("Extensions")]
		public void AsPromise_CastsFromTaskToPromise()
		{
			var task = Task.CompletedTask;
			var promise = task.AsPromise();

			promise.ShouldNotBeNull();
		}

		[TestMethod, TestCategory("Extensions")]
		public void AsPromise_CastsFromGenericTaskToGenericPromise()
		{
			var task = Task.FromResult(42);
			var promise = task.AsPromise();

			promise.ShouldNotBeNull();
		}
	}
}
