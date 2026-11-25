using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IAmOnTrack.UI
{
    //Builds drop-down menus that match the app's look (white surface, lavender hover) instead of the grey default.
    public static class ModernMenu
    {
        public static ContextMenuStrip Create(IList<string> items, int selectedIndex, Action<int> onPick)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Renderer = new MenuRenderer();
            menu.ShowImageMargin = false;
            menu.ShowCheckMargin = false;
            menu.BackColor = Theme.White;
            menu.Font = Theme.Font(14);
            menu.Padding = new Padding(6);
            menu.DropShadowEnabled = false;

            for (int i = 0; i < items.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(items[i]);
                item.Padding = new Padding(10, 8, 18, 8);
                item.ForeColor = Theme.Charcoal;
                item.Font = i == selectedIndex ? Theme.Semibold(14) : Theme.Font(14);
                item.Click += (s, e) => onPick(index);
                menu.Items.Add(item);
            }
            return menu;
        }

        private class MenuRenderer : ToolStripRenderer
        {
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                e.Graphics.Clear(Theme.White);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (Pen pen = new Pen(Theme.Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                }
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    Theme.HighQuality(e.Graphics);
                    Theme.FillRounded(e.Graphics, Theme.Lavender, new RectangleF(1, 1, e.Item.Width - 2, e.Item.Height - 2), 8);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Selected ? Theme.Violet : Theme.Charcoal;
                base.OnRenderItemText(e);
            }
        }
    }
}
