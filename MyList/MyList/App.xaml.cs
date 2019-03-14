using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.Storage;
using SQLitePCL;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MyList {
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        public bool isSuspend = false;
        public static ViewModels.ListItemViewModels ViewModel = new ViewModels.ListItemViewModels();
        public static SQLiteConnection conn;
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                    if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NavigationState"))
                    {
                        rootFrame.SetNavigationState((string)ApplicationData.Current.LocalSettings.Values["NavigationState"]);
                        isSuspend = false;
                    }
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            // Set active window colors
            titleBar.ForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.Black;

            // Set inactive window colors
            titleBar.InactiveForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Black;
            //扩展亚克力
            extendAcrylicIntoTitleBar();

            //处理返回请求
            rootFrame.Navigated += OnNavigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            LoadDatabase();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            isSuspend = true;
            // Get the frame navigation state serialized as a string and save in settings
            Frame frame = Window.Current.Content as Frame;
            // ApplicationData.Current.LocalSettings.Values["NavigationState"] = frame.GetNavigationState();
            deferral.Complete();

        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;

            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        ///将亚克力扩展到标题栏 
        private void extendAcrylicIntoTitleBar()
        {
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
        }

        /// <summary>
        /// 加载数据库
        /// </summary>
        private async void LoadDatabase()
        {
            conn = new SQLiteConnection("mylist.db");
            string sql = @"CREATE TABLE IF NOT EXISTS
                                Items (Id VARCHAR(40) PRIMARY KEY NOT NULL,
                                       Title TEXT,
                                       Description TEXT,
                                       Completed BOOL,
                                       Date STRING,
                                       Image STRING,
                                       Width DOUBLE);";
            using (var statement = conn.Prepare(sql)) {
                statement.Step();
            }

            using (var countStatement = conn.Prepare("SELECT Count(*) FROM Items")) {
                countStatement.Step();
                using (var statement = conn.Prepare("SELECT * FROM Items")) {
                    for (int i = 0; i < (System.Int64)countStatement[0]; i++) {
                        statement.Step();

                        var id = (string)statement[0];
                        var title = (string)statement[1];
                        var description = (string)statement[2];
                        var completed = (System.Int64)statement[3] == 0 ? false : true;
                        var date = DateTimeOffset.Parse((string)statement[4]);
                        var filePath = (string)statement[5];
                        StorageFile file = null;
                        ImageSource image;

                        if (filePath != "") {
                            file = await StorageFile.GetFileFromPathAsync(filePath);
                            IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                            BitmapImage bi = new BitmapImage();
                            await bi.SetSourceAsync(ir);
                            image = bi;
                        }
                        else {
                            BitmapImage bi = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
                            image = bi;
                        }
                        var imageWidth = (double)statement[6];

                        ViewModel.AddListItem(title, description, date, image, imageWidth, file, completed);

                        using (var deleteStatement = conn.Prepare("DELETE FROM Items WHERE Id = ?")) {
                            deleteStatement.Bind(1, id);
                            deleteStatement.Step();
                        }
                    }
                }
            }
        }
    }
}
