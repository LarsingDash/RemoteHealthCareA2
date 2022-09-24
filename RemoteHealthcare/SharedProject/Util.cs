using System;
using System.Linq;
using System.Text;

namespace SharedProject;

public class Util
{
    private static readonly Random random = new Random();
    
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
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    /// <summary>
    /// It takes a byte array and returns a string representation of the byte array
    /// </summary>
    /// <param name="bytes">The byte array to convert to a string.</param>
    /// <returns>
    /// A string representation of the byte array.
    /// </returns>
    public static string ByteArrayToString(byte[] bytes)
    {
        var sb = new StringBuilder("[");
        foreach (var b in bytes)
        {
            sb.Append(b + ", ");
        }
        sb.Length-=2;
        sb.Append(']');
        return sb.ToString();
    }
}