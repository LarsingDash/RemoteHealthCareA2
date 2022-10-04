using System.Text;

namespace ClientSide.Helpers;

public class Util
{
	private static readonly Random random = new Random();
    
	public static string RandomString()
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, 20)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
    
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