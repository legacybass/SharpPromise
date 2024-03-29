﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using SharpPromise;

namespace Promise.Tests
{

#pragma warning disable CC0031 // Check for null before calling a delegate
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
			const int expected = 42;

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
			const int expected = 42;
			await new Promise<int>(resolve => { resolve(expected); }).Then((Func<int, IPromise>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ThenFiresAfterResolution()
		{
			Action<int> resolver = null;
			var wasCalled = false;
			const int expected = 42;

			IPromise<int> promise = new SharpPromise.Promise<int>(resolve => resolver = resolve);

			resolver.ShouldNotBeNull();

			var other = promise.Then(i =>
			{
				wasCalled = true;
				return 3;
			});

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
			const int expected = 42;

			IPromise<int> promise = new SharpPromise.Promise<int>(resolve => resolver = resolve);

			resolver.ShouldNotBeNull();

			resolver(expected);

			var other = promise.Then(i =>
			{
				wasCalled = true;
				return 3;
			});

			await promise;
			await other;

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Then:NoReturn")]
		public async Task ChainedThensExecuteInCorrectOrder()
		{
			var count = 1;
			const int expected = 42;
			IPromise<int> promise = new SharpPromise.Promise<int>(resolve =>
			{
				Task.Delay(200).Wait();
				resolve(expected);
			});

			await promise.Then(i => count++.ShouldBe(1))
			.Then(() => count++.ShouldBe(2));
		}

		[TestMethod, TestCategory("Then:Return"), ExpectedException(typeof(ArgumentNullException))]
		public void ThrowsOnNullResolvedCallbackWithReturn()
		{
			const int expected = 42;
			new SharpPromise.Promise<int>(resolve => { resolve(expected); }).Then((Func<bool>)null);
			Assert.Fail("Null resolve method did not throw exception");
		}

		[TestMethod, TestCategory("Then:Return")]
		public async Task ReturnedValueFromOneThenIsPassedToNextThen()
		{
			const int value = 42;
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

		[TestMethod, TestCategory("Then:Task")]
		public async Task ThenReturnsTaskWithResult()
		{
			var hasResolved = false;
			const int result = 42;

			// This form of Resolve is needed to get the generic form of IPromise back. The base interface breaks the test.
			var testPromise = Promise<int>.Resolve(0)
			.Then(i =>
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
		public async Task ThenWithParamReturnsTaskWithResult()
		{
			var hasResolved = false;
			const int result1 = 73, result2 = 42;

			var testPromise = Promise<int>.Resolve(0)
			.Then(i =>
			{
				return Task.FromResult(result1);
			})
			.Then(previous =>
			{
				hasResolved = true;
				return Task.FromResult((result1, result2));
			});

			await testPromise.Then(val =>
			{
				hasResolved.ShouldBeTrue();
				val.result1.ShouldBe(result1);
				val.result2.ShouldBe(result2);
			})
			.Catch(ex => 0.ShouldSatisfyAllConditions($"Something internal failed. {ex.Message}", () => throw ex));
		}

		[TestMethod, TestCategory("Then:Task")]
		public async Task ThenReturnsTaskWithoutResult()
		{
			bool hasResolved = false;

			var testPromise = Promise<int>.Resolve(0)
			.Then(i =>
			{
				return Task.CompletedTask.ContinueWith(_ =>
				{
					hasResolved = true;
				});
			});

			await testPromise.Then(() =>
			{
				hasResolved.ShouldBeTrue();
			})
			.Catch(ex => 0.ShouldSatisfyAllConditions($"Something internal failed. {ex.Message}", () => throw ex));
		}

		[TestMethod, TestCategory("Then:Task")]
		public async Task ThenWithParamReturnsTaskWithoutResult()
		{
			bool hasResolved = false;
			const int result1 = 73;

			var testPromise = Promise<int>.Resolve(0)
			.Then(_ =>
			{
				return Task.FromResult(result1);
			})
			.Then(previous =>
			{
				hasResolved = true;
			});

			await testPromise.Then(() =>
			{
				hasResolved.ShouldBeTrue();
			})
			.Catch(ex => 0.ShouldSatisfyAllConditions($"Something internal failed. {ex.Message}", () => throw ex));
		}

		[TestMethod, TestCategory("Catch")]
		public async Task CatchDealsWithExceptionFurtherUpTheChain()
		{
			var othersWereCalled = false;
			const int expected = 42;

			var exception = new TaskCanceledException();

			var promise = new Promise<int>(resolve => resolve(expected));

			await promise.Then(i => 42)
			.Then(_ => 73)
			.Then((Action<int>)(_ => throw exception))
			.Then(() => { othersWereCalled = true; })
			.Then(() => { othersWereCalled = true; })
			.Catch(ex => ex.ShouldBeAssignableTo<Exception>());

			othersWereCalled.ShouldBeFalse("Subsequent \"Then\" calls were made after failing.");
		}

		[TestMethod, TestCategory("Chaining")]
		public async Task IfReturnValueIsAPromiseReturnValueOfThatPromiseIsChained()
		{
			Action<int> returnedResolver = null;
			const int expected = 42;
			var returnedPromise = new Promise<int>(resolve => returnedResolver = resolve);
			var testedPromise = new SharpPromise.Promise(resolve => resolve());
			var resultPromise = testedPromise.Then(() => returnedPromise)
			.Then(result => result.ShouldBe(expected));

			returnedResolver(expected);

			await resultPromise;
		}

		[TestMethod, TestCategory("All")]
		public async Task AllMethodWaitsForAllPromisesToFullfill()
		{
			var promise2Resolve = false;
			var promise3Resolved = false;

			const int prom1Result = 42,
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

		[TestMethod, TestCategory("All")]
		public async Task AllMethodWaitsForAllTasksToFullfill()
		{
			var task2Resolve = false;
			var task3Resolve = false;

			var task2Time = DateTime.Now;
			var task3Time = task2Time;

			var result1 = 1;
			var result2 = 2;
			var result3 = 3;

			var t1 = Task.FromResult(result1);
			var t2 = Task.Delay(300).ContinueWith(_ =>
			{
				task2Resolve = true;
				task2Time = DateTime.Now;
				return result2;
			});
			var t3 = Task.Delay(100).ContinueWith(_ =>
			{
				task3Resolve = true;
				task3Time = DateTime.Now;
				return result3;
			});

			await SharpPromise.Promise<int>.All(t1, t2, t3)
			.Then(results =>
			{
				task2Resolve.ShouldBeTrue("Task 2 did not resolve.");
				task3Resolve.ShouldBeTrue("Task 3 did not resolve.");

				task2Time.ShouldBeGreaterThan(task3Time, "Task 2 resolved before task 3");

				results.ShouldNotBeNull("Results were null");
				results.Length.ShouldBe(3, "Not all tasks returned a value");
				results[0].ShouldBe(result1, "Task 1 result did not match");
				results[1].ShouldBe(result2, "Task 2 result did not match");
				results[2].ShouldBe(result3, "Task 3 result did not match");
			});
		}

		[TestMethod, TestCategory("Cast")]
		public void CastPromiseToTask()
		{
			const int result = 42;

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

		[TestMethod, TestCategory("Finally")]
		public async Task ValueIsPassedThroughFinally()
		{
			var wasCalled = false;
			var value = 42;
			await SharpPromise.Promise<int>.Resolve(value)
			.Finally(() => { wasCalled = true; })
			.Then(val => val.ShouldBe(value));

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Finally")]
		public async Task ExceptionIsPassedThroughFinally()
		{
			var wasCalled = false;
			var message = "It was passed as expected.";
			var value = new InternalTestFailureException(message);

			await SharpPromise.Promise<int>.Reject(value)
			.Finally(() => { wasCalled = true; })
			.Catch(val => val.Message.ShouldBe(message));

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Finally")]
		public async Task ExceptionIsPassedThroughFinallyAndHandledInThen()
		{
			var wasCalled = false;
			var wrongThenCalled = false;
			var value = new InternalTestFailureException("It was passed as expected.");

			await SharpPromise.Promise<int>.Reject(value)
			.Finally(() => { wasCalled = true; })
			.Then(() => { wrongThenCalled = true; }, val => val.ShouldBe(value));

			wasCalled.ShouldBeTrue();
			wrongThenCalled.ShouldBeFalse();
		}

		[TestMethod, TestCategory("Any")]
		public async Task AnyReturnsValueOfFirstPromiseFinished()
		{
			int result = 73, result2 = 37;

			IPromise<int> resolvingPromise = new SharpPromise.Promise<int>((resolve) =>
			{
				Task.Delay(500)
				.ContinueWith(_ => resolve(result));
			});

			IPromise<int> neverEndingPromise = new SharpPromise.Promise<int>((resolve) =>
			{
				Task.Delay(700)
				.ContinueWith(_ => resolve(result2));
			});

			IPromise<int> neverEndingStory = new SharpPromise.Promise<int>((Action<Action<int>, Action>)((resolve, reject) =>
			{
			}));

			bool didFinish = false;

			var promise = SharpPromise.Promise<int>.Any(resolvingPromise, neverEndingPromise, neverEndingStory)
			.Then(val =>
			{
				didFinish = true;
				val.ShouldBe(result);
			});

			var task = promise.AsTask();
			task.Wait(1000);

			didFinish.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Any")]
		public void AnyReturnsValueOfFirstTaskFinished()
		{
			int result = 73, result2 = 37;

			var resolvingPromise = Task.Delay(500).ContinueWith(t => result);
			var neverEndingPromise = Task.Delay(700).ContinueWith(t => result2);

			var exceptedTask = new TaskCompletionSource<int>();
			var neverEndingStory = exceptedTask.Task;

			bool didFinish = false;

			var promise = SharpPromise.Promise<int>.Any(resolvingPromise, neverEndingPromise, neverEndingStory)
			.Then(val =>
			{
				didFinish = true;
				val.ShouldBe(result);
			});

			var task = promise.AsTask();
			task.Wait(1000);

			didFinish.ShouldBeTrue();
		}
#pragma warning restore CC0031 // Check for null before calling a delegate
	}
}
