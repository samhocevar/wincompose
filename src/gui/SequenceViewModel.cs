using System.Globalization;
using System.Reflection;

namespace WinCompose.gui
{
    public class SequenceViewModel
    {
        public SequenceViewModel(CategoryViewModel category, Sequence sequence)
        {
            Category = category;
            Character = sequence.m_result[0];
            Description = sequence.m_description;
            Sequence = sequence.m_keys;
        }

        public CategoryViewModel Category { get; private set; }

        public char Character { get; private set; }

        // TODO: verify this actually returns the Unicode of the char...
        public int Unicode { get { return Character; } }

        public UnicodeCategory UnicodeCategory { get { return CharUnicodeInfo.GetUnicodeCategory(Character); } }

        public string Description { get; private set; }

        public string Sequence { get; set; }
    }
}