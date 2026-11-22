using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace IAmOnTrack.UI
{
    public enum IconKind
    {
        None, Home, Layers, Clipboard, BarChart, BarChartFilled, Folder, Gear, Search, Bell,
        DocPlus, Clock, Calendar, Target, Plus, Arrow, Inbox, GradCap, Chevron,
        Minimize, Maximize, Restore, Close
    }

    //Every icon is drawn as vector lines on a 24 x 24 grid, so icons are crisp at any size
    //and no image files need to be shipped with the project.
    public static class Icons
    {
        private static PointF P(float x, float y)
        {
            return new PointF(x, y);
        }

        private static void BeginGrid(Graphics g, RectangleF bounds, out GraphicsState saved)
        {
            saved = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = Math.Min(bounds.Width, bounds.Height) / 24f;
            g.TranslateTransform(bounds.X + (bounds.Width - 24f * s) / 2f, bounds.Y + (bounds.Height - 24f * s) / 2f);
            g.ScaleTransform(s, s);
        }

        //stroke is measured in grid units (1.8 = about 7.5% of the icon size)
        public static void Draw(Graphics g, IconKind kind, RectangleF bounds, Color color, float stroke = 1.8f)
        {
            if (kind == IconKind.None)
            {
                return;
            }

            GraphicsState saved;
            BeginGrid(g, bounds, out saved);

            using (Pen pen = new Pen(color, stroke))
            using (SolidBrush brush = new SolidBrush(color))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                switch (kind)
                {
                    case IconKind.Home:
                        g.DrawLines(pen, new[] { P(3f, 11.5f), P(12f, 3.5f), P(21f, 11.5f) });
                        g.DrawLines(pen, new[] { P(5.5f, 10f), P(5.5f, 20.5f), P(18.5f, 20.5f), P(18.5f, 10f) });
                        g.DrawLines(pen, new[] { P(10f, 20.5f), P(10f, 14.5f), P(14f, 14.5f), P(14f, 20.5f) });
                        break;

                    case IconKind.Layers:
                        g.DrawPolygon(pen, new[] { P(12f, 2.8f), P(21.5f, 7.6f), P(12f, 12.4f), P(2.5f, 7.6f) });
                        g.DrawLines(pen, new[] { P(2.5f, 12f), P(12f, 16.8f), P(21.5f, 12f) });
                        g.DrawLines(pen, new[] { P(2.5f, 16.4f), P(12f, 21.2f), P(21.5f, 16.4f) });
                        break;

                    case IconKind.Clipboard:
                        using (GraphicsPath body = Theme.RoundedRect(new RectangleF(5f, 4.5f, 14f, 17.3f), 2.6f))
                        {
                            g.DrawPath(pen, body);
                        }
                        using (GraphicsPath clip = Theme.RoundedRect(new RectangleF(9f, 2.4f, 6f, 4.2f), 1.5f))
                        {
                            g.DrawPath(pen, clip);
                        }
                        g.DrawLine(pen, 8.6f, 11.6f, 15.4f, 11.6f);
                        g.DrawLine(pen, 8.6f, 15.6f, 13.2f, 15.6f);
                        break;

                    case IconKind.BarChart:
                    case IconKind.BarChartFilled:
                        RectangleF[] bars =
                        {
                            new RectangleF(3.5f, 13f, 4f, 8f),
                            new RectangleF(10f, 8f, 4f, 13f),
                            new RectangleF(16.5f, 3f, 4f, 18f)
                        };
                        foreach (RectangleF bar in bars)
                        {
                            using (GraphicsPath p = Theme.RoundedRect(bar, 1.3f))
                            {
                                if (kind == IconKind.BarChartFilled)
                                {
                                    g.FillPath(brush, p);
                                }
                                else
                                {
                                    g.DrawPath(pen, p);
                                }
                            }
                        }
                        break;

                    case IconKind.Folder:
                        g.DrawPolygon(pen, new[] { P(3f, 6.5f), P(9f, 6.5f), P(11.5f, 9f), P(21f, 9f), P(21f, 19f), P(3f, 19f) });
                        break;

                    case IconKind.Gear:
                        g.DrawEllipse(pen, 12f - 3.2f, 12f - 3.2f, 6.4f, 6.4f);
                        g.DrawEllipse(pen, 12f - 6.6f, 12f - 6.6f, 13.2f, 13.2f);
                        using (Pen tooth = new Pen(color, stroke * 1.8f))
                        {
                            tooth.StartCap = LineCap.Round;
                            tooth.EndCap = LineCap.Round;
                            for (int i = 0; i < 8; i++)
                            {
                                double a = i * Math.PI / 4.0;
                                float cos = (float)Math.Cos(a);
                                float sin = (float)Math.Sin(a);
                                g.DrawLine(tooth, 12f + cos * 7.6f, 12f + sin * 7.6f, 12f + cos * 9.3f, 12f + sin * 9.3f);
                            }
                        }
                        break;

                    case IconKind.Search:
                        g.DrawEllipse(pen, 3.5f, 3.5f, 13f, 13f);
                        g.DrawLine(pen, 14.8f, 14.8f, 20.5f, 20.5f);
                        break;

                    case IconKind.Bell:
                        using (GraphicsPath bell = new GraphicsPath())
                        {
                            bell.AddArc(7f, 4f, 10f, 10f, 180f, 180f);
                            bell.AddLine(17f, 9f, 17.5f, 14.5f);
                            bell.AddLine(17.5f, 14.5f, 19.5f, 17f);
                            bell.AddLine(19.5f, 17f, 4.5f, 17f);
                            bell.AddLine(4.5f, 17f, 6.5f, 14.5f);
                            bell.CloseFigure();
                            g.DrawPath(pen, bell);
                        }
                        g.DrawLine(pen, 12f, 2.4f, 12f, 4.2f);
                        g.DrawArc(pen, 9.6f, 17.6f, 4.8f, 3.6f, 0f, 180f);
                        break;

                    case IconKind.DocPlus:
                        g.DrawLines(pen, new[] { P(13.5f, 2.8f), P(5.5f, 2.8f), P(5.5f, 21.2f), P(11f, 21.2f) });
                        g.DrawLines(pen, new[] { P(13.5f, 2.8f), P(18.5f, 7.8f), P(18.5f, 11f) });
                        g.DrawLines(pen, new[] { P(13.5f, 2.8f), P(13.5f, 7.8f), P(18.5f, 7.8f) });
                        g.DrawLine(pen, 8.6f, 11.8f, 12.4f, 11.8f);
                        g.DrawLine(pen, 8.6f, 15.4f, 10.8f, 15.4f);
                        g.FillEllipse(brush, 12.2f, 12.2f, 10.6f, 10.6f);
                        using (Pen plus = new Pen(Color.White, stroke))
                        {
                            plus.StartCap = LineCap.Round;
                            plus.EndCap = LineCap.Round;
                            g.DrawLine(plus, 17.5f, 14.7f, 17.5f, 20.3f);
                            g.DrawLine(plus, 14.7f, 17.5f, 20.3f, 17.5f);
                        }
                        break;

                    case IconKind.Clock:
                        g.DrawEllipse(pen, 3f, 3f, 18f, 18f);
                        g.DrawLines(pen, new[] { P(12f, 7.2f), P(12f, 12f), P(15.4f, 14f) });
                        break;

                    case IconKind.Calendar:
                        using (GraphicsPath cal = Theme.RoundedRect(new RectangleF(3.5f, 5f, 17f, 15.5f), 2.6f))
                        {
                            g.DrawPath(pen, cal);
                        }
                        g.DrawLine(pen, 3.5f, 10f, 20.5f, 10f);
                        g.DrawLine(pen, 8f, 2.6f, 8f, 6.6f);
                        g.DrawLine(pen, 16f, 2.6f, 16f, 6.6f);
                        break;

                    case IconKind.Target:
                        g.DrawEllipse(pen, 3f, 3f, 18f, 18f);
                        g.DrawEllipse(pen, 7f, 7f, 10f, 10f);
                        g.FillEllipse(brush, 10.4f, 10.4f, 3.2f, 3.2f);
                        break;

                    case IconKind.Plus:
                        g.DrawLine(pen, 12f, 5f, 12f, 19f);
                        g.DrawLine(pen, 5f, 12f, 19f, 12f);
                        break;

                    case IconKind.Arrow:
                        g.DrawLine(pen, 4.5f, 12f, 19.5f, 12f);
                        g.DrawLines(pen, new[] { P(13.5f, 6f), P(19.5f, 12f), P(13.5f, 18f) });
                        break;

                    case IconKind.Inbox:
                        g.DrawPolygon(pen, new[] { P(3f, 13f), P(6.6f, 4.6f), P(17.4f, 4.6f), P(21f, 13f), P(21f, 19f), P(3f, 19f) });
                        g.DrawLines(pen, new[] { P(3f, 13f), P(8.4f, 13f), P(10f, 16f), P(14f, 16f), P(15.6f, 13f), P(21f, 13f) });
                        break;

                    case IconKind.GradCap:
                        g.FillPolygon(brush, new[] { P(12f, 4.5f), P(23f, 9.5f), P(12f, 14.5f), P(1f, 9.5f) });
                        g.DrawLines(pen, new[] { P(6f, 12.6f), P(6f, 17f), P(12f, 19.8f), P(18f, 17f), P(18f, 12.6f) });
                        g.DrawLine(pen, 22f, 10.5f, 22f, 16.5f);
                        break;

                    case IconKind.Chevron:
                        g.DrawLines(pen, new[] { P(6f, 9f), P(12f, 15f), P(18f, 9f) });
                        break;

                    case IconKind.Minimize:
                        g.DrawLine(pen, 4f, 12f, 20f, 12f);
                        break;

                    case IconKind.Maximize:
                        g.DrawRectangle(pen, 5f, 5f, 14f, 14f);
                        break;

                    case IconKind.Restore:
                        g.DrawRectangle(pen, 4.5f, 9f, 10.5f, 10.5f);
                        g.DrawLines(pen, new[] { P(9f, 9f), P(9f, 4.5f), P(19.5f, 4.5f), P(19.5f, 15f), P(15f, 15f) });
                        break;

                    case IconKind.Close:
                        g.DrawLine(pen, 5f, 5f, 19f, 19f);
                        g.DrawLine(pen, 19f, 5f, 5f, 19f);
                        break;
                }
            }
            g.Restore(saved);
        }

        //The IAmOnTrack logo: three stacked layers (organised modules) that step upwards (progress)
        //under a flat top face (achievement). It is drawn in code so it needs no image file.
        public static void DrawLogo(Graphics g, RectangleF bounds)
        {
            GraphicsState saved;
            BeginGrid(g, bounds, out saved);
            using (SolidBrush top = new SolidBrush(Color.White))
            using (SolidBrush middle = new SolidBrush(ColorTranslator.FromHtml("#CFC3FF")))
            using (SolidBrush bottom = new SolidBrush(ColorTranslator.FromHtml("#9F89FF")))
            {
                g.FillPolygon(bottom, new[] { P(1.5f, 16.4f), P(12f, 21.8f), P(22.5f, 16.4f), P(22.5f, 18.7f), P(12f, 24f), P(1.5f, 18.7f) });
                g.FillPolygon(middle, new[] { P(1.5f, 11.2f), P(12f, 16.6f), P(22.5f, 11.2f), P(22.5f, 13.5f), P(12f, 18.9f), P(1.5f, 13.5f) });
                g.FillPolygon(top, new[] { P(12f, 0.8f), P(22.5f, 6.2f), P(12f, 11.6f), P(1.5f, 6.2f) });
            }
            g.Restore(saved);
        }
    }
}
