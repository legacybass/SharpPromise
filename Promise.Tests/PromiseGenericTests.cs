using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using SharpPromise;

namespace Promise.Tests
{
	[TestClass]
	public class PromiseGenericTests
	{
		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForAction()
		{
			var promise = new SharpPromise.Promise<int>((Action<Action<int>>)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForTask()
		{
			var promise = new SharpPromise.Promise<int>((Task<int>)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForActionAction()
		{
			var promise = new SharpPromise.Promise<int>((Action<Action<int>, Action>)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor"), ExpectedException(typeof(ArgumentNullException), "No exception thrown when a null callback was used")]
		public void ThrowsExceptionIfNoCallbackPassedForActionActionException()
		{
			var promise = new SharpPromise.Promise<int>((Action<Action<int>, Action<Exception>>)null);
			Assert.Fail("No exception was thrown on passing null to the constructor");
		}

		[TestMethod, TestCategory("Promise:Constructor")]
		public void SetsCorrectStateOnEmptyResolve()
		{
			Action<int> resolver = null;
			int expected = 42;

			IPromise<int> promise = new SharpPromise.Promise<int>((resolve) =>
			{
				resolver = resolve;
			});

			promise.State.ShouldBe(PromiseState.Pending);
			resolver.ShouldNotBeNull();

			resolver(expected);

			promise.State.ShouldBe(PromiseState.Fulfilled);
		}

		[TestMethod, TestCategory("Promise<T>:Constructor")]
		public void SetsCorrectStateOnParameterizedResolve()
		{
			Action<int> resolver = null;

			IPromise<int> promise = new Promise<int>((resolve) =>
			{
				resolver = resolve;
			});

			promise.State.ShouldBe(PromiseState.Pending);
			resolver.ShouldNotBeNull();

			resolver(42);

			promise.State.ShouldBe(PromiseState.Fulfilled);
		}

		[TestMethod, TestCategory("Promise:Constructor")]
		public void SetsCorrectStateOnEmptyReject()
		{
			Action rejecter = null;
			void callback(Action<int> resolve, Action reject)
			{
				rejecter = reject;
			}

			IPromise<int> promise = new SharpPromise.Promise<int>(callback);

			promise.State.ShouldBe(PromiseState.Pending);
			rejecter.ShouldNotBeNull();

			rejecter();

			promise.State.ShouldBe(PromiseState.Rejected);
		}

		[TestMethod, TestCategory("Promise:Constructor")]
		public void SetsCorrectStateOnParameterizedReject()
		{
			Action<Exception> rejecter = null;
			void callback(Action<int> resolve, Action<Exception> reject)
			{
				rejecter = reject;
			}

			IPromise<int> promise = new SharpPromise.Promise<int>(callback);

			promise.State.ShouldBe(PromiseState.Pending);
			rejecter.ShouldNotBeNull();

			rejecter(new Exception("Testing"));

			promise.State.ShouldBe(PromiseState.Rejected);
		}

		[TestMethod, TestCategory("Then:NoReturn"), ExpectedException(typeof(ArgumentNullException))]
		public async Task ThrowsOnNullResolvedCallback()
		{
			int expected = 42;
			await new Promise<int>(resolve => { resolve(expected); }).Then(null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:NoReturn"), ExpectedException(typeof(ArgumentNullException))]
		public async Task ThrowsOnNullRejectedCallback()
		{
			int expected = 42;
			await new SharpPromise.Promise<int>(resolve => { resolve(expected); }).Then(() => { }, (Action<Exception>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ThenFiresAfterResolution()
		{
			Action<int> resolver = null;
			var wasCalled = false;
			int expected = 42;

			IPromise<int> promise = new SharpPromise.Promise<int>(resolve => resolver = resolve);

			resolver.ShouldNotBeNull();

			var other = promise.Then(() => wasCalled = true);

			resolver(expected);

			await promise;
			await other;

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ThenFiresAfterResolutionOnAlreadyCompletedPromise()
		{
			Action<int> resolver = null;
			var wasCalled = false;
			int expected = 42;

			IPromise<int> promise = new SharpPromise.Promise<int>(resolve => resolver = resolve);

			resolver.ShouldNotBeNull();

			resolver(expected);

			var other = promise.Then(() => wasCalled = true);

			await promise;
			await other;

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ChainedThensExecuteInCorrectOrder()
		{
			int count = 1;
			int expected = 42;
			IPromise<int> promise = new SharpPromise.Promise<int>(resolve =>
			{
				Task.Delay(200).Wait();
				resolve(expected);
			});

			await promise.Then(() => count++.ShouldBe(1))
			.Then(() => count++.ShouldBe(2));
		}

		[TestMethod, TestCategory("Then:Return"), ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsOnNullResolvedCallbackWithReturn()
		{
			int expected = 42;
			new SharpPromise.Promise<int>(resolve => { resolve(expected); }).Then((Func<bool>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:Return"), ExpectedException(typeof(ArgumentNullException))]
		public async Task ThrowsOnNullRejectedCallbackWithReturn()
		{
			int expected = 42;
			await new SharpPromise.Promise<int>(resolve => { resolve(expected); }).Then(value => {  }, (Action<Exception>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:Return")]
		public async Task ReturnedValueFromOneThenIsPassedToNextThen()
		{
			var value = 42;
			IPromise<int> promise = new SharpPromise.Promise<int>(resolve =>
			{
				resolve(value);
			});

			await promise.Then(result => result * result)
			.Then(val => val.ShouldBe(value * value));
		}

		[TestMethod, TestCategory("Then:Return")]
		public async Task ExceptionFromOnePromiseIsGivenToHandlerInThen()
		{
			var exception = new TaskCanceledException();
			await new SharpPromise.Promise<int>((resolve, reject) =>
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
			int expected = 42;

			var exception = new TaskCanceledException();

			var promise = new SharpPromise.Promise<int>(resolve => resolve(expected));

			await promise.Then(() => { })
			.Then(() => { })
			.Then(() => throw exception)
			.Then(() => { othersWereCalled = true; })
			.Then(() => { othersWereCalled = true; })
			.Catch(ex => ex.ShouldBeAssignableTo<Exception>());

			othersWereCalled.ShouldBeFalse();
		}

		[TestMethod, TestCategory("Chaining")]
		public async Task IfReturnValueIsAPromiseReturnValueOfThatPromiseIsChained()
		{
			Action<int> returnedResolver = null;
			int expected = 42;
			var returnedPromise = new SharpPromise.Promise<int>(resolve => returnedResolver = resolve);
			var testedPromise = new SharpPromise.Promise(resolve => resolve());
			var resultPromise = testedPromise.Then(() => returnedPromise)
			.Then(result => result.ShouldBe(expected));

			returnedResolver(expected);

			await resultPromise;
		}

		[TestMethod, TestCategory("All")]
		public async Task AllMethodWaitsForAllPromisesToFullfill()
		{
			bool promise2Resolve = false,
				promise3Resolved = false;

			int prom1Result = 42,
				prom2Result = 73,
				prom3Result = 13;

			var promise1 = SharpPromise.Promise<int>.Resolve(prom1Result);
			var promise2 = new SharpPromise.Promise<int>((resolve) =>
			{
				Task.Delay(1000).ContinueWith(_ => {
					promise2Resolve = true;
					resolve(prom2Result);
				});
			});
			var promise3 = new SharpPromise.Promise<int>((resolve) =>
			{
				Task.Delay(2000).ContinueWith(_ =>
				{
					promise3Resolved = true;
					resolve(prom3Result);
				});
			});

			await SharpPromise.Promise<int>.All(promise1, promise2, promise3)
			.Then(results =>
			{
				promise1.State.ShouldBe(PromiseState.Fulfilled);
				promise2Resolve.ShouldBeTrue("Promise 2 did not resolve in time");
				promise3Resolved.ShouldBeTrue("Promise 3 did not resolve in time");

				results.Length.ShouldBe(3);
				results[0].ShouldBe(prom1Result);
				results[1].ShouldBe(prom2Result);
				results[2].ShouldBe(prom3Result);
			});
		}

		[TestMethod, TestCategory("Cast")]
		public void CastPromiseToTask()
		{
			int result = 42;

			Action<int> resolver = null;
			var promise = new Promise<int>(r => resolver = r);

			Task<int> test = promise;

			test.ShouldNotBeNull();
			test.IsCompleted.ShouldBeFalse();

			resolver(result);

			test.IsCompleted.ShouldBeTrue();
			test.Result.ShouldBe(result);
		}

		[TestMethod, TestCategory("Cast")]
		public void CastTaskToPromise()
		{
			var result = 42;
			var completionSource = new TaskCompletionSource<int>();
			var task = completionSource.Task;

			Promise<int> promise = task;

			promise.State.ShouldBe(PromiseState.Pending);

			completionSource.SetResult(result);

			promise.State.ShouldBe(PromiseState.Fulfilled);
			promise.Then(i => i.ShouldBe(result));
		}
	}
}
