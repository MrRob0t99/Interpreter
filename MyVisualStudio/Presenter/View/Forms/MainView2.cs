using HtmlAgilityPack;
using MyVisualStudio.Presenter;
using MyVisualStudio.View.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyVisualStudio
{
    public partial class MainView : Form, IMainView
    {

        public event EventHandler NewStripMenuItemClick = delegate { };
        public event EventHandler OpenStripMenuItemClick = delegate { };
        public event EventHandler SaveProject = delegate { };
        public event EventHandler RunProject = delegate { };
        public event EventHandler FormLoad = delegate { };

        public TabControl TabControl
        {
            get { return tabControl; }
            set { value = tabControl; }
        }

        public MainView()
        {
            InitializeComponent();
            var presenter = new MainPresenter(this);
        }

        #region Themes

        private void LoadDarkTheme()
        {
            BackColor = Color.FromArgb(45, 45, 48);
            //richTextBox1.BackColor = Color.FromArgb(28, 28, 28);
            menuStrip1.BackColor = Color.FromArgb(45, 45, 48);
            fileToolStripMenuItem.ForeColor = Color.White;
            newToolStripMenuItem.BackColor = Color.Blue;
            button1.BackColor = Color.FromArgb(45, 45, 48);
            button1.ForeColor = Color.White;
        }

        #endregion

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            //for (int i = 0; i < this.tabControl.TabPages.Count; i++)
            //{
            //    var r = tabControl.GetTabRect(i);
            //    //Getting the position of the "x" mark.
            //    var closeButton = new Rectangle(r.Right - 15, r.Top + 4, 9, 7);
            //    if (closeButton.Contains(e.Location))
            //    {
            //        if (MessageBox.Show("Would you like to Close this Tab?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            //        {
            //            tabControl.TabPages.RemoveAt(i);
            //            break;
            //        }
            //    }
            //}
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MainView_Load(object sender, EventArgs e)
        {
            LoadDarkTheme();
            button1.Enabled = false;
            button2.Enabled = false;
            button1.ForeColor = Color.White;


            ///////////News Block
            var tabPage = new TabPage();
            tabControl.SelectedTab = tabPage;
            tabPage.Text = "MainPage";
            tabPage.Name = "MainPage";
            tabPage.BackColor = Color.FromArgb(37, 37, 38);
            tabControl.Controls.Add(tabPage);
            var panel = new Panel();
            panel.Name = "UnderPanel";
            panel.Dock = DockStyle.Right;
            panel.Width = 400;
            panel.BackColor = Color.FromArgb(45, 45, 48);
            tabPage.Controls.Add(panel);
            var label = new Label();
            label.Text = "Developer news";
            label.ForeColor = Color.FromArgb(104, 154, 188);
            label.Dock = DockStyle.Top;
            label.Width = panel.Width;
            label.Padding = new Padding(110, 0, 0, 0);
            label.Font = new Font("Arial", 15, FontStyle.Bold);
            panel.Controls.Add(label);
            var innerPanel = new Panel();
            innerPanel.Dock = DockStyle.Bottom;
            innerPanel.Height = 600;
            innerPanel.Name = "InnerPanel";
            innerPanel.AutoScroll = true;
            panel.Controls.Add(innerPanel);
            FormLoad(sender, e);
            ///////////////////
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(0, 122, 204);
            RunProject(this, e);
        }

        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {

            OpenStripMenuItemClick(this, e);
        }

        private void newToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            NewStripMenuItemClick(this, e);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tab = tabControl.SelectedTab;
            if (tab != null && !string.IsNullOrWhiteSpace(tab.Name) && File.Exists(string.Concat(tab.Name,".jc")))
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
        }

        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            //var page = new TabPage();
            //var col = Color.Blue;
            //e.Graphics.FillRectangle(new SolidBrush(col), e.Bounds);

            //var paddedBounds = e.Bounds;
            //int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            //paddedBounds.Offset(1, yOffset);
            //TextRenderer.DrawText(e.Graphics, page.Text, Font, paddedBounds, page.ForeColor);
        }

        #region Buttons

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(62, 62, 64);
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(45, 45, 48);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            SaveProject(this, e);
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            button2.BackColor = Color.FromArgb(62, 62, 64);
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.BackColor = Color.FromArgb(45, 45, 48);
        }

        #endregion
    }
}
