using System;
using System.Drawing;
using System.Windows.Forms;
using IAmOnTrack.UI;

namespace IAmOnTrack
{
    //Builds the Records page layout (same design system and chrome as the dashboard)
    public partial class RecordsForm
    {
        private SidebarPanel _sidebar;
        private Panel _titleBar;
        private PageHeader _header;
        private Panel _content;
        private StatCard _modulesCard;
        private StatCard _assessmentsCard;
        private StatCard _marksCard;
        private StatCard _averageCard;
        private RoundedPanel _tableCard;
        private TabStrip _tabs;
        private FilterDropdown _yearFilter;
        private FilterDropdown _semesterFilter;
        private FilterDropdown _moduleFilter;
        private FilterDropdown _typeFilter;
        private FilterDropdown _statusFilter;
        private RecordGrid _grid;
        private PagerBar _pagerBar;
        private RoundedButton _exportButton;
        private RoundedButton _refreshButton;

        private void BuildInterface()
        {
            SuspendLayout();

            Padding = new Padding(ResizeBorder);
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            ClientSize = new Size(Math.Min(1360, area.Width - 40), Math.Min(840, area.Height - 40));

            // ---------- Sidebar ----------
            _sidebar = new SidebarPanel();
            _sidebar.Dock = DockStyle.Left;
            _sidebar.SetUser(CurrentUser.FullName, CurrentUser.Role);
            _sidebar.DragRequested += DragHandle_MouseDown;
            _sidebar.Navigate += Sidebar_Navigate;

            // ---------- Title bar + header ----------
            _titleBar = CreateTitleBar();

            _header = new PageHeader("Search records...", "Add Record");
            _header.Dock = DockStyle.Top;
            _header.SetText("Academic Records", "Manage your modules, assessments and academic performance.");
            _header.MouseDown += DragHandle_MouseDown;
            _header.Search.QueryChanged += Search_QueryChanged;
            _header.PrimaryButton.Click += AddRecordButton_Click;

            // ---------- Scrollable content ----------
            _content = new Panel();
            _content.Dock = DockStyle.Fill;
            _content.BackColor = Theme.OffWhite;
            _content.AutoScroll = true;
            _content.AutoScrollMinSize = new Size(1000, 640);
            _content.Padding = new Padding(18, 0, 18, 6);

            TableLayoutPanel body = new TableLayoutPanel();
            body.Dock = DockStyle.Fill;
            body.BackColor = Theme.OffWhite;
            body.ColumnCount = 1;
            body.RowCount = 3;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 108f));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 66f));
            body.Controls.Add(BuildOverviewRow(), 0, 0);
            body.Controls.Add(BuildTableCard(), 0, 1);
            body.Controls.Add(BuildFooter(), 0, 2);
            _content.Controls.Add(body);

            // ---------- Assemble ----------
            Panel main = new Panel();
            main.Dock = DockStyle.Fill;
            main.BackColor = Theme.OffWhite;
            main.Controls.Add(_content);
            main.Controls.Add(_header);
            main.Controls.Add(_titleBar);

            Controls.Add(main);
            Controls.Add(_sidebar);

            ResumeLayout(true);
        }

        //Four overview cards (reusing the dashboard's StatCard)
        private Control BuildOverviewRow()
        {
            _modulesCard = new StatCard("Modules", IconKind.Layers, Theme.LavenderCard, Theme.LavenderDeep, Theme.Violet);
            _assessmentsCard = new StatCard("Assessments", IconKind.Clipboard, Color.FromArgb(243, 248, 255), Theme.SkyTint, Theme.SkyDeep);
            _marksCard = new StatCard("Marks Recorded", IconKind.BarChartFilled, Theme.MintCard, Theme.MintTint, Theme.MintDeep);
            _averageCard = new StatCard("Overall Average", IconKind.Target, Theme.LavenderCard, Theme.LavenderDeep, Theme.Violet);

            TableLayoutPanel row = new TableLayoutPanel();
            row.Dock = DockStyle.Fill;
            row.BackColor = Theme.OffWhite;
            row.ColumnCount = 4;
            row.RowCount = 1;
            for (int i = 0; i < 4; i++)
            {
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            StatCard[] cards = { _modulesCard, _assessmentsCard, _marksCard, _averageCard };
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Dock = DockStyle.Fill;
                row.Controls.Add(cards[i], i, 0);
            }
            return row;
        }

        //The big white card: tabs, filters, table and pager
        private Control BuildTableCard()
        {
            _tableCard = new RoundedPanel();
            _tableCard.Dock = DockStyle.Fill;
            _tableCard.CornerRadius = 18;
            _tableCard.HoverEffect = false;
            _tableCard.Padding = new Padding(RoundedPanel.ShadowInset.Left + 20, RoundedPanel.ShadowInset.Top + 12,
                                             RoundedPanel.ShadowInset.Right + 20, RoundedPanel.ShadowInset.Bottom + 6);

            _tabs = new TabStrip("All Records", "Modules", "Assessments", "Marks");
            _tabs.Dock = DockStyle.Top;
            _tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;

            _yearFilter = new FilterDropdown("Academic Year");
            _semesterFilter = new FilterDropdown("Semester");
            _moduleFilter = new FilterDropdown("Module");
            _typeFilter = new FilterDropdown("Type");
            _statusFilter = new FilterDropdown("Status");

            FlowLayoutPanel filters = new FlowLayoutPanel();
            filters.Dock = DockStyle.Top;
            filters.AutoSize = true;
            filters.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            filters.WrapContents = true;
            filters.BackColor = Theme.White;
            filters.Padding = new Padding(0, 8, 0, 4);
            foreach (FilterDropdown dropdown in new[] { _yearFilter, _semesterFilter, _moduleFilter, _typeFilter, _statusFilter })
            {
                dropdown.SelectionChanged += Filter_SelectionChanged;
                filters.Controls.Add(dropdown);
            }

            _grid = new RecordGrid();
            _grid.Dock = DockStyle.Fill;
            _grid.ActionInvoked += Grid_ActionInvoked;
            _grid.EmptyActionClicked += Grid_EmptyActionClicked;
            _grid.Resize += Grid_Resize;

            _pagerBar = new PagerBar();
            _pagerBar.Dock = DockStyle.Bottom;
            _pagerBar.PreviousClicked += (s, e) => { _pager.Previous(); ApplyView(); };
            _pagerBar.NextClicked += (s, e) => { _pager.Next(); ApplyView(); };

            //docking: the control added last is laid out first
            _tableCard.Controls.Add(_grid);
            _tableCard.Controls.Add(_pagerBar);
            _tableCard.Controls.Add(filters);
            _tableCard.Controls.Add(_tabs);
            return _tableCard;
        }

        private Control BuildFooter()
        {
            Panel footer = new Panel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = Theme.OffWhite;

            _exportButton = CreateSecondaryButton("Export to Excel", IconKind.Download);
            _exportButton.Location = new Point(RoundedPanel.ShadowInset.Left, 12);
            _exportButton.Click += ExportButton_Click;

            _refreshButton = CreateSecondaryButton("Refresh Records", IconKind.Refresh);
            _refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _refreshButton.Location = new Point(footer.Width - RoundedPanel.ShadowInset.Right - _refreshButton.Width, 12);
            _refreshButton.Click += (s, e) => RefreshRecords();

            footer.Controls.Add(_exportButton);
            footer.Controls.Add(_refreshButton);
            footer.Resize += (s, e) => _refreshButton.Left = footer.Width - RoundedPanel.ShadowInset.Right - _refreshButton.Width;
            return footer;
        }

        private static RoundedButton CreateSecondaryButton(string text, IconKind icon)
        {
            RoundedButton button = new RoundedButton();
            button.Text = text;
            button.LeadingIcon = icon;
            button.Font = Theme.Semibold(14.5f);
            button.BackColor = Theme.OffWhite;
            button.ButtonColor = Theme.Lavender;
            button.HoverColor = Theme.LavenderDeep;
            button.PressedColor = Theme.LavenderDeep;
            button.TextColor = Theme.Violet;
            button.CornerRadius = 12;
            button.Size = new Size(178, 46);
            return button;
        }
    }
}
