// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;

/// <summary>
/// This class is used to return a value or an error from a function.
/// </summary>
/// <typeparam name="T">Type of the return value</typeparam>
/// <remarks>
/// Use the implicit conversion operator to get the value returned by the
/// function.  The caller should check the Error property to see if there was
/// an error; if they do not, an exception will be raised.
///
/// Example:
/// <code>
/// ReturnStatus&lt;int&gt; status = GetValue(); // Get the value or an error
/// if (status.HasError) { ... }
/// int value = status; // Implicit conversion to the value
/// </code>
/// 
/// This approach is the preferred way to return error results, esp for errors
/// that the user should be aware of, and/or can correct.
/// There are still times to throw exeptions -- such as in a constructor,
/// or in procedures for well-known interfaces that doesnt provide any
/// other way to produce errors.
/// </remarks>
public class ReturnStatus<T>
{
    /// <summary>
    /// Implicit conversion to the value returned by the function.
    /// </summary>
    /// <param name="status">The return status</param>
    /// <returns>The value</returns>
    /// <remarks>
    /// Throws InvalidOperationException if there is an error.
    /// </remarks>
    public static implicit operator T(ReturnStatus<T> status)
    {
        // If there is an error, throw an exception
        // (The caller should check the Error property to see if there was an error)
        if (status.HasError)
            // The irony is that ReturnStatus exists to get away from the exceptions
            throw new InvalidOperationException(status.Error);

        // Return the value
        return status.value_!;
    }


    /// <summary>
    /// The value returned by the function
    /// </summary>
    private readonly T? value_;

    /// <summary>
    /// True if there is an error
    /// </summary>
    public bool HasError => Error != null;

    /// <summary>
    /// The error message if the function call failed
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// The value returned by the function
    /// </summary>
    public T Value => value_ ?? throw new InvalidOperationException("No value returned");

    /// <summary>
    /// Use this constructor to return a value
    /// </summary>
    /// <param name="value">The value returned by the function</param>
    public ReturnStatus(T value)
    {
        value_ = value;
    }

    /// <summary>
    /// Use this constructor to return an error
    /// </summary>
    internal ReturnStatus()
    {
    }

    /// <summary>
    /// Use this constructor to return an error
    /// </summary>
    /// <param name="error">The error message</param>
    internal static ReturnStatus<T> Failed(string error)
    {
        return new ReturnStatus<T>(){Error = error};
    }
}
