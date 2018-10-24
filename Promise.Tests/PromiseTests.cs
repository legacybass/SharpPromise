using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpPromise;
using System;
using System.Collections.Generic;
using Shouldly;
using System.Threading.Tasks;
using System.Threading;

namespace Promise.Tests
{
#pragma warning disable CC0031 // Check for null before calling a delegate
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
			new SharpPromise.Promise(resolve => { resolve(); }).Then((Action)null);
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

		[TestMethod, TestCategory("Then:Promise")]
		public async Task ThenReturnsPromiseWithResult()
		{
			var hasResolved = false;

			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				return new SharpPromise.Promise((resolve) =>
				{
					resolve();
				})
				.Then(() =>
				{
					hasResolved = true;
					return hasResolved;
				});
			});

			await testPromise.Then(() =>
			{
				hasResolved.ShouldBeTrue("Returned promise did not resolve before reaching this point");
			})
			.Catch(ex => 0.ShouldSatisfyAllConditions($"Something internal failed. {ex.Message}", () => throw ex));
		}

		[TestMethod, TestCategory("Then:Promise")]
		public async Task ThenReturnsPromiseWithException()
		{
			bool hasResolved = false;

			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				return new SharpPromise.Promise((resolve, reject) =>
				{
					reject(new Exception());
				})
				.Then(() =>
				{
					hasResolved = true;
				});
			});

			await testPromise.Then(() =>
			{
				Assert.Fail();
			})
			.Catch(ex =>
			{
				hasResolved.ShouldBeFalse();
			});
		}

		[TestMethod, TestCategory("Then:Task")]
		public async Task ThenReturnsTask()
		{
			var hasResolved = false;
			var chainsCalled = false;

			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				var firstTask = Task.Delay(50);
				var secondTask = Task.Delay(10);
				var thirdTask = Task.Delay(100);

				return Task.WhenAll(firstTask, secondTask, thirdTask).ContinueWith(tasks =>
				{
					hasResolved = true;
				});
			})
			.Then(() =>
			{

			})
			.Then(() =>
			{
				chainsCalled = true;
			});

			await testPromise.Then(() =>
			{
				hasResolved.ShouldBeTrue("Promise did not wait for returned task to complete.");
				chainsCalled.ShouldBeTrue("Chains were not called.");
			})
			.Catch(ex => 0.ShouldSatisfyAllConditions($"Something internal failed. {ex.Message}", () => throw ex));
		}

		[TestMethod]
		public void MultipleThensReturnMultipleTasks()
		{

		}

		[TestMethod, TestCategory("Then:Task")]
		public async Task ThenReturnsTaskWithResult()
		{
			var hasResolved = false;
			const int result = 42;

			// This form of Resolve is needed to get the generic form of IPromise back. The base interface breaks the test.
			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				return Task.CompletedTask.ContinueWith(_ =>
				{
					hasResolved = true;
					return result;
				});
			});

			await testPromise.Then(val =>
			{
				hasResolved.ShouldBeTrue();
				val.ShouldBe(result);
			})
			.Catch(ex => 0.ShouldSatisfyAllConditions($"Something internal failed. {ex.Message}", () => throw ex));
		}

		[TestMethod, TestCategory("Then:Task")]
		public void ThenReturnsTaskThatFails()
		{
			var source = new TaskCompletionSource<int>();
			var ex = new InternalTestFailureException("Task failed, but that's what we wanted.");

			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				source.SetException(ex);
				return source.Task;
			});

			testPromise.Then(() => 0.ShouldSatisfyAllConditions($"Promise resolved, rather than rejecting with failed task.", () => throw ex),
				resultException =>
				{
					resultException.ShouldBe(ex);
				});
		}

		[TestMethod, TestCategory("Catch")]
		public async Task CatchDealsWithExceptionFurtherUpTheChain()
		{
			var othersWereCalled = false;

			var exception = new TaskCanceledException();

			var promise = new SharpPromise.Promise(resolve => resolve());

			await promise.Then(() => { })
			.Then(() => { })
			.Then((Action)(() => throw exception))
			.Then(() => { othersWereCalled = true; })
			.Then(() => { othersWereCalled = true; })
			.Catch(ex => ex.ShouldBeAssignableTo<TaskCanceledException>());

			othersWereCalled.ShouldBeFalse("Then calls after exception should not be called");
		}

		[TestMethod, TestCategory("All")]
		public async Task AllMethodWaitsForAllPromisesToFullfill()
		{
			bool promise2Resolve = false,
				promise3Resolved = false;

			var promise1 = SharpPromise.Promise.Resolve();
			var promise2 = new SharpPromise.Promise((resolve) =>
			{
				Task.Delay(1000).ContinueWith(_ => {
					promise2Resolve = true;
					resolve();
				});
			});
			var promise3 = new SharpPromise.Promise((resolve) =>
			{
				Task.Delay(2000).ContinueWith(_ =>
				{
					promise3Resolved = true;
					resolve();
				});
			});

			await SharpPromise.Promise.All(promise1, promise2, promise3)
			.Then(() =>
			{
				promise2Resolve.ShouldBeTrue("Promise 2 did not resolve in time");
				promise3Resolved.ShouldBeTrue("Promise 3 did not resolve in time");
			});
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
#pragma warning restore CC0031 // Check for null before calling a delegate
}
