using FastColoredTextBoxNS;
using HtmlAgilityPack;
using MyParsr;
using MyVisualStudio.Model;
using MyVisualStudio.View.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyVisualStudio.Presenter
{
    public class MainPresenter
    {
        private readonly IMainView _mainView;
        private readonly IMainModel _mainModel;
        private readonly string _code;
        private readonly int _consoleHide;
        private readonly int _consoleShow;
        private IEnumerable<News> _news;
        private readonly Style _greenStyle;
        private readonly Style _functionStyle;
        private readonly Style _stringStyle;
        private readonly Style _consoleStyle;
        private string _path;


        public MainPresenter(IMainView mainView)
        {
            _greenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
            _functionStyle = new TextStyle(Brushes.CornflowerBlue, null, FontStyle.Regular);
            _stringStyle = new TextStyle(Brushes.Orange, null, FontStyle.Regular);
            _consoleStyle = new TextStyle(Brushes.Violet, null, FontStyle.Regular);

            _mainView = mainView;
            _mainModel = new MainModel();
            _code = string.Empty;
            _consoleHide = 0;
            _consoleShow = 1;

            _mainView.NewStripMenuItemClick += NewProject;
            _mainView.OpenStripMenuItemClick += OpenProject;
            _mainView.RunProject += RunProject;
            _mainView.FormLoad += FormLoadAsync;
            _mainView.SaveProject += SaveProject;
        }

        public async void FormLoadAsync(object sender, EventArgs e)
        {
            //_news = await _mainModel.ParseAsync();
            //Draw();
        }

        private void SaveProject(object sender, EventArgs e)
        {
            string code = string.Empty;
            var path = string.Concat(_mainView.TabControl.SelectedTab.Name, ".txt");
            _path = path;
            var rtb = _mainView.TabControl
                  .SelectedTab.Controls
                  .Cast<Control>()
                  .FirstOrDefault(x => x is FastColoredTextBox);
            if (rtb != null)
                code = rtb.Text;
            _mainModel.SaveProject(code, path);
        }

        private void Draw()
        {
        //    var tab = _mainView.TabControl.SelectedTab.Controls.Cast<Control>().Where(x => x is Panel).ToList().FirstOrDefault(p => p.Name == "UnderPanel");
        //    var innerPanel = tab.Controls
        //         .Cast<Control>()
        //         .Where(x => x is Panel).ToList().FirstOrDefault(p => p.Name == "InnerPanel");

        //    if (innerPanel != null)
        //    {
        //        foreach (var item in _news)
        //        {
        //            var panel = new Panel();
        //            var textBox = new TextBox();
        //            var linkLabel = new LinkLabel();
        //            linkLabel.Text = item.Title;
        //            linkLabel.Click += UrlClick;
        //            linkLabel.Dock = DockStyle.Top;
        //            linkLabel.LinkColor = Color.FromArgb(123, 189, 234);
        //            textBox.Dock = DockStyle.Top;
        //            textBox.Multiline = true;
        //            textBox.WordWrap = true;
        //            textBox.Height = 100;
        //            panel.Height = 150;
        //            panel.Dock = DockStyle.Top;
        //            panel.BackColor = Color.FromArgb(45, 45, 48);
        //            textBox.BackColor = Color.FromArgb(45, 45, 48);
        //            textBox.ForeColor = Color.FromArgb(241, 241, 241);
        //            textBox.BorderStyle = BorderStyle.None;
        //            var text = item.Content;
        //            text = text.Trim();
        //            text = text.Replace("  ", "");
        //            textBox.Text = text;
        //            panel.Controls.Add(textBox);
        //            panel.Controls.Add(linkLabel);
        //            textBox.Enabled = false;
        //            innerPanel.Controls.Add(panel);
        //        }
        //    }
        }

        private void UrlClick(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("http://www.microsoft.com");
        }

        public void NewProject(object sender, EventArgs e)
        {
            var newProjectView = new NewProjectView();
            var dialogResult = newProjectView.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                var path = Path.Combine(newProjectView.Path, newProjectView.ProjectName, string.Concat(newProjectView.ProjectName));
                AddNewTabPage(path, newProjectView.ProjectName);
            }
        }

        public void OpenProject(object sender, EventArgs e)
        {
            var result = _mainModel.OpenProject();
            if (!string.IsNullOrWhiteSpace(result.Item1) && !string.IsNullOrWhiteSpace(result.Item2) && !string.IsNullOrWhiteSpace(result.Item3))
            {
                AddNewTabPage(string.Concat(result.Item1, result.Item2), result.Item2);

                var rtb = _mainView.TabControl
                   .SelectedTab.Controls
                   .Cast<Control>()
                   .FirstOrDefault(x => x is FastColoredTextBox);
                if (rtb != null)
                    rtb.Text = result.Item3;
            }
        }

        private void AddNewTabPage(string fullPath, string projectName)
        {
            var tabPage = new TabPage();
            tabPage.Text = projectName.Substring(projectName.LastIndexOf('\\') + 1);
            tabPage.ForeColor = Color.Red;
            tabPage.BorderStyle = BorderStyle.FixedSingle;
            tabPage.Name = fullPath;
            var textBox = new FastColoredTextBox();
            textBox.BackColor = Color.FromArgb(37, 37, 38);
            textBox.Dock = DockStyle.Fill;
            textBox.ForeColor = Color.White;
            textBox.Font = new Font("Consolas", 10);
            textBox.LineNumberColor = Color.FromArgb(43, 145, 175);
            textBox.SelectionColor = Color.FromArgb(43, 145, 175);
            textBox.ReservedCountOfLineNumberChars = 3;
            textBox.IndentBackColor = Color.FromArgb(30, 30, 30);
            textBox.AutoCompleteBrackets = true;
            tabPage.Controls.Add(textBox);
            textBox.TextChanged += ChangedTextBox;
            _mainView.TabControl.TabPages.Add(tabPage);
            _mainView.TabControl.SelectedTab = tabPage;
        }

        public void ChangedTextBox(object sender, TextChangedEventArgs e)
        {
            e.ChangedRange.ClearStyle(_greenStyle);
            e.ChangedRange.ClearStyle(_functionStyle);
            e.ChangedRange.SetStyle(_greenStyle, @"//.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(_functionStyle, @"\b(function)\s");
            e.ChangedRange.SetStyle(_stringStyle, "\\\"(.*?)\\\"");
            e.ChangedRange.SetStyle(_functionStyle, @"\b(return)\s");
            e.ChangedRange.SetStyle(_functionStyle, @"\b(else)\s");
            e.ChangedRange.SetStyle(_functionStyle, @"\b(if)\s");
            e.ChangedRange.SetStyle(_functionStyle, @"\b(elseif)\s");
            e.ChangedRange.SetStyle(_consoleStyle, @"\b(Print)\s");
            //e.ChangedRange.SetStyle(ConsoleStyle, @"PrintLine");
            //e.ChangedRange.SetStyle(ConsoleStyle, @"\b(InputString)\s");

        }

        public void BuildProject()
        {
            Control richTextBox = null;
            if (_mainView.TabControl.TabCount > 0)
                richTextBox = _mainView.TabControl
                .SelectedTab.Controls
                .Cast<Control>()
                .FirstOrDefault(x => x is FastColoredTextBox);

            if (richTextBox != null)
            {
                var path = _mainView.TabControl.SelectedTab.Name;
            }
        }

        public void RunProject(object sender, EventArgs e)
        {
            var handle = GetConsoleWindow();
            try
            {
                ShowWindow(handle, _consoleShow);
                SaveProject(sender, e);
                Interpreter interpreter = new Interpreter();
                interpreter.LoadFile(_path);
                interpreter.RunFile();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                ShowWindow(handle, _consoleHide);
                Console.Clear();
            }
        }


        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();


        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
