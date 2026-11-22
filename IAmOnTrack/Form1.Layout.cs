using System;
using System.Drawing;
using System.Windows.Forms;
using IAmOnTrack.UI;

namespace IAmOnTrack
{
    //Builds the dashboard layout from panels and table layouts (Dock / Anchor based, so it resizes).
    //Controls are created in code rather than the designer because they are custom-painted.
    public partial class DashBoardForm
    {
        private SidebarPanel _sidebar;
        private Panel _titleBar;
        private WindowControlButton _minimizeButton;
        private WindowControlButton _maximizeButton;
        private WindowControlButton _closeButton;
        private DashboardHeader _header;
        private Panel _content;
        private ActionBanner _actionBanner;
        private StatCard _modulesCard;
        private StatCard _assessmentsCard;
        private StatCard _marksCard;
        private ActivityCard _activityCard;
        private SemesterProgressCard _semesterCard;
        private PerformanceCard _performanceCard;
        private NextStepsCard _nextStepsCard;

        private void BuildInterface()
        {
            SuspendLayout();

            DoubleBuffered = true;
            Padding = new Padding(ResizeBorder);

            //start at a comfortable size, but never bigger than the screen
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(1360, area.Width - 40), Math.Min(840, area.Height - 40));

            // ---------- Sidebar ----------
            _sidebar = new SidebarPanel();
            _sidebar.Dock = DockStyle.Left;
            _sidebar.SetUser(UserFullName, UserRole);
            _sidebar.DragRequested += DragHandle_MouseDown;

            // ---------- Title bar (drag area + window buttons) ----------
            _titleBar = new Panel();
            _titleBar.Dock = DockStyle.Top;
            _titleBar.Height = 40;
            _titleBar.BackColor = Theme.OffWhite;
            _titleBar.MouseDown += DragHandle_MouseDown;

            _minimizeButton = new WindowControlButton(IconKind.Minimize);
            _minimizeButton.Dock = DockStyle.Right;
            _minimizeButton.Click += (s, e) => WindowState = FormWindowState.Minimized;

            _maximizeButton = new WindowControlButton(IconKind.Maximize);
            _maximizeButton.Dock = DockStyle.Right;
            _maximizeButton.Click += (s, e) => ToggleMaximize();

            _closeButton = new WindowControlButton(IconKind.Close);
            _closeButton.Dock = DockStyle.Right;
            _closeButton.Click += (s, e) => Close();

            //the control added LAST is docked first, so Close ends up on the far right
            _titleBar.Controls.Add(_minimizeButton);
            _titleBar.Controls.Add(_maximizeButton);
            _titleBar.Controls.Add(_closeButton);

            // ---------- Header ----------
            _header = new DashboardHeader();
            _header.Dock = DockStyle.Top;
            _header.MouseDown += DragHandle_MouseDown;

            // ---------- Scrollable content ----------
            _content = new Panel();
            _content.Dock = DockStyle.Fill;
            _content.BackColor = Theme.OffWhite;
            _content.AutoScroll = true;
            _content.AutoScrollMinSize = new Size(1100, 690); //below this size the content scrolls instead of squashing
            _content.Padding = new Padding(18, 0, 18, 10);

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.BackColor = Theme.OffWhite;
            grid.ColumnCount = 2;
            grid.RowCount = 1;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 352f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.Controls.Add(BuildLeftColumn(), 0, 0);
            grid.Controls.Add(BuildRightColumn(), 1, 0);
            _content.Controls.Add(grid);

            // ---------- Assemble ----------
            Panel main = new Panel();
            main.Dock = DockStyle.Fill;
            main.BackColor = Theme.OffWhite;
            main.Controls.Add(_content);   //Fill
            main.Controls.Add(_header);    //Top (below title bar)
            main.Controls.Add(_titleBar);  //Top (docked first = very top)

            Controls.Add(main);            //Fill
            Controls.Add(_sidebar);        //Left (docked first)

            ResumeLayout(true);
        }

        //Left column: action banner, statistics row, recent activity
        private Control BuildLeftColumn()
        {
            TableLayoutPanel left = new TableLayoutPanel();
            left.Dock = DockStyle.Fill;
            left.BackColor = Theme.OffWhite;
            left.ColumnCount = 1;
            left.RowCount = 3;
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 318f));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 108f));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _actionBanner = new ActionBanner();
            _actionBanner.Dock = DockStyle.Fill;
            _actionBanner.AddAssessmentCard.ActionButton.Click += AddAssessmentButton_Click;
            _actionBanner.EnterMarksCard.ActionButton.Click += EnterMarksButton_Click;

            _modulesCard = new StatCard("Modules", IconKind.Layers, Theme.LavenderCard, Theme.LavenderDeep, Theme.Violet);
            _assessmentsCard = new StatCard("Assessments", IconKind.Clipboard, Color.FromArgb(243, 248, 255), Theme.SkyTint, Theme.SkyDeep);
            _marksCard = new StatCard("Marks Recorded", IconKind.BarChartFilled, Theme.MintCard, Theme.MintTint, Theme.MintDeep);
            _modulesCard.Dock = DockStyle.Fill;
            _assessmentsCard.Dock = DockStyle.Fill;
            _marksCard.Dock = DockStyle.Fill;

            TableLayoutPanel stats = new TableLayoutPanel();
            stats.Dock = DockStyle.Fill;
            stats.BackColor = Theme.OffWhite;
            stats.ColumnCount = 3;
            stats.RowCount = 1;
            for (int i = 0; i < 3; i++)
            {
                stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
            }
            stats.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            stats.Controls.Add(_modulesCard, 0, 0);
            stats.Controls.Add(_assessmentsCard, 1, 0);
            stats.Controls.Add(_marksCard, 2, 0);

            _activityCard = new ActivityCard();
            _activityCard.Dock = DockStyle.Fill;

            left.Controls.Add(_actionBanner, 0, 0);
            left.Controls.Add(stats, 0, 1);
            left.Controls.Add(_activityCard, 0, 2);
            return left;
        }

        //Right column: semester progress, performance overview, next steps
        private Control BuildRightColumn()
        {
            TableLayoutPanel right = new TableLayoutPanel();
            right.Dock = DockStyle.Fill;
            right.BackColor = Theme.OffWhite;
            right.ColumnCount = 1;
            right.RowCount = 3;
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 240f));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 206f));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _semesterCard = new SemesterProgressCard();
            _performanceCard = new PerformanceCard();
            _nextStepsCard = new NextStepsCard();
            _semesterCard.Dock = DockStyle.Fill;
            _performanceCard.Dock = DockStyle.Fill;
            _nextStepsCard.Dock = DockStyle.Fill;

            right.Controls.Add(_semesterCard, 0, 0);
            right.Controls.Add(_performanceCard, 0, 1);
            right.Controls.Add(_nextStepsCard, 0, 2);
            return right;
        }
    }
}
