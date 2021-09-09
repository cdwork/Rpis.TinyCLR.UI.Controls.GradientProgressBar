using GHIElectronics.TinyCLR.UI.Media;

namespace Rpis.TinyCLR.UI.Controls
{
    internal static class Helper
    {
        public static bool ColorEquals(this Color color, Color value)
        {
            return color.A == value.A && color.R == value.R && color.G == value.G && color.B == value.B;
        }
    }
}