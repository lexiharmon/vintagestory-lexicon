using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace Lexicon
{
    /// <summary>
    /// Replaces the survival handbook's partial-match filtering with exact matching.
    /// The ordering of results is kept the same as vanilla.
    /// </summary>
    [HarmonyPatch(typeof(GuiDialogHandbook), nameof(GuiDialogHandbook.FilterItems))]
    public static class FilterItemsPatch
    {
        public static ExactSearchConfig Config = new ExactSearchConfig();

        public static bool Prefix(
            GuiDialogHandbook __instance,
            List<GuiHandbookPage> ___allHandbookPages,
            List<IFlatListItem> ___shownHandbookPages,
            string ___currentSearchText,
            bool ___loadingPagesAsync,
            GuiComposer ___overviewGui,
            double ___listHeight)
        {
            // Only the survival handbook; leave the command handbook alone. An empty search shows everything, as in vanilla.
            if (!Config.Enabled || __instance is not GuiDialogSurvivalHandbook) return true;
            string searchText = ___currentSearchText ?? "";
            if (searchText.Trim().Length == 0) return true;

            ___shownHandbookPages.Clear();

            if (!___loadingPagesAsync)
            {
                var matcher = new ExactMatcher(searchText, Config.Mode, Config.SearchDescriptions, Config.PrefixLastWord);
                var strictRegex = GuiDialog.RegexFromSearchText(searchText, true);

                var weightedPages = new List<WeightedHandbookPage>();
                foreach (GuiHandbookPage page in ___allHandbookPages)
                {
                    if (__instance.currentCatgoryCode != null && page.CategoryCode != __instance.currentCatgoryCode || page.IsDuplicate) continue;

                    PageText pageText = page.GetPageText();
                    if (!matcher.Matches(pageText.Title, pageText.Text)) continue;

                    int titleMatches = matcher.WholeWordHits(pageText.Title);
                    weightedPages.Add(new WeightedHandbookPage
                    {
                        Page = page,
                        TitleMatches = titleMatches,
                        StrictTitleMatches = titleMatches,
                        TitleLength = pageText.Title?.Length ?? 0,
                        TextMatches = GuiDialog.CountMatches(pageText.Text ?? "", strictRegex),
                        SearchWeight = 1 + page.SearchWeightOffset
                    });
                }

                weightedPages.Sort((a, b) =>
                {
                    int sort = b.StrictTitleMatches - a.StrictTitleMatches;
                    if (sort != 0) return sort;
                    sort = b.SearchWeight.CompareTo(a.SearchWeight);
                    if (sort != 0) return sort;
                    sort = a.TitleLength - b.TitleLength;
                    if (sort != 0) return sort;
                    return b.TextMatches - a.TextMatches;
                });

                weightedPages.ForEach(wp => ___shownHandbookPages.Add(wp.Page));
            }

            GuiElementFlatList stacklist = ___overviewGui.GetFlatList("stacklist");
            stacklist.CalcTotalHeight();
            ___overviewGui.GetScrollbar("scrollbar").SetHeights((float)___listHeight, (float)stacklist.insideBounds.fixedHeight);

            return false;
        }
    }
}
