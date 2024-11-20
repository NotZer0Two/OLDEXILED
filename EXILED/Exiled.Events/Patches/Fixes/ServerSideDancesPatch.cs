// -----------------------------------------------------------------------
// <copyright file="ServerSideDancesPatch.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
#pragma warning disable SA1313
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features.Pools;
    using Exiled.API.Features;
    using HarmonyLib;
    using Mirror;
    using PlayerRoles.PlayableScps.Scp3114;
    using UnityEngine;

    /// <summary>
    /// Patches the <see cref="Scp3114Dance.DanceVariant"/>.
    /// Fix that the game doesn't write this.
    /// </summary>
    [HarmonyPatch(typeof(Scp3114Dance), nameof(Scp3114Dance.ServerWriteRpc))]
    internal class ServerSideDancesPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label skip = generator.DefineLabel();

            newInstructions.Add(new CodeInstruction(OpCodes.Ret));
            newInstructions[newInstructions.Count - 1].labels.Add(skip);

            newInstructions.InsertRange(7, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Br_S, skip),
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        private static void Postfix(ref Scp3114Dance __instance, NetworkWriter writer)
        {
            Npc npc = Npc.Get(__instance.Owner);
            if (npc != null && __instance.DanceVariant != byte.MaxValue)
            {
                writer.WriteByte((byte)__instance.DanceVariant);
                return;
            }

            writer.WriteByte((byte)Random.Range(0, 255));
            return;
        }
    }
}
