using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace VSDocBar
{
    /// <summary>
    /// used to pick the font weight for a given item in the list
    /// </summary>
    internal class DocListFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableOpenDoc doc)
            {
                // active doc is bold
                if (doc.IsActive)
                    return FontWeights.Bold;
                // all other docs are normal
                else
                    return FontWeights.Normal;
            }
            // project headings are always bold
            else if (value is ObservableProject proj)
            {
                return FontWeights.Bold;
            }
            // unexpected usage, just return normal
            else
            {
                return FontWeights.Normal;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
