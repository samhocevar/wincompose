namespace WinCompose.gui
{
    public class CategoryViewModel : ViewModelBase
    {
        private bool isSelected;

        public CategoryViewModel(string name, int start, int end)
        {
            Name = name;
            RangeStart = start;          
            RangeEnd = end;
        }

        public string Name { get; private set; }

        public int RangeStart { get; private set; }
        
        public int RangeEnd { get; private set; }

        public bool IsSelected { get { return isSelected; } set { SetValue(ref isSelected, value, "IsSelected", RefreshFilter); } }

        private void RefreshFilter(bool obj)
        {
            RootViewModel.Instance.RefreshFilters();
        }
    }
}
