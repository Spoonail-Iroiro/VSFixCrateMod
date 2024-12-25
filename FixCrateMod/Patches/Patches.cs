using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AirThermoMod.Patches {
    internal enum FirstNonEmptySlotPatchType {
        TakeOne,
        TakeFullStack
    }

    internal class PatchedFirstNonEmptySlot {

        public static ItemSlot Dispatch(InventoryBase instance, IPlayer byPlayer) {
            bool take = !byPlayer.Entity.Controls.ShiftKey;
            bool bulk = byPlayer.Entity.Controls.CtrlKey;

            if (take) {
                return bulk ? MaxSlot(instance) : NotZeroMinSlot(instance);
            }

            return instance.FirstNonEmptySlot;
        }

        public static ItemSlot MaxSlot(InventoryBase instance) {
            instance.Api.Logger.Event("MaxSlot");
            int maxCount = 0;
            ItemSlot rtn = null;
            using (IEnumerator<ItemSlot> enumerator = instance.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    ItemSlot current = enumerator.Current;
                    if (!current.Empty && current.StackSize > maxCount) {
                        rtn = current;
                        maxCount = current.StackSize;
                    }

                }
            }

            return rtn;
        }
        public static ItemSlot NotZeroMinSlot(InventoryBase instance) {
            instance.Api.Logger.Event("NotZeroMinSlot");
            int minCount = int.MaxValue;
            ItemSlot rtn = null;
            using (IEnumerator<ItemSlot> enumerator = instance.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    ItemSlot current = enumerator.Current;
                    if (!current.Empty && current.StackSize < minCount) {
                        rtn = current;
                        minCount = current.StackSize;
                    }
                }
            }

            return rtn;
        }
    }

    [HarmonyPatch(typeof(BlockEntityCrate), "OnBlockInteractStart")]
    internal class BlockEntityCrateOnBlockInteractStartPatcher {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            // Replaces the first call of `inventory.FirstNonEmptySlot` in `BlockEntityCrate.OnBlockInteractStart` with `PatchedFirstNonEmptySlot.Dispatch`
            // The `Dispatch` method determines the behavior based on the context:
            // (A) Calls `NonZeroMinSlot(inventory)` if `take && !bulk`. This ensures the player takes an item from the slot with minimum count
            // (B) Calls `MaxSlot(inventory)` if `take && bulk`. This ensures the player takes all items from the slot with maximum count,
            //     which means they always takes a full stack or all remaining items in the crate
            // (C) Calls `inventory.FirstNonEmptySlot` in all other cases
            bool firstProcessed = false;
            var dispatchMethodInfo = SymbolExtensions.GetMethodInfo(() => PatchedFirstNonEmptySlot.Dispatch(null, null));
            foreach (var instruction in instructions) {
                if (!firstProcessed && instruction.opcode == OpCodes.Callvirt && instruction.operand != null) {
                    var strop = instruction.operand.ToString();
                    if (strop != null && strop.Contains("FirstNonEmptySlot")) {
                        // The private field `inventory` is already on the stack because here is where `inventory.FirstNonEmptySlot` is called
                        // Loads the argument `IPlayer byPlayer` on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        // This calls PatchedFirstNonEmptySlot.Dispatch(inventory, byPlayer)
                        yield return new CodeInstruction(OpCodes.Call, dispatchMethodInfo);
                        firstProcessed = true;
                        // Skips the original instruction
                        continue;
                    }
                }

                yield return instruction;
            }
        }
    }

    [HarmonyPatch]
    internal class PatchMain {
        public Harmony harmony;

        public string ModID { get; private set; }

        public PatchMain(string modID) {
            ModID = modID;
        }

        public void init() {
            if (!Harmony.HasAnyPatches(ModID)) {
                harmony = new Harmony(ModID);
                harmony.PatchAll();
            }
        }

        public void Dispose() {
            harmony?.UnpatchAll(ModID);
        }
    }
}
