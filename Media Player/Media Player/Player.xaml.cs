using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Media_Player {
    /// <summary>
    /// 媒体播放页。
    /// </summary>
    public sealed partial class Player : Page {

        private MediaPlayer mediaPlayer = App.mediaPlayer;
        private MediaTimelineController mediaTimelineController = App.mediaTimelineController;
        private TimeSpan duration;

        public Player() {
            this.InitializeComponent();
            media.DoubleTapped += _mediaPlayerElement_DoubleTapped;
            if (mediaTimelineController.State == MediaTimelineControllerState.Running) {
                playButton.Icon = new SymbolIcon(Symbol.Pause);
            }
            else {
                playButton.Icon = new SymbolIcon(Symbol.Play);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            media.SetMediaPlayer(mediaPlayer);
        }

        /// <summary>
        /// 控制媒体播放暂停。
        /// </summary>
        private void PlayButtonClicked(object sender, RoutedEventArgs e) {
            if (mediaTimelineController.State == MediaTimelineControllerState.Running) {
                mediaTimelineController.Pause();
                playButton.Icon = new SymbolIcon(Symbol.Play);
                spin.Pause();
            }
            else {
                mediaTimelineController.Resume();
                playButton.Icon = new SymbolIcon(Symbol.Pause);
                if (spin.GetCurrentState() == Windows.UI.Xaml.Media.Animation.ClockState.Stopped)
                    spin.Begin();
                else
                    spin.Resume();
            }
        }

        /// <summary>
        /// 控制媒体停止。
        /// </summary>
        private void StopButtonClicked(object sender, RoutedEventArgs e) {
            mediaTimelineController.Pause();
            slider.Value = 0;
            playButton.Icon = new SymbolIcon(Symbol.Play);
            spin.Stop();
        }

        private void PreviousButtonClicked(object sender, RoutedEventArgs e) {
            slider.Value = 0;
        }

        private void NextButtonClicked(object sender, RoutedEventArgs e) {
            slider.Value = 0;
        }

        /// <summary>
        /// 打开本地视频文件。
        /// </summary>
        private async void LoadVideoButtonClicked(object sender, RoutedEventArgs e) {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".avi");
            openPicker.FileTypeFilter.Add(".mov");
            openPicker.FileTypeFilter.Add(".mkv");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null) {
                // 设置MediaPlayer和MediaTimelineController
                mediaPlayer.TimelineController = null;
                var mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
                mediaPlayer.Source = mediaSource;
                media.SetMediaPlayer(mediaPlayer);
                media.Visibility = Visibility.Visible;
                mediaPlayer.CommandManager.IsEnabled = false;
                mediaPlayer.TimelineController = mediaTimelineController;
                mediaTimelineController.PositionChanged += MediaTimelineController_PositionChanged;
                playButton.Icon = new SymbolIcon(Symbol.Pause);
                // 读取视频信息
                VideoProperties videoProperties = await file.Properties.GetVideoPropertiesAsync();
                var title = videoProperties.Title.Equals("") ? file.DisplayName : videoProperties.Title;
                mediaName.Text = title;
                StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 500);
                // 设置背景及磁贴
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image) {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    albumArt.ImageSource = bitmapImage;
                    albumArtSmall.Source = bitmapImage;
                }
                var background = new BitmapImage(new Uri("ms-appx:///Assets/Mint Drizzle.png"));
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = background;
                imageBrush.Stretch = Stretch.UniformToFill;
                bottom.Background = imageBrush;
                spin.Stop();
                TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
            }
        }

        /// <summary>
        /// 打开本地音频文件。
        /// </summary>
        private async void LoadMusicButtonClicked(object sender, RoutedEventArgs e) {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;

            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".wav");
            openPicker.FileTypeFilter.Add(".flac");
            openPicker.FileTypeFilter.Add(".wma");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null) {
                // 设置MediaPlayer和MediaTimelineController
                mediaPlayer.TimelineController = null;
                var mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
                mediaPlayer.Source = mediaSource;
                media.Visibility = Visibility.Collapsed;
                mediaPlayer.CommandManager.IsEnabled = false;
                mediaPlayer.TimelineController = mediaTimelineController;
                mediaTimelineController.PositionChanged += MediaTimelineController_PositionChanged;
                playButton.Icon = new SymbolIcon(Symbol.Pause);
                // 读取音频信息
                MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                var title = musicProperties.Title.Equals("") ? file.DisplayName : musicProperties.Title;
                var album = musicProperties.Album.Equals("") ? "未知" : musicProperties.Album;
                var artist = musicProperties.Artist.Equals("") ? "未知" : musicProperties.Artist;
                mediaName.Text = title + " - " + artist;
                songTitle.Text = title;
                songAlbum.Text = "专辑：" + album;
                songArtist.Text = "歌手：" + artist;
                StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 300);
                // 设置背景及磁贴
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image) {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    albumArt.ImageSource = bitmapImage;
                    albumArtSmall.Source = bitmapImage;

                    var imageBrush = new ImageBrush();
                    imageBrush.ImageSource = bitmapImage;
                    imageBrush.Stretch = Stretch.UniformToFill;
                    bottom.Background = imageBrush;
                }
                else {
                    var bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/album.png"));
                    albumArt.ImageSource = bitmapImage;
                    albumArtSmall.Source = bitmapImage;
                    var background = new BitmapImage(new Uri("ms-appx:///Assets/Mint Drizzle.png"));
                    var imageBrush = new ImageBrush();
                    imageBrush.ImageSource = background;
                    imageBrush.Stretch = Stretch.UniformToFill;
                    bottom.Background = imageBrush;
                }
                spin.Stop();
                spin.Begin();
                Update(title, artist);
            }
        }

        /// <summary>
        /// 按钮控制全屏。
        /// </summary>
        private void FullScreenButtonClicked(object sender, RoutedEventArgs e) {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode) {
                view.ExitFullScreenMode();
            }
            else {
                view.TryEnterFullScreenMode();
            }
        }

        /// <summary>
        /// 双击控制全屏。
        /// </summary>
        private void _mediaPlayerElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode) {
                view.ExitFullScreenMode();
            }
            else {
                view.TryEnterFullScreenMode();
            }
        }

        /// <summary>
        /// 完成媒体加载并初始化进度条。
        /// </summary>
        private async void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args) {
            duration = sender.Duration.GetValueOrDefault();
            mediaTimelineController.Start();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                slider.Minimum = 0;
                slider.Maximum = duration.TotalSeconds;
                slider.StepFrequency = 1;
            });
        }

        /// <summary>
        /// 将媒体进度与进度条绑定。
        /// </summary>
        private async void MediaTimelineController_PositionChanged(MediaTimelineController sender, object args) {
            if (duration != TimeSpan.Zero) {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    slider.Value = sender.Position.TotalSeconds;
                    time.Text = sender.Position.ToString(@"mm\:ss") + " / " +  duration.ToString(@"mm\:ss");
                    if (slider.Value == slider.Maximum) {
                        // 播放完毕自动停止
                        mediaTimelineController.Pause();
                        slider.Value = 0;
                        playButton.Icon = new SymbolIcon(Symbol.Play);
                        spin.Stop();
                    }
                    
                });
            }
        }

        /// <summary>
        /// 更新磁贴
        /// </summary>
        private void Update(string title, string artist) {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Clear();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText("Tile.xml"));
            XmlNodeList texts = xml.GetElementsByTagName("text");
            foreach (IXmlNode text in texts) {
                if (text.InnerText == "Title")
                    text.InnerText = title;
                if (text.InnerText == "Artist")
                    text.InnerText = artist;
            }
            var notification = new TileNotification(xml);
            updater.EnableNotificationQueue(true);
            updater.Update(notification);
        }
    }
}
