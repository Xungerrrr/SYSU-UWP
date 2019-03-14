using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyList.ViewModels
{
    public class ListItemViewModels
    {
        private ObservableCollection<Models.ListItem> allItems =
            new ObservableCollection<Models.ListItem>();
        public ObservableCollection<Models.ListItem> AllItems { get { return this.allItems; } }
        public void AddListItem(string title, string description, DateTimeOffset date, ImageSource image, double imageWidth, StorageFile file, bool completed) {
            Models.ListItem newItem = new Models.ListItem(title, description, date, image, imageWidth, file, completed);
            this.allItems.Add(newItem);
            var db = App.conn;
            using (var statement = db.Prepare("INSERT INTO Items (Id, Title, Description, Completed, Date, Image, Width) VALUES (?, ?, ?, ?, ?, ?, ?)")) {
                statement.Bind(1, newItem.id);
                statement.Bind(2, newItem.title);
                statement.Bind(3, newItem.description);
                statement.Bind(4, newItem.completed ? 1 : 0);
                statement.Bind(5, newItem.date.ToString("yyyy-MM-dd"));
                statement.Bind(6, newItem.file == null ? "" : newItem.file.Path);
                statement.Bind(7, newItem.imageWidth);
                statement.Step();
            }
        }
        public void RemoveListItem(Models.ListItem Item)
        {
            this.allItems.Remove(Item);
            var db = App.conn;
            using (var statement = db.Prepare("DELETE FROM Items WHERE Id = ?")) {
                statement.Bind(1, Item.id);
                statement.Step();
            }
        }
        public void UpdateListItem(Models.ListItem Item, string title, string description, DateTimeOffset date, ImageSource image, double imageWidth, StorageFile file, bool completed)
        {
            Models.ListItem newItem = new Models.ListItem(title, description, date, image, imageWidth, file, completed);
            this.allItems.Insert(this.allItems.IndexOf(Item), newItem);
            this.allItems.Remove(Item);
            var db = App.conn;
            using (var statement = db.Prepare("UPDATE Items SET Id = ?, Title = ?, Description = ?, Completed = ?, Date = ?, Image = ?, Width = ? WHERE Id = ?")) {
                statement.Bind(1, newItem.id);
                statement.Bind(2, newItem.title);
                statement.Bind(3, newItem.description);
                statement.Bind(4, newItem.completed ? 1 : 0);
                statement.Bind(5, newItem.date.ToString("yyyy-MM-dd"));
                statement.Bind(6, newItem.file == null ? "" : newItem.file.Path);
                statement.Bind(7, newItem.imageWidth);
                statement.Bind(8, Item.id);
                statement.Step();
            }
        }
    }

    public class CheckBoxConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            System.Boolean completed = (System.Boolean)value;
            System.Nullable<System.Boolean> ischecked = completed;
            return ischecked;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            System.Nullable<System.Boolean> ischecked = (System.Nullable<System.Boolean>)value;
            System.Boolean completed = (bool)ischecked;
            return completed;
        }
    }
}
