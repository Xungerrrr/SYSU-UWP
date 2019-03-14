using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Player.ViewModels
{
    /// <summary>
    /// TimeSpan与double转换。
    /// </summary>
    class SliderConverter : Windows.UI.Xaml.Data.IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}
