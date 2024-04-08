using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace LeaveMyRezAlone.Patches;

[HarmonyPatch(typeof(UserInterface))]
public class ExampleShoppingCartPatch {
    [HarmonyPatch(nameof(UserInterface.LateUpdate))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> RemoveResolutionOverwrite(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return new CodeMatcher(instructions, generator)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldflda),
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Call))
            .ThrowIfNotMatch("Couldn't find `this.m_currentResolution.Equals(@int)`!")
            .Advance(1)
            .Insert(new CodeInstruction(OpCodes.Pop))
            .SearchForward(inst => inst.opcode == OpCodes.Brtrue_S || inst.opcode == OpCodes.Brtrue)
            .ThrowIfNotMatch("Couldn't find branch!")
            .SetOpcodeAndAdvance(OpCodes.Br)
            .Instructions();
    }
}