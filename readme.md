# SharpPromise

SharpPromise is a library intended to bring JavaScript's promise patterns to C#.

[![Build status](https://ci.appveyor.com/api/projects/status/0hd8gh5fci88hmtr?svg=true)](https://ci.appveyor.com/project/legacybass/sharppromise)
[![NuGet](https://img.shields.io/nuget/v/SharpPromise.svg?maxAge=2592000)](https://www.nuget.org/packages/SharpPromise/)
[![.NET Core](https://img.shields.io/badge/.NET%20Core-2.0-brightgreen.svg)](https://dotnet.github.io/)

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
Promise#Then(Action onResolve);
Promise#Then(Action onResolve, Action onReject);
Promise#Then(Action onResolve, Action<Exception> onReject);
Promise#Then<T>(Action<T> onResolve);
Promise#Then<T>(Action<T> onResolve, Action onReject);
Promise#Then<T>(Action<T> onResolve, Action<Exception> onReject);
// The same as above, but with Funcs
```

This allows for all combinations of the Then functionality available to JS. When using `Then`, you can supply methods that don't care about the results of previous calls and don't return anything, all the way down to methods that take in the previous result and return a new result.

### Catch Method
If at any point in the chain a promise fails, the remaining `Then` calls will be skipped and not invoked. To handle the error, use `Catch` to take the error and deal with what has happened. Any exception in the `Then` chains will be given to the `Catch` method.

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