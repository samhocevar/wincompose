using System.Globalization;
using System.Reflection;

namespace WinCompose.gui
{
    public class SequenceViewModel
    {
        public SequenceViewModel(CategoryViewModel category, char character, string desc)
        {
            Category = category;
            Character = character;
            Description = desc;
        }

        public CategoryViewModel Category { get; private set; }

        public char Character { get; private set; }

        // TODO: verify this actually returns the Unicode of the char...
        public int Unicode { get { return Character; } }

        public UnicodeCategory UnicodeCategory { get { return CharUnicodeInfo.GetUnicodeCategory(Character); } }

        public string Description { get; private set; }

        public object Sequence { get; set; }
    }
}