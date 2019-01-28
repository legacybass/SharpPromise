using System;
using System.Collections.Generic;
using System.Text;

namespace Promise.Tests
{
	/// <summary>
	/// Used when an exception is needed in a promise test
	/// </summary>
	public class PromiseTestException : Exception
	{
		public PromiseTestException()
		{

		}

		public PromiseTestException(string message) : base(message)
		{

		}

		public PromiseTestException(string message, Exception innerException) : base(message, innerException)
		{

		}
	}
}
