namespace WinCompose.gui
{
    public class CategoryViewModel
    {
        public CategoryViewModel(string name, int start, int end)
        {
            Name = name;
            RangeStart = start;          
            RangeEnd = end;
        }

        public string Name { get; private set; }

        public int RangeStart { get; private set; }
        
        public int RangeEnd { get; private set; }
    }
}
