namespace MyVisualStudio.Model
{
    public interface INewProjectModel
    {
        string GetPath();

        string CreateNewProject(string name, string path);

        bool IsValidPath(string path, string projectName);
    }
}
