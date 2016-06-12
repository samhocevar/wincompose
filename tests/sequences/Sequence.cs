using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WinCompose
{
    [TestClass]
    public class SequenceTreeTest
    {
        SequenceTree tree = new SequenceTree();

        [TestMethod]
        public void TestUnicodeSequences()
        {
            AssertUnicodeSequence("u0020", " ");
            AssertUnicodeSequence("u0057", "W");
            AssertUnicodeSequence("u0043", "C");
            AssertUnicodeSequence("u00FC", "ü");
            AssertUnicodeSequence("u00fc", "ü");
            AssertShortUnicodeSequence("ufc", "ü");
            AssertShortUnicodeSequence("udc", "Ü");
            AssertUnicodeSequence("u03c0", "π"); // Greek lowercase letter pi
            AssertUnicodeSequence("u0430", "а"); // Cyrillic small letter a
            AssertShortUnicodeSequence("u1e60", "\u1e60"); // Latin capital letter s with dot above
            AssertShortUnicodeSequence("u2013", "–"); // En-dash
            AssertUnicodeSequence("u328c", "\u328c"); // Circled ideograph water
            AssertUnicodeSequence("u4de1", "\u4de1"); // Hexagram for great power
            AssertUnicodeSequence("u5000", "\u5000");
            AssertUnicodeSequence("u6000", "\u6000");
            AssertUnicodeSequence("u7000", "\u7000");
            AssertUnicodeSequence("u8000", "\u8000");
            AssertUnicodeSequence("u9000", "\u9000");
            AssertUnicodeSequence("ua000", "\ua000");
            AssertUnicodeSequence("ub000", "\ub000");
            AssertUnicodeSequence("uc000", "\uc000");
            AssertUnicodeSequence("ud000", "\ud000");
            AssertShortUnicodeSequence("ue000", "\ue000");
            AssertShortUnicodeSequence("uFEFF", "\uFEFF"); // Zero width non-joiner

            AssertUnicodeSequence("u1F600", char.ConvertFromUtf32(0x1F600)); // Grinning face

            AssertInvalidPrefix("x");
            AssertInvalidSequence("u"); // Too short
            AssertInvalidSequence("u3"); // Too short
            AssertInvalidSequence("uf"); // Too short
            AssertInvalidSequence("ud800"); // Part of a surrogate pair
            AssertInvalidSequence("udbff"); // Part of a surrogate pair
            AssertInvalidSequence("udc00"); // Part of a surrogate pair
            AssertInvalidSequence("udfff"); // Part of a surrogate pair
        }

        void AssertValidPrefix(string sequence)
        {
            var keySequence = UnicodeKeySequence(sequence);
            if (!tree.IsValidPrefix(keySequence, false))
                Assert.Fail("Prefix \"" + keySequence + "\" must be valid.");
        }

        void AssertInvalidPrefix(string sequence)
        {
            var keySequence = UnicodeKeySequence(sequence);
            if (tree.IsValidPrefix(keySequence, false))
                Assert.Fail("Prefix \"" + keySequence + "\" must be invalid.");
        }

        void AssertValidSequence(string sequence)
        {
            var keySequence = UnicodeKeySequence(sequence);
            if (!tree.IsValidSequence(keySequence, false))
                Assert.Fail("Sequence \"" + keySequence + "\" must be valid.");
        }

        void AssertInvalidSequence(string sequence)
        {
            var keySequence = UnicodeKeySequence(sequence);
            if (tree.IsValidSequence(keySequence, false))
                Assert.Fail("Sequence \"" + keySequence + "\" must be invalid.");
        }

        void AssertUnicodeSequence(string sequence, string result)
        {
            for (var i = 1; i < sequence.Length - 1; i++)
                AssertValidPrefix(sequence.Substring(0, i));
            AssertInvalidPrefix(sequence);
            AssertValidSequence(sequence);
            Assert.AreEqual(result, tree.GetSequenceResult(UnicodeKeySequence(sequence), false));
        }

        // These sequences could be continued to form a longer Unicode
        // sequence, therefore they are not terminated automatically 
        // but have to be terminated by pressing the Compose key again.
        void AssertShortUnicodeSequence(string sequence, string result)
        {
            for (var i = 1; i < sequence.Length - 1; i++)
                AssertValidPrefix(sequence.Substring(0, i));
            AssertValidSequence(sequence);
            Assert.AreEqual(result, tree.GetSequenceResult(UnicodeKeySequence(sequence), false));
        }

        KeySequence UnicodeKeySequence(string keys)
        {
            var keySequence = new KeySequence();
            foreach (var ch in keys)
                keySequence.Add(Key.FromKeySym(Convert.ToString(ch)));
            return keySequence;
        }
    }
}
