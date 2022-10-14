namespace ClientApplication.ServerConnection.Helpers;

public class JsonFolder
{
	JsonFolder(string path)
	{
		this.Path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + path;
	}

	public string Path { get; }

	public override string ToString(){ return Path; }
    
	public static readonly JsonFolder Json = new("Json\\");
	public static readonly JsonFolder TunnelMessages = new("Json\\TunnelMessages\\");
	public static readonly JsonFolder Terrain = new("Json\\TunnelMessages\\Terrain\\");
	public static readonly JsonFolder Route = new("Json\\TunnelMessages\\Route\\");
	public static readonly JsonFolder Panel = new("Json\\TunnelMessages\\Panel\\");
}