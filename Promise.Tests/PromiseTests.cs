using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpPromise;
using System;
using System.Collections.Generic;
using Shouldly;
using System.Threading.Tasks;
using System.Threading;

namespace Promise.Tests
{
	[TestClass]
	public class PromiseTests
	{
		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForAction()
		{
			var promise = new SharpPromise.Promise((Action<Action>)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForTask()
		{
			var promise = new SharpPromise.Promise((Task)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForActionAction()
		{
			var promise = new SharpPromise.Promise((Action<Action, Action>)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForActionActionException()
		{
			var promise = new SharpPromise.Promise((Action<Action, Action<Exception>>)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor")]
		public void SetsCorrectStateOnEmptyResolve()
		{
			Action resolver = null;

			IPromise promise = new SharpPromise.Promise((resolve) =>
			{
				resolver = resolve;
			});

			promise.State.ShouldBe(PromiseState.Pending);
			resolver.ShouldNotBeNull();

			resolver();

			promise.State.ShouldBe(PromiseState.Fulfilled);
		}

		[TestMethod, TestCategory("Promise:Constructor")]
		public void SetsCorrectStateOnEmptyReject()
		{
			Action rejecter = null;
			void callback(Action resolve, Action reject)
			{
				rejecter = reject;
			}

			IPromise promise = new SharpPromise.Promise(callback);

			promise.State.ShouldBe(PromiseState.Pending);
			rejecter.ShouldNotBeNull();

			rejecter();

			promise.State.ShouldBe(PromiseState.Rejected);
		}

		[TestMethod, TestCategory("Promise:Constructor")]
		public void SetsCorrectStateOnParameterizedReject()
		{
			Action<Exception> rejecter = null;
			void callback(Action resolve, Action<Exception> reject)
			{
				rejecter = reject;
			}

			IPromise promise = new SharpPromise.Promise(callback);

			promise.State.ShouldBe(PromiseState.Pending);
			rejecter.ShouldNotBeNull();

			rejecter(new Exception("Testing"));

			promise.State.ShouldBe(PromiseState.Rejected);
		}

		[TestMethod, TestCategory("Then:NoReturn"), ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsOnNullResolvedCallback()
		{
			new SharpPromise.Promise(resolve => { resolve(); }).Then(null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:NoReturn"), ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsOnNullRejectedCallback()
		{
			new SharpPromise.Promise(resolve => { resolve(); }).Then(() => { }, (Action<Exception>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ThenFiresAfterResolution()
		{
			Action resolver = null;
			var wasCalled = false;

			IPromise promise = new SharpPromise.Promise(resolve => resolver = resolve);

			resolver.ShouldNotBeNull();

			var other = promise.Then(() => wasCalled = true);

			resolver();

			await promise;
			await other;

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ThenFiresAfterResolutionOnAlreadyCompletedPromise()
		{
			Action resolver = null;
			var wasCalled = false;

			IPromise promise = new SharpPromise.Promise(resolve => resolver = resolve);

			resolver.ShouldNotBeNull();

			resolver();

			var other = promise.Then(() => wasCalled = true);

			await promise;
			await other;

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ChainedThensExecuteInCorrectOrder()
		{
			int count = 1;
			IPromise promise = new SharpPromise.Promise(resolve =>
			{
				Thread.Sleep(200);
				resolve();
			});

			await promise.Then(() => count++.ShouldBe(1))
			.Then(() => count++.ShouldBe(2));
		}

		[TestMethod, TestCategory("Then:Return"), ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsOnNullResolvedCallbackWithReturn()
		{
			new SharpPromise.Promise(resolve => { resolve(); }).Then((Func<bool>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:Return"), ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsOnNullRejectedCallbackWithReturn()
		{
			new SharpPromise.Promise(resolve => { resolve(); }).Then(() => { return true; }, (Action<Exception>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:Return")]
		public async Task ReturnedValueFromOneThenIsPassedToNextThen()
		{
			var value = 42;
			IPromise promise = new SharpPromise.Promise(resolve =>
			{
				resolve();
			});

			await promise.Then(() => value)
			.Then(val => val.ShouldBe(value));
		}

		[TestMethod, TestCategory("Then:Return")]
		public async Task ExceptionFromOnePromiseIsGivenToHandlerInThen()
		{
			var exception = new TaskCanceledException();
			await new SharpPromise.Promise((resolve, reject) =>
			{
				reject(exception);
			})
			.Then(() => { }, ex =>
			{
				ex.ShouldBe(exception);
			});
		}

		[TestMethod, TestCategory("Catch")]
		public async Task CatchDealsWithExceptionFurtherUpTheChain()
		{
			bool othersWereCalled = false;

			var exception = new TaskCanceledException();

			var promise = new SharpPromise.Promise(resolve => resolve());

			await promise.Then(() => { })
			.Then(() => { })
			.Then(() => throw exception)
			.Then(() => { othersWereCalled = true; })
			.Then(() => { othersWereCalled = true; })
			.Catch(ex => ex.ShouldBeAssignableTo<Exception>());

			othersWereCalled.ShouldBeFalse();
		}

		[TestMethod, TestCategory("Cast")]
		public void CastPromiseToTask()
		{
			Action resolver = null;
			var promise = new SharpPromise.Promise(r => resolver = r);

			Task test = promise;

			test.ShouldNotBeNull();
			test.IsCompleted.ShouldBeFalse();

			resolver();

			test.IsCompleted.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Cast")]
		public void CastTaskToPromise()
		{
			var completionSource = new TaskCompletionSource<int>();
			var task = completionSource.Task;

			SharpPromise.Promise promise = task;

			promise.State.ShouldBe(PromiseState.Pending);

			completionSource.SetResult(0);

			promise.State.ShouldBe(PromiseState.Fulfilled);
		}
	}
}
