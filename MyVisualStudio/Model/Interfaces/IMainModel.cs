using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyVisualStudio.Model
{
    interface IMainModel
    {
        (string, string, string) OpenProject();

        void SaveProject(string code, string path);

        Task<IEnumerable<News>> ParseAsync();
    }
}
