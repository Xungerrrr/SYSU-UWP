using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyList.Models
{
    public class ListItem {
        public string id;
        public string title { get; set; }
        public string description { get; set; }
        public bool completed {
            get {
                return isCompleted;
            }
            set {
                isCompleted = value;
                var db = App.conn;
                using (var statement = db.Prepare("UPDATE Items SET Completed = ? WHERE Id = ?")) {
                    statement.Bind(1, value ? 1 : 0);
                    statement.Bind(2, this.id);
                    statement.Step();
                }
            }
        }
        public DateTimeOffset date { get; set; }
        public ImageSource image { get; set; }
        public double imageWidth { get; set; }
        public StorageFile file { get; set; }
        private bool isCompleted;

        public ListItem(string title, string description, DateTimeOffset date, ImageSource image, double imageWidth, StorageFile file, bool completed)
        {
            this.id = Guid.NewGuid().ToString(); //生成id
            this.title = title;
            this.description = description;
            this.completed = completed;
            this.date = date;
            this.image = image;
            this.imageWidth = imageWidth;
            this.file = file;
        }
    }
}
