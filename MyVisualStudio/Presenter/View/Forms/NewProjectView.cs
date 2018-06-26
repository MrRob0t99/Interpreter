using MyVisualStudio.Presenter;
using MyVisualStudio.View;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyVisualStudio
{
    public partial class NewProjectView : Form, INewProjectView
    {
        public event EventHandler BrouseClick = delegate { };
        public event EventHandler OkClick = delegate { };
        public event EventHandler TextChange = delegate { };
        private bool _pathFocus;
        private bool _projectNameFocus;

        public NewProjectView()
        {
            InitializeComponent();
            var presenter = new NewProjectPresenter(this);
        }

        public bool IsHasUnsavedChanges { get; set; }

        public string ProjectName
        {
            get { return projectName.Text; }
            set { projectName.Text = value; }
        }

        public string Path
        {
            get { return path.Text; }
            set { path.Text = value; }
        }

        public Button OkButton
        {
            get { return button3; }
            set { button3 = value; }
        }

        private void NewProjectView_Load(object sender, EventArgs e)
        {
            LoadDarkTheme();
            button3.Enabled = false;
        }

        private void LoadDarkTheme()
        {
            ForeColor = Color.White;
            BackColor = Color.FromArgb(37, 37, 38);

            //TextBox
            path.BackColor = Color.FromArgb(63, 63, 70);
            projectName.BackColor = Color.FromArgb(63, 63, 70);
            path.ForeColor = Color.White;
            projectName.ForeColor = Color.White;
            path.BorderStyle = BorderStyle.FixedSingle;
            projectName.BorderStyle = BorderStyle.FixedSingle;

            // Button
            button1.FlatStyle = FlatStyle.Flat;
            button2.FlatStyle = FlatStyle.Flat;
            button3.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderColor = Color.FromArgb(85,85,85);
            button2.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 85);
            button3.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 85);
            button1.BackColor = Color.FromArgb(63, 63, 70);
            button2.BackColor = Color.FromArgb(63, 63, 70);
            button3.BackColor = Color.FromArgb(63, 63, 70);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            BrouseClick(this, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OkClick(sender, e);
        }

        private void projectName_TextChanged(object sender, EventArgs e)
        {
            TextChange(this, e);
        }

        private void path_Leave(object sender, EventArgs e)
        {
            _pathFocus = false;
            Refresh();
        }

        private void path_Enter(object sender, EventArgs e)
        {
            _pathFocus = true;
            Refresh();
        }

        private void NewProjectView_Paint(object sender, PaintEventArgs e)
        {
            if (_pathFocus)
            {
                Rectangle rect = new Rectangle(path.Location.X, path.Location.Y, path.ClientSize.Width, path.ClientSize.Height);
                rect.Inflate(1, 1);
                ControlPaint.DrawBorder(e.Graphics, rect, Color.FromArgb(51, 153, 255), ButtonBorderStyle.Solid);
            }
            if (_projectNameFocus)
            {
                Rectangle rect = new Rectangle(projectName.Location.X, projectName.Location.Y, projectName.ClientSize.Width, projectName.ClientSize.Height);
                rect.Inflate(1, 1);
                ControlPaint.DrawBorder(e.Graphics, rect, Color.FromArgb(51, 153, 255), ButtonBorderStyle.Solid);
            }
        }

        private void projectName_Enter(object sender, EventArgs e)
        {
            _projectNameFocus = true;
            Refresh();
        }

        private void projectName_Leave(object sender, EventArgs e)
        {
            _projectNameFocus = false;
            Refresh();
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 85);
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.FlatAppearance.BorderColor = Color.FromArgb(51, 153, 255);
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            button2.FlatAppearance.BorderColor = Color.FromArgb(51, 153, 255);
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 85);
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            button3.FlatAppearance.BorderColor = Color.FromArgb(51, 153, 255);
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            button3.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 85);
        }
    }
}
