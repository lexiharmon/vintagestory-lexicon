namespace Lexicon
{
    public enum ExactSearchMode
    {
        /// <summary>Every word typed must appear as a complete word (order and position don't matter).</summary>
        Words,
        /// <summary>The whole query must appear as one contiguous phrase of complete words.</summary>
        Phrase,
        /// <summary>The page title must be exactly what was typed.</summary>
        Title
    }

    public class ExactSearchConfig
    {
        /// <summary>Set to false to get the vanilla search back without uninstalling the mod.</summary>
        public bool Enabled = true;

        public ExactSearchMode Mode = ExactSearchMode.Words;

        /// <summary>
        /// Lets the last word you type match the start of a word, so "iron sa" finds "Iron Saw" while you are still typing.
        /// Earlier words must still be complete words, and complete matches are listed first. Ignored in Title mode.
        /// </summary>
        public bool PrefixLastWord = true;

        /// <summary>When true, page descriptions are searched too (still whole-word only). Titles only by default.</summary>
        public bool SearchDescriptions = false;
    }
}
