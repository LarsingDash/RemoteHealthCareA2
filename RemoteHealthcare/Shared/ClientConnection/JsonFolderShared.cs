using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Shared
{
    internal class JsonFolderShared
    {
        JsonFolderShared(string path)
        {
            this.Path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + path;
        }

        public string Path { get; }

        public override string ToString(){ return Path; }

        public static readonly JsonFolderShared Data = new JsonFolderShared("Data\\");
        public static readonly JsonFolderShared Json = new JsonFolderShared("Json\\");
        public static readonly JsonFolderShared ClientMessages = new JsonFolderShared("Json\\ClientMessages\\");
    }
}