using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VSDocBar
{
    /// <summary>
    /// represents an open doc in the list
    /// </summary>
    internal class ObservableOpenDoc : ObservableItemBase, IEquatable<ObservableOpenDoc>, IComparable<ObservableOpenDoc>, IComparable
    {
        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set { SetProperty(ref _projectName, value); }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public SolidColorBrush BackgroundColor => (SolidColorBrush)_colorConverter.Convert(this, typeof(SolidColorBrush), "background", null);
        public SolidColorBrush ForegroundColor => (SolidColorBrush)_colorConverter.Convert(this, typeof(SolidColorBrush), "foreground", null);
        public FontWeight TextFontWeight => (FontWeight)_fontWeightConverter.Convert(this, typeof(FontWeight), null, null);

        public override string ToString()
        {
            return (ProjectName ?? "null") + ": " + (FileName ?? "null") + ", IsActive=" + IsActive;
        }

        public DocFields DF { get; set; }

        #region equatable, comparable

        public bool Equals(ObservableOpenDoc other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _projectName == other._projectName && _fileName == other._fileName && _isActive == other._isActive;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObservableOpenDoc)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_projectName != null ? _projectName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_fileName != null ? _fileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _isActive.GetHashCode();
                return hashCode;
            }
        }

        public int CompareTo(ObservableOpenDoc other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var projectNameComparison = string.Compare(_projectName, other._projectName, StringComparison.Ordinal);
            if (projectNameComparison != 0) return projectNameComparison;
            var fileNameComparison = string.Compare(_fileName, other._fileName, StringComparison.Ordinal);
            if (fileNameComparison != 0) return fileNameComparison;
            return _isActive.CompareTo(other._isActive);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is ObservableOpenDoc other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ObservableOpenDoc)}");
        }

        public static bool operator <(ObservableOpenDoc left, ObservableOpenDoc right)
        {
            return Comparer<ObservableOpenDoc>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(ObservableOpenDoc left, ObservableOpenDoc right)
        {
            return Comparer<ObservableOpenDoc>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(ObservableOpenDoc left, ObservableOpenDoc right)
        {
            return Comparer<ObservableOpenDoc>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(ObservableOpenDoc left, ObservableOpenDoc right)
        {
            return Comparer<ObservableOpenDoc>.Default.Compare(left, right) >= 0;
        }

        public static bool operator ==(ObservableOpenDoc left, ObservableOpenDoc right)
        {
            return Comparer<ObservableOpenDoc>.Default.Compare(left, right) == 0;
        }

        public static bool operator !=(ObservableOpenDoc left, ObservableOpenDoc right)
        {
            return Comparer<ObservableOpenDoc>.Default.Compare(left, right) != 0;
        }

        public override int Compare(ObservableItemBase other)
        {
            if (other is ObservableOpenDoc otherDoc)
                return CompareTo(otherDoc);
            else
                return 1;
        }

        #endregion
    }
}
