namespace Shared.Log
{
    public class LogImportance
    {
        LogImportance(string name, int importance, string colorCode)
        {
            this.Name = name;
            this.Level = importance;
            this.ColorCode = colorCode;
        }

        public string Name { get; }
        public int Level { get; }
        public string ColorCode { get; }

        /// <summary>
        /// The `ToString()` function is used to convert an object to a string
        /// </summary>
        /// <returns>
        /// The name of the object.
        /// </returns>
        public override string ToString() { return Name; }

        public static readonly LogImportance Fatal = new("Fatal", 1, LogColor.Red.Color);
        public static readonly LogImportance DebugHighlight = new("DebugHighLight", 1, LogColor.Magenta.Color);
        public static readonly LogImportance Error = new("Error", 2, LogColor.Yellow.Color);
        public static readonly LogImportance Warn = new("Warn", 3, LogColor.LightYellow.Color);
        public static readonly LogImportance Information = new("Information", 4, LogColor.Cyan.Color);
        public static readonly LogImportance Debug = new("Debug", 5, LogColor.Green.Color);
    }
}