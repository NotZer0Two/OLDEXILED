// -----------------------------------------------------------------------
// <copyright file="StaminaRegenArmor.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Generic
{
    using Exiled.API.Features;
#pragma warning disable SA1313

    using HarmonyLib;
    using InventorySystem.Items.Armor;

    /// <summary>
    /// Patches <see cref="BodyArmor.StaminaRegenMultiplier"/>.
    /// Implements <see cref="API.Features.Items.Armor.StaminaRegenMultiplier"/>.
    /// </summary>
    [HarmonyPatch(typeof(BodyArmor), nameof(BodyArmor.StaminaRegenMultiplier), MethodType.Getter)]
    internal class StaminaRegenArmor
    {
        private static void Postfix(BodyArmor __instance, ref float __result)
        {
            if(Player.TryGet(__instance.OwnerInventory._hub, out Player player) && player.CurrentArmor != null)
                __result *= player.CurrentArmor.StaminaRegenMultiplier;
        }
    }
}