namespace VSDocBar
{
    /// <summary>
    /// common base class used for items in the list.
    /// </summary>
    internal abstract class ObservableItemBase : ViewModelBase
    {
        // converters used by derived classes to provide color and font weight info when requested
        protected static readonly DocListColorConverter _colorConverter = new DocListColorConverter();
        protected static readonly DocListFontWeightConverter _fontWeightConverter = new DocListFontWeightConverter();

        // convenience for comparison during list updates
        public abstract int Compare(ObservableItemBase other);
    }
}
