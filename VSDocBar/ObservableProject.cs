using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VSDocBar
{
    /// <summary>
    /// represents a project heading in the list
    /// </summary>
    internal class ObservableProject : ObservableItemBase, IEquatable<ObservableProject>, IComparable<ObservableProject>
    {
        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set { SetProperty(ref _projectName, value); }
        }

        public SolidColorBrush BackgroundColor => (SolidColorBrush)_colorConverter.Convert(this, typeof(SolidColorBrush), "background", null);
        public SolidColorBrush ForegroundColor => (SolidColorBrush)_colorConverter.Convert(this, typeof(SolidColorBrush), "foreground", null);
        public FontWeight TextFontWeight => (FontWeight)_fontWeightConverter.Convert(this, typeof(FontWeight), null, null);

        #region equatable, comparable

        public bool Equals(ObservableProject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _projectName == other._projectName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObservableProject) obj);
        }

        public override int GetHashCode()
        {
            return (_projectName != null ? _projectName.GetHashCode() : 0);
        }

        public int CompareTo(ObservableProject other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(_projectName, other._projectName, StringComparison.Ordinal);
        }

        public override int Compare(ObservableItemBase other)
        {
            if (other is ObservableProject otherProj)
                return CompareTo(otherProj);
            else
                return 1;
        }

        #endregion
    }
}
