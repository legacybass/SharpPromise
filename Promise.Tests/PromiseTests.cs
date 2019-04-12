using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpPromise;
using System;
using System.Collections.Generic;
using Shouldly;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

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
			var message = "Promise failed, but that's what we expected";
			var ex = new InternalTestFailureException(message);
			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				return SharpPromise.Promise.Reject(ex);
			});

			await testPromise.Then(() =>
			{
				Assert.Fail("Promise resolved, but should not have.");
			})
			.Catch(e =>
			{
				e.Message.ShouldBe(message);
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
		public async Task ThenReturnsTaskThatFails()
		{
			var message = "Task failed, but that's what we wanted.";
			var ex = new InternalTestFailureException(message);

			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				return Task.FromException(ex);
			});

			await testPromise.Then(() =>
			{
				Assert.Fail("Promise resolved, but should not have.");
			})
			.Catch(e =>
			{
				e.Message.ShouldBe(message);
			});
		}

		[TestMethod, TestCategory("Then:Rejected")]
		public async Task ExceptionGetsHandledInThenCallbackWithTask()
		{
			var message = "Task failed, but that's what we wanted.";
			var ex = new InternalTestFailureException(message);

			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() =>
			{
				return Task.FromException(ex);
			});

			await testPromise.Then(() =>
			{
				Assert.Fail("Promise resolved, but should not have.");
			},
			e =>
			{
				e.Message.ShouldBe(message, "Exception messages did not match.");
			});
		}

		[TestMethod, TestCategory("Then:Rejected")]
		public async Task ExceptionGetsHandledInThenCallbackWithPromise()
		{
			var message = "Task failed, but that's what we wanted.";
			var ex = new InternalTestFailureException(message);

			var testPromise = SharpPromise.Promise.Resolve()
			.Then(() => SharpPromise.Promise.Reject(ex));

			await testPromise.Then(() =>
			{
				Assert.Fail("Promise resolved, but should not have.");
			},
			e =>
			{
				e.Message.ShouldBe(message, "Exception messages did not match.");
			});
		}

		[TestMethod, TestCategory("Then:Rejected")]
		public async Task ExceptionGetsHandledInThenCallbackFromRejectMethod()
		{
			var message = "Task failed, but that's what we wanted.";
			var ex = new InternalTestFailureException(message);

			var testPromise = SharpPromise.Promise.Reject(ex);

			await testPromise.Then(() =>
			{
				Assert.Fail("Promise resolved, but should not have.");
			},
			e =>
			{
				e.Message.ShouldBe(message, "Exception messages did not match.");
			});
		}

		[TestMethod, TestCategory("Then:Rejected")]
		public async Task ExceptionGetsHandledInThenCallbackFromRejectedTask()
		{
			var message = "Task failed, but that's what we wanted.";
			var ex = new InternalTestFailureException(message);

			var testTask = new SharpPromise.Promise(Task.FromException(ex));

			await testTask.Then(() =>
			{
				Assert.Fail("Promise resolved, but should not have.");
			},
			e =>
			{
				e.Message.ShouldBe(message, "Exception messages did not match.");
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

		[TestMethod, TestCategory("Catch")]
		public async Task CatchNotCalledWhenHandledInThenCallback()
		{
			var othersWereCalled1 = false;
			var handlerCalled1 = false;
			var catchCalled1 = false;

			var othersWereCalled2 = false;
			var catchCalled2 = false;

			var exception = new InternalTestFailureException();

			var promise = new SharpPromise.Promise(resolve => resolve());

			var prom = promise.Then(() => { })
			.Then(() => { })
			.Then((Action)(() => throw exception));

			var run1 = prom.Then(() => { }, ex => { handlerCalled1 = true; ex.ShouldBe(exception, "Exceptions were not the same."); })
			.Then(() => { othersWereCalled1 = true; })
			.Catch(ex => { catchCalled1 = true; });

			var run2 = prom.Then(() => { othersWereCalled2 = true; })
			.Catch(ex => { catchCalled2 = true; });

			await SharpPromise.Promise.All(run1, run2);

			othersWereCalled1.ShouldBeTrue("Subsequent \"Then\" calls were not executed after exception was handled.");
			handlerCalled1.ShouldBeTrue("Handler was not called after exception.");
			catchCalled1.ShouldBeFalse("Catch was still called after exception was handled.");

			othersWereCalled2.ShouldBeFalse();
			catchCalled2.ShouldBeTrue();
		}

		[TestMethod, TestCategory("All")]
		public async Task AllMethodWaitsForAllPromisesToFullfill()
		{
			var promise2Resolve = false;
			var promise3Resolved = false;

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

		[TestMethod, TestCategory("All")]
		public async Task AllMethodWaitsForAllTasksToFullfill()
		{
			var task2Resolve = false;
			var task3Resolve = false;

			var task2Time = DateTime.Now;
			var task3Time = task2Time;

			var t1 = Task.FromResult(1);
			var t2 = Task.Delay(300).ContinueWith(_ =>
			{
				task2Resolve = true;
				task2Time = DateTime.Now;
			});
			var t3 = Task.Delay(100).ContinueWith(_ =>
			{
				task3Resolve = true;
				task3Time = DateTime.Now;
			});

			await SharpPromise.Promise.All(t1, t2, t3)
			.Then(() =>
			{
				task2Resolve.ShouldBeTrue("Task 2 did not resolve.");
				task3Resolve.ShouldBeTrue("Task 3 did not resolve.");

				task2Time.ShouldBeGreaterThan(task3Time, "Task 2 resolved before task 3");
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

		[TestMethod, TestCategory("Race")]
		public async Task RaceReturnsOnceAndOnFirstResolve()
		{
			const int delay1 = 300, delay2 = 100, delay3 = 5000;
			var task1Finished = false;
			var task2Finished = false;
			var task3Finished = false;

			var expectedException = new PromiseTestException("Perfectly acceptable exception that should exist for testing.");

			var promise1 = SharpPromise.Promise.Resolve()
			.Then(async () =>
			{
				await Task.Delay(delay1);
				task1Finished = true;
			});

			var promise2 = SharpPromise.Promise.Resolve()
			.Then(async () =>
			{
				await Task.Delay(delay2);
				task2Finished = true;
			});

			var promise3 = SharpPromise.Promise.Resolve()
			.Then(async () =>
			{
				await Task.Delay(delay3);
				task3Finished = true;
				throw expectedException;
			});

			var item = SharpPromise.Promise.Race(promise1, promise2, promise3);
			await item.Then(() =>
			{
				task2Finished.ShouldBeTrue("Task 2 did not finish in time.");
				task1Finished.ShouldBeFalse("Task 1 finished before 2");
			},
			ex =>
			{
				0.ShouldBe(1, "Test 1 resulted in the thrown exception, which should never be reached.");
			});

			var task4Finished = false;
			var task5Finished = false;
			var task6Finished = false;

			var promise4 = SharpPromise.Promise.Resolve()
			.Then(async () =>
			{
				await Task.Delay(delay1);
				task4Finished = true;
			});

			var promise5 = SharpPromise.Promise.Resolve()
			.Then(async () =>
			{
				await Task.Delay(delay3);
				task5Finished = true;
			});

			var promise6 = SharpPromise.Promise.Resolve()
			.Then(async () =>
			{
				await Task.Delay(delay2);
				task6Finished = true;
				throw expectedException;
			});

			var item2 = SharpPromise.Promise.Race(promise4, promise5, promise6);
			await item2.Then(() =>
			{
				0.ShouldBe(1, "Test 2 resulted in an accepted state, which should never be reached.");
			},
			ex =>
			{
				task6Finished.ShouldBeTrue("Task 6 did not finish in time.");
			});
		}

		//[TestMethod, TestCategory("Race")]
		//public async Task RaceReturnsValueFromFirstResolve()
		//{
		//	const int delay1 = 500, delay2 = 100, delay3 = 3000;
		//	const int result1 = 1, result2 = 2, result3 = 3;
		//	var task1Finished = false;
		//	var task2Finished = false;
		//	var task3Finished = false;

		//	var promise1 = SharpPromise.Promise.Resolve()
		//	.Then<object>(async () =>
		//	{
		//		await Task.Delay(delay1);
		//		task1Finished = true;
		//		return result1;
		//	});

		//	var promise2 = SharpPromise.Promise.Resolve()
		//	.Then<object>(async () =>
		//	{
		//		await Task.Delay(delay2);
		//		task2Finished = true;
		//		return result2;
		//	});

		//	var promise3 = SharpPromise.Promise.Resolve()
		//	.Then<object>(async () =>
		//	{
		//		await Task.Delay(delay3);
		//		task3Finished = true;
		//		return result3;
		//	});

		//	var item = SharpPromise.Promise.Race(promise1, promise2, promise3);
		//	await item.Then(result =>
		//	{
		//		task2Finished.ShouldBeTrue("Task 2 did not finish in time.");
		//		task1Finished.ShouldBeFalse("Task 1 finished, but should not have.");
		//		task3Finished.ShouldBeFalse("Task 3 finished, but should not have.");

		//		result.ShouldBeAssignableTo<int>();
		//		((int)result).ShouldBe(result2);
		//	});
		//}

		[TestMethod, TestCategory("Race")]
		public async Task RaceReturnsOnceAndForFirstTask()
		{
			const int delay1 = 500, delay2 = 100, delay3 = 5000;
			var task1Finished = false;
			var task2Finished = false;
			var task3Finished = false;
			var expectedException = new PromiseTestException("Expected to happen");

			var task1 = Task.Run(async () =>
			{
				await Task.Delay(delay1);
				task1Finished = true;
			});

			var task2 = Task.Run(async () =>
			{
				await Task.Delay(delay2);
				task2Finished = true;
			});

			var task3 = Task.Run(async () =>
			{
				await Task.Delay(delay3);
				task3Finished = true;
				throw expectedException;
			});

			var item = SharpPromise.Promise.Race(task1, task2, task3);
			await item.Then(() =>
			{
				task2Finished.ShouldBeTrue("Task 2 did not finish in time.");
			},
			ex =>
			{
				0.ShouldBe(1, "Test 1 resulted in an exception, which should not happen");
			});


			var task4Finished = false;
			var task5Finished = false;
			var task6Finished = false;

			var task4 = Task.Run(async () =>
			{
				await Task.Delay(delay1);
				task4Finished = true;
			});

			var task5 = Task.Run(async () =>
			{
				await Task.Delay(delay3);
				task5Finished = true;
			});

			var task6 = Task.Run(async () =>
			{
				await Task.Delay(delay2);
				task6Finished = true;
				throw expectedException;
			});

			var item2 = SharpPromise.Promise.Race(task4, task5, task6);
			await item2.Then(() =>
			{
				0.ShouldBe(1, "Test 2 resulted in a success, which should not have happened.");
			},
			ex =>
			{
				task6Finished.ShouldBe(true);
			});
		}

		[TestMethod, TestCategory("Race")]
		public async Task RaceReturnsValueFromFirstTask()
		{
			const int delay1 = 500, delay2 = 100, delay3 = 3000;
			const int result1 = 1, result2 = 2, result3 = 3;
			var task1Finished = false;
			var task2Finished = false;
			var task3Finished = false;

			var task1 = Task.Run<object>(async () =>
			{
				await Task.Delay(delay1);
				task1Finished = true;
				return result1;
			});

			var task2 = Task.Run<object>(async () =>
			{
				await Task.Delay(delay2);
				task2Finished = true;
				return result2;
			});

			var task3 = Task.Run<object>(async () =>
			{
				await Task.Delay(delay3);
				task3Finished = true;
				return result3;
			});

			var item = SharpPromise.Promise.Race(task1, task2, task3);
			await item.Then(result =>
			{
				task2Finished.ShouldBeTrue("Task 2 did not finish in time.");
				task1Finished.ShouldBeFalse("Task 1 finished, but should not have.");
				task3Finished.ShouldBeFalse("Task 3 finished, but should not have.");

				result.ShouldBeAssignableTo<int>();
				((int)result).ShouldBe(result2);
			});
		}

		[TestMethod, TestCategory("Race")]
		public async Task RaceReturnsValueFromFirstPromise()
		{
			const int delay1 = 500, delay2 = 100, delay3 = 3000;
			const int result1 = 1, result2 = 2, result3 = 3;
			var task1Finished = false;
			var task2Finished = false;
			var task3Finished = false;

			var task1 = SharpPromise.Promise.Resolve()
			.Then<object>(async () =>
			{
				await Task.Delay(delay1);
				task1Finished = true;
				return result1;
			});

			var task2 = SharpPromise.Promise.Resolve()
			.Then<object>(async () =>
			{
				await Task.Delay(delay2);
				task2Finished = true;
				return result2;
			});

			var task3 = SharpPromise.Promise.Resolve()
			.Then<object>(async () =>
			{
				await Task.Delay(delay3);
				task3Finished = true;
				return result3;
			});

			var item = SharpPromise.Promise.Race(task1, task2, task3);
			await item.Then(result =>
			{
				task2Finished.ShouldBeTrue("Task 2 did not finish in time.");
				task1Finished.ShouldBeFalse("Task 1 finished, but should not have.");
				task3Finished.ShouldBeFalse("Task 3 finished, but should not have.");

				result.ShouldBeAssignableTo<int>();
				((int)result).ShouldBe(result2);
			});
		}

		[TestMethod, TestCategory("Any")]
		public void AnyReturnsIfAnyPromiseFinishes()
		{
			Assert.Fail();
		}

		[TestMethod, TestCategory("Any")]
		public void AnyFailsIfNoPromisesFinish()
		{
			Assert.Fail();
		}

		[TestMethod, TestCategory("Any")]
		public void AnyReturnsIfAnyTaskFinishes()
		{
			Assert.Fail();
		}

		[TestMethod, TestCategory("Any")]
		public void AnyFailsIfNoTasksFinish()
		{
			Assert.Fail();
		}

		[TestMethod, TestCategory("Any")]
		public void AnyReturnsValueOfFirstPromiseFinished()
		{
			Assert.Fail();
		}

		[TestMethod, TestCategory("Any")]
		public void AnyReturnsValueOfFirstTaskFinished()
		{
			Assert.Fail();
		}

		[TestMethod, TestCategory("Finally")]
		public async Task FinallyIsCalledAfterSuccess()
		{
			var wasCalled = false;
			await SharpPromise.Promise.Resolve()
			.Finally(() => { wasCalled = true; });

			wasCalled.ShouldBeTrue();
		}

		[TestMethod, TestCategory("Finally")]
		public async Task FinallyIsCalledAfterFailure()
		{
			var wasCalled = false;
			await SharpPromise.Promise.Reject(new Exception())
			.Finally(() => { wasCalled = true; });

			wasCalled.ShouldBeTrue();
		}
	}
#pragma warning restore CC0031 // Check for null before calling a delegate
}
