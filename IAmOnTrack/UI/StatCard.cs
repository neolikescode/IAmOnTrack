using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Compact statistic card: icon tile, label and a prominent number
    public class StatCard : RoundedPanel
    {
        private readonly string _label;
        private readonly IconKind _icon;
        private readonly Color _tileColor;
        private readonly Color _iconColor;
        private int _value;
        private string _valueText;

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Invalidate();
            }
        }

        //When set, this text is shown instead of the number (e.g. "76.5%" or a dash)
        public string ValueText
        {
            get { return _valueText; }
            set
            {
                _valueText = value;
                Invalidate();
            }
        }

        public StatCard(string label, IconKind icon, Color cardColor, Color tileColor, Color iconColor)
        {
            _label = label;
            _icon = icon;
            _tileColor = tileColor;
            _iconColor = iconColor;
            CardColor = cardColor;
            CornerRadius = 16;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 120 || b.Height < 60)
            {
                return;
            }

            using (var clip = CardPath())
            {
                g.SetClip(clip);
                using (SolidBrush brush = new SolidBrush(Theme.WithAlpha(_tileColor, 140)))
                {
                    g.FillEllipse(brush, b.Right - 90, b.Bottom - 56, 150, 150);
                }
                g.ResetClip();
            }

            int tileSize = 56;
            Rectangle tile = new Rectangle(b.X + 20, b.Y + (b.Height - tileSize) / 2, tileSize, tileSize);
            Theme.FillRounded(g, _tileColor, tile, 16);
            Icons.Draw(g, _icon, new RectangleF(tile.X + 15, tile.Y + 15, 26, 26), _iconColor, 1.8f);

            int textX = tile.Right + 16;
            int textWidth = Math.Max(40, b.Right - 16 - textX);
            Theme.Text(g, _label, Theme.Semibold(14), new RectangleF(textX, tile.Y - 2, textWidth, 20), Theme.Charcoal, Theme.SingleLine);
            Theme.Text(g, _valueText ?? _value.ToString(), Theme.Font(34, FontStyle.Bold), new RectangleF(textX, tile.Y + 18, textWidth, 42), Theme.Charcoal, Theme.SingleLine);
        }
    }
}
