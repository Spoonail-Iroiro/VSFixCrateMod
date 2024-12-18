using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public InventoryBase Target { get; private set; }

        public FirstNonEmptySlotPatchType Type { get; private set; }

        public void Activate(InventoryBase target, FirstNonEmptySlotPatchType type) {
            Target = target;
            Type = type;
        }

        public void Deactivate() {
            Target = null;
        }

        public bool Prefix(InventoryBase instance, ref ItemSlot result) {
            var api = instance.Api;
            if (Target != null) {
                if (Type == FirstNonEmptySlotPatchType.TakeOne) {
                    result = NotZeroMinSlot(instance);
                }
                else if (Type == FirstNonEmptySlotPatchType.TakeFullStack) {
                    result = MaxSlot(instance);
                }
                else {
                    throw new NotImplementedException();
                }

                Deactivate();
                return false;
            }

            return true;
        }

        public ItemSlot MaxSlot(InventoryBase instance) {
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
        public ItemSlot NotZeroMinSlot(InventoryBase instance) {
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

    [HarmonyPatch(typeof(InventoryBase), "FirstNonEmptySlot", MethodType.Getter)]
    internal static class FirstNonEmptySlotPatcher {
        private static Dictionary<EnumAppSide, PatchedFirstNonEmptySlot> patches = new Dictionary<EnumAppSide, PatchedFirstNonEmptySlot> {
            [EnumAppSide.Client] = new PatchedFirstNonEmptySlot(),
            [EnumAppSide.Server] = new PatchedFirstNonEmptySlot()
        };

        public static void Activate(EnumAppSide side, InventoryBase target, FirstNonEmptySlotPatchType type) {
            patches[side].Activate(target, type);
        }

        public static void Deactivate(EnumAppSide side) {
            patches[side].Deactivate();
        }

        public static bool Prefix(InventoryBase __instance, ref ItemSlot __result) {
            if (__instance.Api != null) {
                if (!patches[__instance.Api.Side].Prefix(__instance, ref __result)) return false;
            }

            return true;
        }

        public static void Postfix(ItemSlot __result) {
        }

    }

    [HarmonyPatch(typeof(BlockEntityCrate), "OnBlockInteractStart")]
    internal class BlockEntityCrateOnBlockInteractStartPatcher {
        // This prefix should run right before OnBlockInteractStart, so set low priority here
        [HarmonyPriority(Priority.Low)]
        public static bool Prefix(BlockEntityCrate __instance, IPlayer byPlayer, ref bool __result, InventoryGeneric ___inventory) {
            bool take = !byPlayer.Entity.Controls.ShiftKey;
            bool bulk = byPlayer.Entity.Controls.CtrlKey;
            if (take) {
                FirstNonEmptySlotPatcher.Activate(__instance.Api.Side, ___inventory, bulk ? FirstNonEmptySlotPatchType.TakeFullStack : FirstNonEmptySlotPatchType.TakeOne);
            }

            return true;
        }

        public static void Postfix(BlockEntityCrate __instance) {
            FirstNonEmptySlotPatcher.Deactivate(__instance.Api.Side);
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
