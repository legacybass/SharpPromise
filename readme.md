# SharpPromise

SharpPromise is a library intended to bring JavaScript's promise patterns to C#.

![CI](https://github.com/legacybass/SharpPromise/workflows/CI/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/SharpPromise.svg?maxAge=2592000)](https://www.nuget.org/packages/SharpPromise/)
[![.NET Standard](https://img.shields.io/badge/.net%20standard-2.0-blue.svg?logo=.net&logoColor=white&link=https://dotnet.github.io/&colorA=682079)](https://dotnet.github.io/)
[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/legacybass/SharpPromise/blob/master/LICENSE)

## Why Promises in C#?
The Task library gets very close, but still leaves a few things to be desired. For example, in JS you can use `somePromise.then(() => 42).then(result => console.log(result))` and have `42` logged to the console. In the Task library, the argument to `ContinueWith` is a task, and must be dealt with before you can get to the actual value. Promise's more clean interface makes it easy to get to the data you want, while still dealing with errors using the `catch` method.

## Usage
### Promise Class
There are two classes in the library, `Promise` and `Promise<T>`. Using both, you can get to as much of the Promise functionality as is possible in C#. Creating a promise is as easy as typing `var promise = new Promise((resolve) => resolve());`. This obviously won't do much, but the concept is simple. You do whatever you need to in the callback, and then resolve the promise when done.

The library allows you to pass in callbacks that take just the resolve action, resolve and reject (which takes no parameters), as well as a resolve with a reject that accepts an exception as the reject parameter.

The `Promise<T>` class has the same constructors, but also allows the resolve method to pass back a resolution paramter of type `T`. This is useful for things like REST calls where a return value is expected.

### Then Method
When using promises, you are typically waiting until the promise is finished to do some other work. The `Then` method allows you to do said work after the promise has finished. The API looks like this:

```C#
IPromise Promise#Then(Action onResolve);
IPromise Promise#Then(Action onResolve, Action onReject);
IPromise Promise#Then(Action onResolve, Action<Exception> onReject);

IPromise Promise#Then<T>(Action<T> onResolve);
IPromise Promise#Then<T>(Action<T> onResolve, Action onReject);
IPromise Promise#Then<T>(Action<T> onResolve, Action<Exception> onReject);

IPromise<T> Promise#Then<T>(Func<T> onResolve);
IPromise<T> Promise#Then<T>(Func<T> onResolve, Action onReject);
IPromoise<T> Promise#Then<T>(Func<T> onResolve, Action<Exception> onReject);

IPromise Promise#Then(Func<IPromise> onResolve);
IPromise Promise#Then(Func<IPromise> onResolve, Action onReject);
IPromise Promise#Then(Func<IPromise> onResolve, Action<Exception> onReject);

IPromise<T> Promise#Then<T>(Func<IPromise<T>> onResolve);
IPromise<T> Promise#Then<T>(Func<IPromise<T>> onResolve, Action onReject);
IPromise<T> Promise#Then<T>(Func<IPromise<T>> onResolve, Action<Exception> onReject);

IPromise Promise#Then(Func<Task> onResolve);
IPromise Promise#Then(Func<Task> onResolve, Action onReject);
IPromise Promise#Then(Func<Task> onResolve, Action<Exception> onReject);

IPromise<T> Promise#Then<T>(Func<Task<T>> onResolve);
IPromise<T> Promise#Then<T>(Func<Task<T>> onResolve, Action onReject);
IPromise<T> Promise#Then<T>(Func<Task<T>> onResolve, Action<Exception> onReject);

IPromise<TResult> Promise#Then<TResult>(Func<T, TResult> onFulfilled);
IPromise<TResult> Promise#Then<TResult>(Func<T, TResult> onFulfilled, Action onRejected);
IPromise<TResult> Promise#Then<TResult>(Func<T, TResult> onFulfilled, Action<Exception> onRejected);

IPromise Promise#Then<T>(Func<T, IPromise> onResolve);
IPromise Promise#Then<T>(Func<T, IPromise> onResolve, Action onRejected);
IPromise Promise#Then<T>(Func<T, IPromise> onResolve, Action<Exception> onRejected);

IPromise Promise#Then(Func<T, Task> onResolve);
IPromise Promise#Then(Func<T, Task> onResolve, Action onRejected);
IPromise Promise#Then(Func<T, Task> onResolve, Action<Exception> onRejected);

IPromise<TResult> Promise#Then<TResult>(Func<T, IPromise<TResult>> onFulfilled);
IPromise<TResult> Promise#Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action onRejected);
IPromise<TResult> Promise#Then<TResult>(Func<T, IPromise<TResult>> onFulfilled, Action<Exception> onRejected);

IPromise<TResult> Promise#Then<TResult>(Func<T, Task<TResult>> onResolve);
IPromise<TResult> Promise#Then<TResult>(Func<T, Task<TResult>> onResolve, Action onRejected);
IPromise<TResult> Promise#Then<TResult>(Func<T, Task<TResult>> onResolve, Action<Exception> onRejected);
```

This allows for all combinations of the Then functionality available to JS. When using `Then`, you can supply methods that don't care about the results of previous calls and don't return anything, all the way down to methods that take in the previous result and return a new result.

### Catch Method
If at any point in the chain a promise fails, the remaining `Then` calls will be skipped and not invoked. To handle the error, use `Catch` to take the error and deal with what has happened. Any exception in the `Then` chains will be given to the `Catch` method.

### Finally Method
The finally method will allow you to wait until the promise has been either resolved or rejected, and then perform some action. This is useful for scenarios where a cleanup needs to happen regardless of success or failure.

##### Examples
The `Then` method would be used after any work has been done.

```C#
new Promise(resolve => { var result = webService.DoWork(); resolve(result); })
.Then(() => /* Do something after the web call is made */);

new Promise(resolve => { var result = webService.DoWork(); resolve(result); })
.Then(response => /* Do something with the response */)
.Catch(ex => /* Handle the error */);

new Promise(resolve => { var result = webService.DoWork(); resolve(result); })
.Then(response => JsonConvert(response.Json))
.Then(data => /* Do something with the read data */)
.Catch(ex => /* Handle the error. An error thrown in any of the Then calls will get propagated here. */)
.Finally(() => /* Do something to clean up */);

new Promise(resolve => { var result = webService.DoWork(); resolve(result); })
.Then(response => { var result = webService.DoOtherWork(response.Data); return result; })
.Then(response => /* Do something with the subsequent call's data */);
```

### Promise.Resolve and Promise.Reject
These are helper methods on the Promise class that make it easy to quickly jump into a promise. Most of the time, you will do something like

```C#
Promise.Resolve()
.Then(() => webService.DoWork())
.Then(response => /* Deal with response */);
```

to avoid having to deal with the resolve/reject helper methods.

### Promise.All and Promise.Any
These helper methods will wait until either all or any of the promises have been resolved. This is useful if you want to create a lot of parallel tasks/promises that you want to run simultaneously, but then wait for them all to finish; or, if you have a number of items that could all give a result, and whichever is first is the one you want to use.

```C#
var promise1 = new Promise(resolve => ...);
var promise2 = new Promise(resolve => ...);

Promise.All(promise1, promise2)
.Then(results => /* Handle results */);
```

Due to C#'s strong typing, either all the promises must return the same type, or you must get the results out yourself by casting
them into the type you want. This is a limitation of strongly typed languages as compared to dynamic languages.

### Async/Await
SharpPromise is fully compatible with the async/await pattern, and can be used exactly the same way as using `Task`s. This means that the following is functionally the same:

```C#
await Task.Run(() => {
	// Perform some async action
});

await new Promise(resolve => {
	// Perform some async action
	resolve();
});
```
