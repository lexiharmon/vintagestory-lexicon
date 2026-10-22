using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Lexicon
{
    public class LexiconMod : ModSystem
    {
        const string ConfigFile = "lexicon.json";

        Harmony harmony;

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        public override void StartClientSide(ICoreClientAPI api)
        {
            ExactSearchConfig config = null;
            try
            {
                config = api.LoadModConfig<ExactSearchConfig>(ConfigFile);
            }
            catch (System.Exception e)
            {
                api.Logger.Warning("[lexicon] Could not read {0}, using defaults: {1}", ConfigFile, e.Message);
            }

            if (config == null)
            {
                config = new ExactSearchConfig();
                api.StoreModConfig(config, ConfigFile);
            }
            FilterItemsPatch.Config = config;

            harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchAll(typeof(LexiconMod).Assembly);
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
        }
    }
}
