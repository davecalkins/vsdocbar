using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VSDocBar
{
    /// <summary>
    /// used to determine colors for project/doc items in the list
    /// </summary>
    internal class DocListColorConverter : IValueConverter
    {
        private static Color ColorFromHex(string clr)
        {
            return (Color)ColorConverter.ConvertFromString(clr);
        }

        private Color[] _colors =
        {
            // https://www.schemecolor.com/light-pastels.php
            ColorFromHex("#FBB3BD"),
            ColorFromHex("#FDCEC6"),
            //ColorFromHex("#FCFFE0"),
            ColorFromHex("#97F19E"),
            ColorFromHex("#B39DDC"),
            // https://www.schemecolor.com/pastel-print.php
            ColorFromHex("#C3E6EA"),
            //ColorFromHex("#F7EFDF"),
            ColorFromHex("#E6E0EC"),
            ColorFromHex("#F7EBEC"),
            // https://www.schemecolor.com/beautiful-light-colors.php
            ColorFromHex("#ACDDDE"),
            ColorFromHex("#CAF1DE"),
            ColorFromHex("#E1F8DC"),
            //ColorFromHex("#FEF8DD"),
            ColorFromHex("#FFE7C7"),
            ColorFromHex("#F7D8BA"),
            // https://www.schemecolor.com/slow-walk-in-the-park.php
            ColorFromHex("#689292"),
            ColorFromHex("#A6B894"),
            ColorFromHex("#BCC491"),
            ColorFromHex("#E7E0C1"),
            ColorFromHex("#C2AE88"),
            // https://www.schemecolor.com/simple-soft-pastels.php
            ColorFromHex("#FDD6EA"),
            ColorFromHex("#9CD8EE"),
            ColorFromHex("#F8F1D4"),
            //ColorFromHex("#FFFDF5"),
            ColorFromHex("#F6CFB9"),
            ColorFromHex("#E4B5B2"),
        };

        // http://www.colorsontheweb.com/color-theory/color-contrast
        private Color _activeBackgroundColor = ColorFromHex("#0000B8");
        private Color _activeForegroundColor = ColorFromHex("#F1D700");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Color.FromRgb(255, 0, 255);

            var item = value as ObservableItemBase;
            if (item == null)
                return new SolidColorBrush(color);

            var mode = parameter as string;
            if (mode == null)
                return new SolidColorBrush(color);

            string projectName = "";
            bool isActive = false;
            if (item is ObservableProject proj)
                projectName = proj.ProjectName;
            else if (item is ObservableOpenDoc doc)
            {
                projectName = doc.ProjectName;
                isActive = doc.IsActive;
            }

            // for the active doc, always return the active fg/bg colors
            if (isActive)
            {
                if (mode == "background")
                    color = _activeBackgroundColor;
                else
                    color = _activeForegroundColor;
            }
            else
            {
                // background color is determined by the project name, so that a given project
                // name always gets the same color.
                if (mode == "background")
                {
                    var idx = Math.Abs(projectName.GetHashCode()) % _colors.Length;
                    color = _colors[idx];

                    // for the project headings, darken the color slightly
                    if (item is ObservableProject)
                        color = ColorConversion.ScaleColorBrightness(color, 0.8);
                }
                else
                    // foreground is always black; background colors were chosen to ensure readability with this
                    color = Colors.Black;
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
