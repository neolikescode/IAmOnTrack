using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //One of the two large "what would you like to do" cards (icon tile, title, description, button)
    public class ActionCard : RoundedPanel
    {
        private const int Pad = 20;
        private const int TileSize = 64;

        private readonly IconKind _icon;
        private readonly string _title;
        private readonly string _description;
        private readonly Color _tileColor;
        private readonly Color _iconColor;

        public RoundedButton ActionButton { get; private set; }

        public ActionCard(IconKind icon, string title, string description, string buttonText,
                          Color cardColor, Color tileColor, Color iconColor)
        {
            _icon = icon;
            _title = title;
            _description = description;
            _tileColor = tileColor;
            _iconColor = iconColor;

            CardColor = cardColor;
            DrawBorder = false;
            CornerRadius = 16;

            ActionButton = new RoundedButton();
            ActionButton.Text = buttonText;
            ActionButton.TrailingIcon = IconKind.Arrow;
            ActionButton.BackColor = cardColor;
            Controls.Add(ActionButton);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (ActionButton == null)
            {
                return;
            }
            Rectangle b = BodyBounds;
            int textX = b.X + Pad + TileSize + 16;
            int textWidth = Math.Max(120, b.Right - Pad - textX);
            ActionButton.SetBounds(textX, b.Bottom - Pad - 44, Math.Min(textWidth, 214), 44);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle b = BodyBounds;
            if (b.Width < 200 || b.Height < 100)
            {
                return;
            }

            //soft decorative circle in the bottom-right corner, clipped to the card
            using (var clip = CardPath())
            {
                g.SetClip(clip);
                using (SolidBrush brush = new SolidBrush(Theme.WithAlpha(_tileColor, 150)))
                {
                    g.FillEllipse(brush, b.Right - 120, b.Bottom - 70, 200, 200);
                }
                g.ResetClip();
            }

            //icon tile
            Rectangle tile = new Rectangle(b.X + Pad, b.Y + Pad + 2, TileSize, TileSize);
            Theme.FillRounded(g, _tileColor, tile, 18);
            Icons.Draw(g, _icon, new RectangleF(tile.X + 16, tile.Y + 16, 32, 32), _iconColor, 1.7f);

            int textX = b.X + Pad + TileSize + 16;
            int textWidth = Math.Max(120, b.Right - Pad - textX);
            Theme.Text(g, _title, Theme.Semibold(21), new RectangleF(textX, b.Y + Pad, textWidth, 30), Theme.Charcoal, Theme.SingleLine);
            Theme.Text(g, _description, Theme.Font(13.5f), new RectangleF(textX, b.Y + Pad + 34, textWidth, 56), Theme.Muted, Theme.Wrapped);
        }
    }
}
