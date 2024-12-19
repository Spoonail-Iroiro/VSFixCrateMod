using HarmonyLib;
using SampleClassLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace FixCrateMod.Tests {
    internal class SampleClassPatchMain {
        Harmony harmony;

        public void init() {
            if (!Harmony.HasAnyPatches("sample")) {
                harmony = new Harmony("sample");
                harmony.PatchAll();
            }
        }
    }

    internal class FirstNoneItemSlotPatcher {
        public static MyItemSlot PatchedFirstNoneItemSlot(MyInventoryBase instance, MyControl control) {
            bool take = !control.ShiftKey;
            bool bulk = control.CtrlKey;
            Console.WriteLine($"Hello from Patched FNIS: {take}, {bulk}");
            return new MyItemSlot(65535);
        }
    }

    [HarmonyPatch(typeof(BlockEntityMine), "OnBlockInteractStart")]
    internal class BlockEntityMineOnBlockInteractStartPatcher {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            bool firstProcessed = false;
            var patchedMethodInfo = SymbolExtensions.GetMethodInfo(() => FirstNoneItemSlotPatcher.PatchedFirstNoneItemSlot(null, null));
            foreach (var instruction in instructions) {
                if (!firstProcessed && instruction.opcode == OpCodes.Callvirt && instruction.operand != null) {
                    var strop = instruction.operand.ToString();
                    if (strop != null && strop.Contains("FirstNonEmptySlot")) {
                        Console.WriteLine($"Found: {instruction}");
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, patchedMethodInfo);
                        firstProcessed = true;
                        // replace original instruction
                        continue;
                    }
                }

                yield return instruction;
            }

        }

    }
}
