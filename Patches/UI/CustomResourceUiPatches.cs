using BaseLib.Abstracts;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace BaseLib.Patches.UI;

[HarmonyPatch]
class CustomResourceUiPatches
{
    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateEnergyCostVisuals))]
    [HarmonyPostfix]
    static void UpdateCustomCostVisuals(NCard __instance, PileType pileType)
    {
        var card = __instance.Model;
        if (card == null) return;
        
        foreach (var resourceHandler in CustomResourcePatches.RegisteredResources)
        {
            resourceHandler.GetCost(card)?.UpdateCostVisuals(__instance, pileType);
        }
        
    }

    [HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
    [HarmonyTranspiler]
    static List<CodeInstruction> AddUIElement(IEnumerable<CodeInstruction> code)
    {
        return new InstructionPatcher(code)
            .Match(new CallMatcher(typeof(GodotTreeExtensions).Method(nameof(GodotTreeExtensions.AddChildSafely))))
            .Insert([
                CodeInstruction.LoadArgument(0),
                CodeInstruction.LoadArgument(1),
                CodeInstruction.Call(typeof(CustomResourceUiPatches), nameof(AddUIElementInsert))
            ]);
    }

    static void AddUIElementInsert(NCombatUi __instance, CombatState combatState)
    {
        var playerCombatState = LocalContext.GetMe(combatState)?.PlayerCombatState;
        if (playerCombatState == null)
        {
            BaseLibMain.Logger.Warn("Failed to initialize custom resource UI; local player combat state null");
            return;
        }
        
        foreach (var resource in CustomResourcePatches.RegisteredResources)
        {
            resource.VisualsHandler?.AddDisplay(__instance, playerCombatState);
        }
    }
    
    /*public static AddedNode<NCard, NAdditionalCostDisplay> Node = new((card) =>
    {
        //would probably suggest loading from scene rather than this manual setup
        var control = new NAdditionalCostDisplay();
        
        var tex = ResourceLoader.Load<Texture2D>("res://BaseLibTests/images/powers/power.png");
        
        var size = tex.GetSize();
        var texRect = new TextureRect();
        texRect.Name = tex.ResourcePath;
        texRect.Size = new(50, 50);
        texRect.Texture = tex;
        texRect.PivotOffset = size / 2f;
        texRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        texRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        texRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        
        control.Size = new(50, 50);
        control.Position = new(-126, -231);
        control.AddChild(texRect);
        
        var label = new Label { Text = "1" };
        label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        control.AddChild(label);

        //For cards specifically, this is necessary to use the CardContainer instead of the NCard parent node, 
        //which does not receive all the transforms.
        var cardContainer = card.GetChild(0)!;
        cardContainer.AddChild(control);

        //Changing position to before the star icon node.
        cardContainer.MoveChild(control, cardContainer.GetNode("%StarIcon").GetIndex());

        return control;
    });*/
}

/// <summary>
/// Interface for a class that handles the resource amount display of a custom resource.
/// </summary>
public interface ICustomResourceVisualsHandler
{
    void AddDisplay(NCombatUi nCombatUi, PlayerCombatState playerCombatState);
}

/// <summary>
/// Interface for a class that handles the cost visuals of a custom resource.
/// </summary>
public interface ICustomCostVisualsHandler
{
    
}

