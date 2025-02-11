﻿using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using HarmonyLib;
using Il2CppAssets.Scripts.Models.Towers;

namespace UltimateCrosspathing
{
    [HarmonyPatch(typeof(UpgradeObject), nameof(UpgradeObject.CheckBlockedPath))]
    internal class UpgradeObject_CheckBlockedPath
    {
        [HarmonyPostfix]
        internal static void Postfix(UpgradeObject __instance, ref int __result)
        {
            if (__instance.tts != null && LoadInfo.ShouldWork(__instance.tts.Def.baseId))
            {
                var tier = __instance.tier;
                var tiers = __instance.tts.Def.tiers;
                var sum = tiers.Sum();
                var remainingTiers = Settings.MaxTiers - sum;
                __result = tier + remainingTiers;
                if (__result > 5)
                {
                    __result = 5;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.IsUpgradePathClosed))]
    internal class TowerSelectionMenu_IsUpgradePathClosed
    {
        [HarmonyPostfix]
        internal static void Postfix(TowerSelectionMenu __instance, int path, ref bool __result)
        {
            if (__instance.selectedTower == null) return;
            
            var towerModel = __instance.selectedTower.Def;
            var blockBeastHandler = towerModel.baseId == TowerType.BeastHandler &&
                                    towerModel.tiers.Count(t => t > 0) >= 2 &&
                                    towerModel.tiers[path] == 0;
            if (LoadInfo.ShouldWork(towerModel.baseId) && !blockBeastHandler)
            {
                __result &= towerModel.tiers.Sum() >= Settings.MaxTiers;
            }
        }
    }
    
    /// <summary>
    /// Fix v38.1 inlining of TowerSelectionMenu.IsUpgradePathClosed method
    /// </summary>
    [HarmonyPatch(typeof(UpgradeObject), nameof(UpgradeObject.UpdateVisuals))]
    internal static class UpgradeObject_UpdateVisuals
    {
        [HarmonyPrefix]
        private static bool Prefix(UpgradeObject __instance, int path, bool upgradeClicked)
        {
            if (__instance.towerSelectionMenu.IsUpgradePathClosed(path))
            {
                __instance.upgradeButton.SetUpgradeModel(null);
            }
            __instance.CheckLocked();
            var maxTier = __instance.CheckBlockedPath();
            var maxTierRestricted = __instance.CheckRestrictedPath();
            __instance.SetTier(__instance.tier, maxTier, maxTierRestricted);
            __instance.currentUpgrade.UpdateVisuals();
            __instance.upgradeButton.UpdateVisuals(path, upgradeClicked);
            
            return false;
        }
    }

    [HarmonyPatch(typeof(Bank), nameof(Bank.Cash), MethodType.Setter)]
    internal class Bank_Cash
    {
        [HarmonyPostfix]
        internal static void Postfix(Bank __instance)
        {
            if (__instance.bankModel.autoCollect && __instance.Cash >= __instance.bankModel.capacity)
            {
                __instance.Collect();
            }
        }
    }
}