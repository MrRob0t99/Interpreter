using System;
using System.Windows.Forms;

namespace MyVisualStudio.View.Interfaces
{
    public interface IMainView
    {
        event EventHandler NewStripMenuItemClick;
        event EventHandler OpenStripMenuItemClick;
        event EventHandler RunProject;
        event EventHandler SaveProject;
        event EventHandler FormLoad;

        TabControl TabControl { get; set; }
    }
}
