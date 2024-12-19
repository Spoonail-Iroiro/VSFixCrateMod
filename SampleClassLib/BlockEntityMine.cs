namespace SampleClassLib {
    public class MyControl {
        public bool ShiftKey {
            get {
                return false;
            }
        }

        public bool CtrlKey {
            get {
                return false;
            }
        }

    }

    public class MyItemSlot {
        int id = 0;

        public MyItemSlot(int id) {
            this.id = id;
        }


        public override string ToString() {
            return $"ItemSlot[{id}]";
        }
    }

    public class MyInventoryBase {
        public MyItemSlot FirstNonEmptySlot {
            get {
                return new MyItemSlot(0);
            }
        }
    }

    public class BlockEntityMine {
        MyInventoryBase inventory = new MyInventoryBase();

        public void OnBlockInteractStart(MyControl control) {
            bool put = control.ShiftKey;
            bool take = !put;
            bool bulk = control.CtrlKey;

            var ownSlot = inventory.FirstNonEmptySlot;

            Console.WriteLine($"{put}, {take}, {bulk}, {ownSlot.ToString()}");
        }

    }
}
