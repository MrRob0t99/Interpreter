using System;
using System.Windows.Forms;

namespace MyVisualStudio.View
{
    public interface INewProjectView
    {
        bool IsHasUnsavedChanges { get; set; }

        string ProjectName { get; set; }

        string Path { get; set; }

        Button OkButton { get; set; }

        event EventHandler BrouseClick;

        event EventHandler OkClick;

        event EventHandler TextChange;
    }
}
