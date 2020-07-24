using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Windows
{
    public static class ExtensionsMethods
    {
        public static Color GetTransparence(this Color color)
        {
            return GetTransparence(color, (color.GetBrightness() * 1.8F));
        }

        public static Color GetTransparence(this Color color, float coeficiente)
        {
            float r, g, b;

            Func<float, int> validate = (val) =>
            {
                if (val > 255)
                    return 255;
                return (int)val;
            };

            r = validate(color.R * coeficiente);
            g = validate(color.G * coeficiente);
            b = validate(color.B * coeficiente);

            return Color.FromArgb((int)r, (int)g, (int)b);
        }
    }
}
