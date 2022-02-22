using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VSDocBar
{
    /// <summary>
    /// called to pick which data template gets used for items in the list
    /// </summary>
    internal class DocListDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var ele = container as FrameworkElement;
            if (ele == null)
                return null;

            // simple choice of project vs. doc here

            if (item is ObservableProject)
            {
                return ele.FindResource("ProjectItemTemplate") as DataTemplate;
            }
            else if (item is ObservableOpenDoc)
            {
                return ele.FindResource("OpenDocItemTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
