using MyVisualStudio.Model;
using MyVisualStudio.View;
using System;

namespace MyVisualStudio.Presenter
{
    class NewProjectPresenter
    {
        private readonly INewProjectModel _newProjectModel;
        private readonly INewProjectView _newProjectView;

        public NewProjectPresenter(INewProjectView newProjectView)
        {
            _newProjectModel = new NewProjectModel();
            _newProjectView = newProjectView;
            _newProjectView.BrouseClick += GetPath;
            _newProjectView.OkClick += OnClick;
            _newProjectView.TextChange += TextChange;
        }

        public void GetPath(object sender, EventArgs e)
        {
            var path = _newProjectModel.GetPath();
            if (path != null && !string.IsNullOrWhiteSpace(path))
                _newProjectView.Path = path;
        }

        public void OnClick(object sender, EventArgs e)
        {
            var name = _newProjectView.ProjectName;
            var path = _newProjectView.Path;
            var result = _newProjectModel.CreateNewProject(name, path);
        }

        public void TextChange(object sender,EventArgs e)
        {
            var isValid = _newProjectModel.IsValidPath(_newProjectView.Path, _newProjectView.ProjectName);
            _newProjectView.OkButton.Enabled = isValid;
        }
    }
}
