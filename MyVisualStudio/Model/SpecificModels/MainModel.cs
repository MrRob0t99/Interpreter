using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyVisualStudio.Model
{
    class MainModel : IMainModel
    {
        public void SaveProject(string code, string path)
        {
            using (var streamWriter = new StreamWriter(path))
            {
                streamWriter.WriteAsync(code).GetAwaiter().GetResult();
            }
        }

        public (string, string, string) OpenProject()
        {
            string code = string.Empty;
            var fileDialog = new System.Windows.Forms.FolderBrowserDialog();
            string path = string.Empty;
            string projectName = string.Empty;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                path = fileDialog.SelectedPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                projectName = path.Substring(path.LastIndexOf(@"\"));

                using (var streamReader = new StreamReader(string.Concat(path, projectName + ".txt")))
                {
                    code = streamReader.ReadToEnd();
                }
            }
            return (path, projectName, code);
        }

        public async Task<IEnumerable<News>> ParseAsync()
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = await htmlWeb.LoadFromWebAsync("https://dou.ua/");
            var divArticles = htmlDocument.DocumentNode.SelectNodes("//div")
               .Where(x => x.GetClasses()
               .ToList()
               .Any(c => c.Contains("b-articles")))
               .FirstOrDefault().ChildNodes.FirstOrDefault(x => x.Name == "ul" && x.GetClasses().ToList().Any(c => c.Contains("l-articles")))
               .ChildNodes.Where(n => n.Name == "li")
               .ToList()
               .Select(n => n.ChildNodes.FirstOrDefault(a => a.Name == "a").GetAttributeValue("href", string.Empty));
            var _news = new List<News>();
            foreach (var item in divArticles)
            {
                htmlDocument = await htmlWeb.LoadFromWebAsync(item);
                var article = new News();
                article.Url = item;
                article.Title = htmlDocument.DocumentNode.SelectNodes("//h1").FirstOrDefault().InnerText;
                article.Content = htmlDocument.DocumentNode.SelectNodes("//article").FirstOrDefault().ChildNodes.FirstOrDefault(n => n.Name == "p").InnerText;
                _news.Add(article);
            }
            return _news;
        }
    }
}
