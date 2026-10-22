using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vintagestory.API.Util;

namespace Lexicon
{
    /// <summary>Decides whether a handbook page matches a query as a whole word / phrase / title.</summary>
    public class ExactMatcher
    {
        const string WordStart = @"(?<![\p{L}\p{N}])";
        const string WordEnd = @"(?![\p{L}\p{N}])";
        const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        readonly ExactSearchMode mode;
        readonly bool searchDescriptions;
        readonly string normalizedQuery;
        readonly List<Regex> wordPatterns = new List<Regex>();
        readonly List<Regex> wholeWordPatterns = new List<Regex>();
        readonly Regex phrasePattern;

        /// <param name="prefixLastWord">
        /// Treat the last word typed as the start of a word ("iron sa" finds "Iron Saw"); all earlier words stay whole-word.
        /// </param>
        public ExactMatcher(string query, ExactSearchMode mode, bool searchDescriptions, bool prefixLastWord)
        {
            this.mode = mode;
            this.searchDescriptions = searchDescriptions;

            // Same normalisation the game applies to search words (strips diacritics, quote marks, etc.)
            normalizedQuery = string.Join(" ", Regex.Split(query.ToSearchFriendly(), @"\s+").Where(w => w != ""));

            string[] words = normalizedQuery.Length == 0 ? Array.Empty<string>() : normalizedQuery.Split(' ');
            string wordEndForLast = prefixLastWord ? "" : WordEnd;

            for (int i = 0; i < words.Length; i++)
            {
                string escaped = Regex.Escape(words[i]);
                bool isLast = i == words.Length - 1;
                wordPatterns.Add(new Regex(WordStart + escaped + (isLast ? wordEndForLast : WordEnd), Options));
                wholeWordPatterns.Add(new Regex(WordStart + escaped + WordEnd, Options));
            }

            phrasePattern = new Regex(WordStart + string.Join(@"\s+", words.Select(Regex.Escape)) + wordEndForLast, Options);
        }

        /// <summary>How many of the typed words appear in the title as complete words; used to rank exact hits above prefix hits.</summary>
        public int WholeWordHits(string title)
        {
            title ??= "";
            return wholeWordPatterns.Count(p => p.IsMatch(title));
        }

        public bool Matches(string title, string text)
        {
            title ??= "";
            text ??= "";

            switch (mode)
            {
                case ExactSearchMode.Title:
                    return string.Equals(title.Trim(), normalizedQuery, StringComparison.OrdinalIgnoreCase);

                case ExactSearchMode.Phrase:
                    return phrasePattern.IsMatch(title) || (searchDescriptions && phrasePattern.IsMatch(text));

                default:
                    string haystack = searchDescriptions ? title + " " + text : title;
                    return wordPatterns.All(p => p.IsMatch(haystack));
            }
        }
    }
}
