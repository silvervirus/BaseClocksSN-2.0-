using UnityEngine;

namespace BaseClocks
{
    public static class ColorExtensions
    {
        public static Color SetRed(this Color c, float r)
        {
            return new Color(r, c.g, c.b);
        }

        public static Color SetGreen(this Color c, float g)
        {
            return new Color(c.r, g, c.b);
        }

        public static Color SetBlue(this Color c, float b)
        {
            return new Color(c.r, c.g, b);
        }

        public static Color SetAlpha(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
        public static Color SetBodyRed(this Color c1, float r1)
        {
            return new Color(r1, c1.g, c1.b);
        }

        public static Color SetBodyGreen(this Color c1, float g1)
        {
            return new Color(c1.r, g1, c1.b);
        }

        public static Color SetBodyBlue(this Color c1, float b1)
        {
            return new Color(c1.r, c1.g, b1);
        }

        public static Color SetBodyAlpha(this Color c1, float a1)
        {
            return new Color(c1.r, c1.g, c1.b, a1);
        }
    }
}
