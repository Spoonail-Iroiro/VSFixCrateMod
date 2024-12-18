using AirThermoMod.Patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace FixCrateMod {
    public class FixCrateModModSystem : ModSystem {
        public static string ModID { get; private set; } = "";

        PatchMain patcher;

        public override void Start(ICoreAPI api) {
            ModID = Mod.Info.ModID;
            patcher = new PatchMain(ModID);

            patcher.init();
        }

        public override void StartServerSide(ICoreServerAPI api) {
        }

        public override void StartClientSide(ICoreClientAPI api) {
        }

        public override void Dispose() {
            base.Dispose();

            patcher?.Dispose();
        }

    }
}
