using System.Reflection;
using BaseLib.Utils.NodeFactories;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace BaseLib.Patches.Compatibility;

[HarmonyPatch]
class OptionalFormNodePatch
{
    private static readonly MethodInfo? AddFormVfx = typeof(NCreatureVisuals).Method("AddFormVfx");
    private static readonly MethodInfo? RemoveFormVfx = typeof(NCreatureVisuals).Method("RemoveFormVfx");

    private static readonly FieldInfo? RunStateRef = typeof(NCreatureVisuals).Field("_formVfxHolder");
    
    static IEnumerable<MethodBase> TargetMethods()
    {
        if (AddFormVfx != null) yield return AddFormVfx;
        if (RemoveFormVfx != null) yield return RemoveFormVfx;
    }

    static bool Prepare()
    {
        return AddFormVfx != null || RemoveFormVfx != null;
    }
    
    [HarmonyPrefix]
    static bool SkipIfMissingHolder(NCreatureVisuals __instance)
    {
        if (NodeFactory.CreatedFromFactory(__instance) && RunStateRef != null && RunStateRef.GetValue(__instance) == null)
        {
            BaseLibMain.Logger.Info("Skipping form vfx; no form vfx holder.");
            return false;
        }

        return true;
    }
}