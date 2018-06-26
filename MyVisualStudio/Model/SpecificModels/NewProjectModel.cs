using System;
using System.IO;

namespace MyVisualStudio.Model
{
    class NewProjectModel : INewProjectModel
    {
        public bool IsValidPath(string path, string projectName)
        {
            if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(projectName) && Directory.Exists(path))
                return true;
            return false;
        }

        public string GetPath()
        {
            var fileDialog = new System.Windows.Forms.FolderBrowserDialog();
            string path = string.Empty;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                path = fileDialog.SelectedPath;
            return path;
        }

        public string CreateNewProject(string name, string path)
        {
            var newDirectory = Path.Combine(path, name);
            var directoryInfo = Directory.CreateDirectory(newDirectory);
            var textFile = new FileInfo(Path.Combine(newDirectory, string.Concat(name, ".txt")));
            textFile.Create().Close();
            var jcFile = new FileInfo(Path.Combine(newDirectory, string.Concat(name, ".jc")));
            jcFile.Create().Close();
            var result = Path.Combine(newDirectory, name);
            return result;
        }
    }
}

