using System.Drawing;

namespace ContextHubDev
{
    public class FenceData
    {
        public string Name { get; set; } = "Нова зона";
        public int X { get; set; } = 100;
        public int Y { get; set; } = 100;
        public int W { get; set; } = 300;
        public int H { get; set; } = 400;
        public Color ThemeColor { get; set; } = Color.FromArgb(180, 45, 52, 54);
    }
}