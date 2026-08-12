using BaseLib.Abstracts;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Runs;

namespace BaseLib.Patches.UI;

[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary.OnSubmenuOpened))]
class InRunCompendiumPatch
{
    [HarmonyTranspiler]
    static List<CodeInstruction> AddCheckCustomDefault(IEnumerable<CodeInstruction> code)
    {
        return new InstructionPatcher(code)
            .Match(new InstructionMatcher()
                .call_any(typeof(LocalContext).Method(nameof(LocalContext.GetMe), [typeof(IPlayerCollection)]))
                .stloc_any().LazyMatch())
            .Step(-1)
            .Insert([
                CodeInstruction.LoadArgument(0),
                CodeInstruction.Call(typeof(InRunCompendiumPatch), nameof(CheckCustomDefault))
            ]);
    }

    static CharacterModel? CheckCustomDefault(CharacterModel? character, NCardLibrary __instance)
    {
        if (character is not CustomCharacterModel customCharacter)
            return character;

        var defaultModel = ModelDb.GetByIdOrNull<AbstractModel>(customCharacter.DefaultCompendiumOpenModelId);

        if (defaultModel is CharacterModel characterModel)
        {
            if (__instance._cardPoolFilters.ContainsKey(characterModel))
                return characterModel;
            
            BaseLibMain.Logger.Info("Character not visible in compendium; defaulting to null");
        }
        else if (defaultModel is not CharacterModel)
        {
            BaseLibMain.Logger.Warn($"Default compendium model ID {customCharacter.DefaultCompendiumOpenModelId} " +
                                    $"for character {customCharacter.Id} is a non-character ID.");
        }
        
        return null;

    }
}