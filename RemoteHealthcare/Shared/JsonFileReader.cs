

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Shared.Encryption;
using System.IO;
using Shared.Log;

namespace Shared {


    public static class JsonFileReader {
        private static string pathDir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + "\\Json\\";
        
        /// <summary>
        /// It takes a file name, a dictionary of values, and a path to a folder, and returns a JObject with the values
        /// replaced
        /// </summary>
        /// <param name="fileName">The name of the file you want to get the object from.</param>
        /// <param name="values">A dictionary of values to replace in the JSON file.</param>
        /// <param name="path">The path to the folder where the files are stored.</param>
        /// <returns>
        /// A JObject
        /// </returns>
        public static JObject GetObject(string fileName, Dictionary<string, string> values, string path)
        {
            fileName = CheckFileName(fileName);
            string ob = JObject.Parse(File.ReadAllText(path + fileName)).ToString();
            foreach (string key in values.Keys)
            {
                ob = ob.Replace(key, values[key]);
            }
            return JObject.Parse(ob);
        }

        /// <summary>
        /// Calls the same method with the default directory
        /// </summary>
        /// <param name="fileName">The name of the file you want to load.</param>
        /// <param name="values">A dictionary of key/value pairs. The key is the name of the variable in the JSON file, and the
        /// value is the value you want to replace it with.</param>
        /// <returns>
        /// A JObject
        /// </returns>
        public static JObject GetObject(string fileName, Dictionary<string, string> values)
        {
            return GetObject(fileName, values, pathDir);
        }

        /// <summary>
        /// It takes a file name and a dictionary of values, and returns a string representation of the file with the values
        /// replaced
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <param name="values">A dictionary of key/value pairs. The key is the name of the variable in the template, and the
        /// value is the value to replace it with.</param>
        /// <returns>
        /// A string
        /// </returns>
        public static string GetObjectAsString(string fileName, Dictionary<string, string> values)
        {
            return GetObject(fileName, values).ToString();
        }
        
        /// <summary>
        /// It takes a file name, a dictionary of values, and a path, and returns the decrypted text of the file
        /// </summary>
        /// <param name="fileName">The name of the file to be encrypted.</param>
        /// <param name="values">A dictionary of strings that will be replaced in the file.</param>
        /// <param name="path">The path to the folder where the encrypted files are stored.</param>
        /// <returns>
        /// The decrypted text of the file.
        /// </returns>
        public static string GetEncryptedText(string fileName, Dictionary<string, string> values, string path)
        {
            CheckFileName(fileName);
            var key = EncryptionKeys.GetEncryptKey();
            var iv = EncryptionKeys.GetEncryptIv();

            byte[] encrypted = Array.ConvertAll(File.ReadAllText(path + fileName).Replace(" ", "").Split(Convert.ToChar(",")), s => byte.Parse(s));
            string decrypted = AesHelper.DecryptMessage(encrypted, key, iv)!;
            foreach (string stringKey in values.Keys)
            {
                decrypted = decrypted.Replace(stringKey, values[stringKey]);
            }

            return decrypted;
        }

        /// <summary>
        /// It takes a file name, a dictionary of values, and a path, and returns a JObject of the encrypted text.
        /// </summary>
        /// <param name="fileName">The name of the file you want to encrypt.</param>
        /// <param name="values">A dictionary of key/value pairs that will be used to replace the placeholders in the
        /// template.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// A JObject
        /// </returns>
        public static JObject GetEncryptedObject(string fileName, Dictionary<string, string> values, string path)
        {
            return JObject.Parse(GetEncryptedText(fileName, values, path));
        }
        
        /// <summary>
        /// Same method as GetObjectAsString(string, Dictionary) only adds a path.
        /// </summary>
        /// <param name="fileName">The name of the file to be used as a template.</param>
        /// <param name="values">A dictionary of key/value pairs that will be used to replace the placeholders in the
        /// template.</param>
        /// <param name="path">The path to the folder where the template files are located.</param>
        /// <returns>
        /// A string
        /// </returns>
        public static string GetObjectAsString(string fileName, Dictionary<string, string> values, string path)
        {
            return GetObject(fileName, values, path).ToString();
        }

        /// <summary>
        /// If the file name doesn't end with ".json", add it. If the file name starts with a backslash, remove it
        /// </summary>
        /// <param name="fileName">The name of the file you want to save to.</param>
        /// <returns>
        /// The file name with the extension .json
        /// </returns>
        public static string CheckFileName(string fileName)
        {
            if (!fileName.EndsWith(".json") && !fileName.EndsWith(".txt"))
            {
                fileName += ".json";
            }

            if (fileName.StartsWith("\\"))
            {
                fileName = fileName.Substring(1, fileName.Length);
            }

            return fileName;
        }
        
        

        /// <summary>
        /// This function is used to initialize the path of the folder where the json files are stored
        /// </summary>
        /// <param name="jsonFolder">The folder where the json files are located.</param>
        public static void Initialize(string jsonFolder)
        {
            pathDir = jsonFolder;
        }

    }
}