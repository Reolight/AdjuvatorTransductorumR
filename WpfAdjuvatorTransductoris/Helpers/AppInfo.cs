using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace WpfAdjuvatorTransductoris.Helpers;

public class AppInfo
{
    private static AppInfo? _instance = null;
    
    public ObservableCollection<ProjectInfo> ProjectInfos { get; private set; } = new();

    public class ProjectInfo
    {
        public string Name { get; set; } = string.Empty;
        public string LastEdit { get; set; } = string.Empty;
        public string CreatedTime { get; set; } = string.Empty;
    }

    private AppInfo()
    {
        DirectoryInfo dir = new DirectoryInfo("Project");
        if (!dir.Exists)
        {
            dir.Create();
            return;
        }

        foreach (FileInfo file in dir.GetFiles().Where(f => f.Extension == ".xml"))
        {
            ProjectInfos.Add(new ProjectInfo
            {
                Name = file.Name.Split('.')[0],
                LastEdit = file.LastWriteTime.ToShortDateString(),
                CreatedTime = file.CreationTime.ToShortDateString()
            });
        }
    }

    public static AppInfo GetInstance()
    {
        _instance ??= new AppInfo();
        return _instance;
    }
}