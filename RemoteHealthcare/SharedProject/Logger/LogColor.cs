namespace ServerApplication.Log
{
    public class LogColor
    {
        LogColor(string color)
        {
            this.Color = color;
        }

        public string Color { get; }

        /// <summary>
        /// The ToString() function is a built-in function that returns a string representation of an object
        /// </summary>
        /// <returns>
        /// The color of the car.
        /// </returns>
        public override string ToString()
        {
            return Color;
        }


        public static readonly LogColor Gray = new ("\u001B[90m");
        public static readonly LogColor Red = new ("\u001B[31m");
        public static readonly LogColor Green = new ("\u001B[32m");
        public static readonly LogColor Yellow = new ("\u001B[33m");
        public static readonly LogColor LightYellow = new ("\u001B[93m");
        public static readonly LogColor Blue = new ("\u001B[34m");
        public static readonly LogColor Magenta = new ("\u001B[35m");
        public static readonly LogColor Cyan = new ("\u001B[36m");
        public static readonly LogColor White = new ("\u001B[37m");
    
        public static readonly LogColor Bold = new ("\u001B[1m");
        public static readonly LogColor UnBold = new ("\u001B[21m");
        public static readonly LogColor Underline = new ("\u001B[4m");
        public static readonly LogColor StopUnderline = new ("\u001B[24m");
        public static readonly LogColor Blink = new ("\u001B[5m");
        public static readonly LogColor Reset = new ("\u001B[0m");
    
    }
}