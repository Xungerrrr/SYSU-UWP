using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.DataTransfer;
using SQLitePCL;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public ViewModels.ListItemViewModels ViewModel;
        public Models.ListItem Item;
        public string filePath;
        public StorageFile file;
        public MainPage()
        {
            ViewModel = App.ViewModel;
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).isSuspend;
            if (suspending)
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["title"] = title.Text;
                composite["detail"] = detail.Text;
                composite["date"] = date.Date;
                composite["slider"] = slider.Value;
                composite["file"] = filePath;
                ApplicationData.Current.LocalSettings.Values["mainpage"] = composite;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {

                // If this is a new navigation, this is a fresh launch so we can
                // discard any saved state
                ApplicationData.Current.LocalSettings.Values.Remove("mainpage");
            }
            else
            {
                // Try too restore state if any, in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("newpage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["mainpage"] as ApplicationDataCompositeValue;
                    title.Text = (string)composite["title"];
                    detail.Text = (string)composite["detail"];
                    date.Date = (DateTimeOffset)composite["date"];
                    slider.Value = (double)composite["slider"];
                    if (filePath != "")
                    {
                        StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync((string)ApplicationData.Current.LocalSettings.Values["image"]);
                        IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                        BitmapImage bi = new BitmapImage();
                        await bi.SetSourceAsync(ir);
                        image.Source = bi;
                    }
                    else
                    {
                        BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
                        image.Source = bi;
                    }
                    // We're done with it, so remove it
                    ApplicationData.Current.LocalSettings.Values.Remove("mainpage");
                }
            }
        }
        /// <summary>
        /// 点击增加按钮触发的新建Item函数
        /// </summary>
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(Window.Current.Bounds.Width < 800)
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(NewPage));
            }
            else
            {
                title.Text = "";
                detail.Text = "";
                date.Date = DateTimeOffset.Now;
                create.Content = "Create";
                Item = null;
                BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
                image.Source = bi;
                DeleteAppBarButton.Visibility = Visibility.Collapsed;
                slider.Value = 280;
                filePath = "";
            }
        }

        /// <summary>
        /// 点击Item触发的跳转函数
        /// </summary>
        private void ListItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            Item = (Models.ListItem)e.ClickedItem;
            if (Window.Current.Bounds.Width < 800)
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(NewPage), Item);
            }
            else
            {
                title.Text = Item.title;
                detail.Text = Item.description;
                date.Date = Item.date;
                image.Source = Item.image;
                create.Content = "Update";
                slider.Value = Item.imageWidth;
                DeleteAppBarButton.Visibility = Visibility.Visible;
                file = Item.file;
            }
        }

        /// <summary>
        /// 点击Cancel按钮触发的清除函数
        /// </summary>
        private void Cancel(object sender, RoutedEventArgs e)
        {
            if (Item == null) // 如果Item不存在，则清空内容
            {
                title.Text = "";
                detail.Text = "";
                date.Date = DateTimeOffset.Now;
                BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
                image.Source = bi;
                slider.Value = 280;
                filePath = "";
            }
            else //Item存在则还原
            {
                title.Text = Item.title;
                detail.Text = Item.description;
                date.Date = Item.date;
                image.Source = Item.image;
                slider.Value = Item.imageWidth;
            }

        }

        /// <summary>
        /// 点击Create或Update按钮触发的更新函数
        /// </summary>
        private async void Create(object sender, RoutedEventArgs e)
        {
            // 判断控件状态合法性
            if (title.Text.Trim() == "" && detail.Text.Trim() == "")
            {
                ContentDialog empty = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Title and detail cannot be empty.",
                    CloseButtonText = "OK"
                };

                await empty.ShowAsync();
                return;
            }

            if (title.Text.Trim() == "")
            {
                ContentDialog empty = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Title cannot be empty.",
                    CloseButtonText = "OK"
                };

                await empty.ShowAsync();
                return;
            }

            if (detail.Text.Trim() == "")
            {
                ContentDialog empty = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Detail cannot be empty.",
                    CloseButtonText = "OK"
                };

                await empty.ShowAsync();
                return;
            }

            if ((date.Date - DateTimeOffset.Now).Days < 0)
            {
                ContentDialog empty = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Due date cannot be before the current date.",
                    CloseButtonText = "OK"
                };

                await empty.ShowAsync();
                return;
            }

            // 新建Item
            if ((string)create.Content == "Create")
            {
                ContentDialog success = new ContentDialog()
                {
                    Title = "Success",
                    Content = "Create to-do item successfully.",
                    CloseButtonText = "OK"
                };
                await success.ShowAsync();
                ViewModel.AddListItem(title.Text, detail.Text, date.Date, image.Source, slider.Value, file, false);
                title.Text = "";
                detail.Text = "";
                date.Date = DateTimeOffset.Now;
                create.Content = "Create";
                Item = null;
                BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
                image.Source = bi;
                slider.Value = 280;
            }

            // 更新Item
            else
            {
                ContentDialog success = new ContentDialog()
                {
                    Title = "Success",
                    Content = "Update to-do item successfully.",
                    CloseButtonText = "Ok"
                };
                await success.ShowAsync();
                ViewModel.UpdateListItem(Item, title.Text, detail.Text, date.Date, image.Source, slider.Value, file, Item.completed);
                title.Text = "";
                detail.Text = "";
                date.Date = DateTimeOffset.Now;
                create.Content = "Create";
                Item = null;
                BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
                image.Source = bi;
                DeleteAppBarButton.Visibility = Visibility.Collapsed;
                slider.Value = 280;
                filePath = "";
            }
            Update();
        }

        /// <summary>
        /// 点击删除按钮触发的删除函数
        /// </summary>
        private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog delete = new ContentDialog()
            {
                Title = "Delete Item",
                Content = "Are you sure to delete this to-do item?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };

            ContentDialogResult result = await delete.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.RemoveListItem(Item);
                title.Text = "";
                detail.Text = "";
                date.Date = DateTimeOffset.Now;
                create.Content = "Create";
                Item = null;
                BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
                image.Source = bi;
                DeleteAppBarButton.Visibility = Visibility.Collapsed;
                slider.Value = 280;
                filePath = "";
            }
            Update();
        }

        /// <summary>
        /// 选取图片的函数
        /// </summary>
        private async void SelectImage(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                ApplicationData.Current.LocalSettings.Values["image"] = StorageApplicationPermissions.FutureAccessList.Add(file);
                filePath = file.Path;
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                image.Source = bi;
            }
        }

        /// <summary>
        /// 更新磁贴
        /// </summary>
        private void Update()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Clear();
            foreach (Models.ListItem item in ViewModel.AllItems)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(File.ReadAllText("Tile.xml"));
                XmlNodeList texts = xml.GetElementsByTagName("text");
                XmlNodeList images = xml.GetElementsByTagName("image");
                foreach (IXmlNode text in texts)
                {
                    if (text.InnerText == "Title")
                        text.InnerText = item.title;
                    if (text.InnerText == "Detail")
                        text.InnerText = item.description;
                }
                foreach (IXmlNode image in images)
                {
                    XmlElement imageEle = (XmlElement)image;
                    if (item.file != null)
                        imageEle.SetAttribute("src", item.file.Path);
                    else
                        imageEle.SetAttribute("src", "Assets/background.jpg");
                }
                
                var notification = new TileNotification(xml);
                updater.EnableNotificationQueue(true);
                updater.Update(notification);
            }
        }

        /// <summary>
        /// 点击设置按钮触发的函数
        /// </summary>
        private void Setting_Clicked(object sender, RoutedEventArgs e)
        {
            var data = (sender as FrameworkElement).DataContext;
            var item = ItemListView.ContainerFromItem(data) as ListViewItem;
            Item = item.Content as Models.ListItem;
        }


        private void Share(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        /// <summary>
        /// 分享事件处理
        /// </summary>
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.SetText(Item.description);
            if (Item.file != null)
            {
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(Item.file));
            }
            else
            {
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/background.jpg")));
            }
            request.Data.Properties.Title = Item.title;
            request.Data.Properties.Description = Item.description;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        private async void Search(object sender, RoutedEventArgs e) {
            var db = App.conn;
            string content = searchBox.Text.Trim();
            if (content != "") {
                StringBuilder result = new StringBuilder("");
                string searchSQL = @"SELECT * FROM Items WHERE Title LIKE '%" + content + "%' " +
                                                               "OR Description LIKE '%" + content + "%' " +
                                                               "OR Date LIKE '%" + content + "%'";
                using (var statement = db.Prepare(searchSQL)) {
                    while (SQLiteResult.DONE != statement.Step()) {
                        var title = (string)statement[1];
                        var description = (string)statement[2];
                        var date = (string)statement[4];
                        string info = "Title: " + title + " Detail: " + description + " Due Date: " + date;
                        result.AppendLine(info);
                    }
                }
                if (result.Length == 0) {
                    result.Append("No matched item.");
                }
                ContentDialog search = new ContentDialog() {
                    Title = "Search Result",
                    Content = result,
                    CloseButtonText = "Close"
                };
                await search.ShowAsync();
            }
        }
    }
}
