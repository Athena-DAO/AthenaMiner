using AthenaMiner.Tasks;
using AthenaMiner.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AthenaMiner
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public ObservableCollection<Container> Containers { get; set; }

        public DashboardPage()
        {
            this.InitializeComponent();
            Containers = new ObservableCollection<Container>();

            new Thread(async () =>
            {
                while(true)
                {
                    StorageFolder storageFolder = KnownFolders.SavedPictures;
                    foreach (var folder in await storageFolder.GetFoldersAsync())
                    {
                        string name = await FileIO.ReadTextAsync(await folder.GetFileAsync("name.txt"));
                        string image = await FileIO.ReadTextAsync(await folder.GetFileAsync("image.txt"));
                        string timestamp = await FileIO.ReadTextAsync(await folder.GetFileAsync("timestamp.txt"));
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            if (Containers.Where(x => x.Name == name).Count() == 0)
                            {
                                Containers.Add(new Container
                                {
                                    Name = name,
                                    Description = image,
                                    TimestapCreated = timestamp
                                });
                            }
                        });
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }
    }
}
