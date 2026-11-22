using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Central place for the colour palette, fonts and small drawing helpers so the whole
    //dashboard stays visually consistent.
    public static class Theme
    {
        // ---- Palette (from the design brief) ----
        public static readonly Color Navy = ColorTranslator.FromHtml("#171829");        // sidebar
        public static readonly Color Violet = ColorTranslator.FromHtml("#7657FF");      // primary
        public static readonly Color Lavender = ColorTranslator.FromHtml("#EEE9FF");    // selected / subtle bg
        public static readonly Color Mint = ColorTranslator.FromHtml("#A8E6CF");        // success / secondary
        public static readonly Color OffWhite = ColorTranslator.FromHtml("#F7F7FB");    // app background
        public static readonly Color White = Color.White;                               // cards
        public static readonly Color Charcoal = ColorTranslator.FromHtml("#282938");    // primary text

        // ---- Supporting tones derived from the palette ----
        public static readonly Color VioletHover = ColorTranslator.FromHtml("#6847F0");
        public static readonly Color VioletPressed = ColorTranslator.FromHtml("#5A3CD9");
        public static readonly Color LavenderDeep = ColorTranslator.FromHtml("#E0D8FF");
        public static readonly Color LavenderCard = ColorTranslator.FromHtml("#F7F4FF");
        public static readonly Color MintCard = ColorTranslator.FromHtml("#F1FAF6");
        public static readonly Color MintTint = ColorTranslator.FromHtml("#D6F3E7");
        public static readonly Color MintDeep = ColorTranslator.FromHtml("#2DAA7A");
        public static readonly Color SkyTint = ColorTranslator.FromHtml("#E2ECFF");
        public static readonly Color SkyDeep = ColorTranslator.FromHtml("#4C7DF0");
        public static readonly Color Banner = ColorTranslator.FromHtml("#2E2468");
        public static readonly Color NavyHover = ColorTranslator.FromHtml("#242640");
        public static readonly Color NavyRaised = ColorTranslator.FromHtml("#1F2137");
        public static readonly Color NavyText = ColorTranslator.FromHtml("#C6C8DF");
        public static readonly Color NavyMuted = ColorTranslator.FromHtml("#8B8DAA");
        public static readonly Color Muted = ColorTranslator.FromHtml("#7C7D92");
        public static readonly Color Border = ColorTranslator.FromHtml("#E8E8F1");
        public static readonly Color TrackGrey = ColorTranslator.FromHtml("#E9E9F1");
        public static readonly Color Danger = ColorTranslator.FromHtml("#E5484D");

        // ---- Text flags used with TextRenderer ----
        public const TextFormatFlags Base = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
        public const TextFormatFlags SingleLine = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
        public const TextFormatFlags Centered = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
        public const TextFormatFlags Wrapped = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;
        public const TextFormatFlags WrappedCentered = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak;

        // ---- Fonts (sizes are in pixels so layouts stay predictable) ----
        private static readonly Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        public static Font Font(float pixels, FontStyle style = FontStyle.Regular)
        {
            return GetFont("Segoe UI", pixels, style);
        }

        //Segoe UI Semibold gives the "medium weight" section titles from the brief
        public static Font Semibold(float pixels)
        {
            return GetFont("Segoe UI Semibold", pixels, FontStyle.Regular);
        }

        private static Font GetFont(string family, float pixels, FontStyle style)
        {
            string key = family + "|" + pixels + "|" + (int)style;
            Font font;
            if (!_fonts.TryGetValue(key, out font))
            {
                font = new Font(family, pixels, style, GraphicsUnit.Pixel);
                _fonts[key] = font;
            }
            return font;
        }

        // ---- Drawing helpers ----
        public static void HighQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
            if (d <= 0.5f)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRounded(Graphics g, Color color, RectangleF r, float radius)
        {
            using (GraphicsPath path = RoundedRect(r, radius))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }
        }

        //Soft shadow made of a few translucent rounded rectangles (cheap and good looking)
        public static void DrawShadow(Graphics g, RectangleF body, float radius, int blur, int alpha, float offsetY)
        {
            for (int i = blur; i >= 1; i--)
            {
                float fade = 1f - ((float)(i - 1) / blur);
                int a = Math.Max(1, (int)(alpha * fade * fade));
                RectangleF r = body;
                r.Inflate(i, i);
                r.Y += offsetY;
                using (GraphicsPath path = RoundedRect(r, radius + i))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(a, 45, 35, 110)))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        public static Color Blend(Color from, Color to, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(from.A + (to.A - from.A) * t),
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t));
        }

        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(alpha, color);
        }

        public static void Text(Graphics g, string text, Font font, RectangleF bounds, Color color, TextFormatFlags flags)
        {
            TextRenderer.DrawText(g, text, font, Rectangle.Round(bounds), color, flags | Base);
        }

        public static Size Measure(Graphics g, string text, Font font)
        {
            return TextRenderer.MeasureText(g, text, font, new Size(int.MaxValue, int.MaxValue), Base);
        }
    }
}
