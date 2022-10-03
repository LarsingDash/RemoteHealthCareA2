using System.Text;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Shared.Encryption;
using System.Linq;

namespace Shared;

public class Util
{
    private static readonly Random Random = new();
    
    /// <summary>
    /// It generates a random string of 20 characters.
    /// </summary>
    /// <returns>
    /// A random string of 20 characters.
    /// </returns>
    public static string RandomString()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 20)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// It takes a byte array and returns a string representation of the byte array
    /// </summary>
    /// <param name="bytes">The byte array to convert to a string.</param>
    /// <param name="brackets">If array should start with [ and end with ]</param>
    /// <returns>
    /// A string representation of the byte array.
    /// </returns>
    public static string ByteArrayToString(byte[] bytes, bool brackets = true)
    {
        var sb = new StringBuilder();
        if (brackets)
            sb.Append("[");
        foreach (var b in bytes)
        {
            sb.Append(b + ", ");
        }
        sb.Length-=2;
        if(brackets)
            sb.Append(']');
        return sb.ToString();
    }
    
    public static string ArrayToString<T>(T[] values, bool brackets = true)
    {
        var sb = new StringBuilder();
        if (brackets)
            sb.Append("[");
        foreach (var b in values)
        {
            sb.Append(b.ToString() + ", ");
        }

        if (values.Length > 0)
        {
            sb.Length-=2;
        }
        if(brackets)
            sb.Append(']');
        return sb.ToString();
    }
}