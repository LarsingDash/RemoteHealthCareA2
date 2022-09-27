namespace ServerApplication;

public class EncryptionKeys
{
    private static string sharedDir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("RemoteHealthcare", StringComparison.Ordinal)) + "RemoteHealthcare\\SharedProject" + "\\";

    
    public static byte[] GetEncryptKey()
    {
        string path = sharedDir + "EncryptionKeys\\AesKey.txt";
        byte[] key = Array.ConvertAll(File.ReadAllText(path).Replace(" ", "").Split(Convert.ToChar(",")), s=> byte.Parse(s));
        return key;
    }
    public static byte[] GetEncryptIv()
    {
        string path = sharedDir + "EncryptionKeys\\AesIV.txt";
        byte[] key = Array.ConvertAll(File.ReadAllText(path).Replace(" ", "").Split(Convert.ToChar(",")), s=> byte.Parse(s));
        return key;
    }
}