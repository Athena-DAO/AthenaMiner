using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace AthenaMiner.Tasks
{
    class DockerUpdateTask 
    {
        public static async Task UpdateList()
        {
            StorageFolder folder = KnownFolders.SavedPictures;
            foreach(var file in await folder.GetFoldersAsync())
            {
                DashboardPage page = new DashboardPage();
                page.Containers.Add(new ViewModels.Container
                {
                    Name = "New",
                    Description = "New",
                    TimestapCreated = "New"
                });
            }
        }
    }
}
