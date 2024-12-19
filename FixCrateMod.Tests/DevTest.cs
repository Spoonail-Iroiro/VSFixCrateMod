using FluentAssertions;
using SampleClassLib;

namespace FixCrateMod.Tests {
    [TestClass]
    public sealed class DevTest {
        [TestMethod]
        public void TestMethod1() {
            var myDev = new ILDev();

            var myAdd = myDev.Create();

            myAdd(1, 2).Should().Be(102);
        }

        [TestMethod]
        public void TestBlockEntityMine() {
            var patchMain = new SampleClassPatchMain();
            patchMain.init();

            var beMine = new BlockEntityMine();
            var control = new MyControl();
            beMine.OnBlockInteractStart(control);
        }
    }
}
