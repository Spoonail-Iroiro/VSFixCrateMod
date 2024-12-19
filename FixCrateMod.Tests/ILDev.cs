using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FixCrateMod.Tests {
    internal class MethodClass {
        public static int GetConst() {
            return 80;
        }
    }


    internal class ILDev {
        public Func<int, int, int> Create() {
            var methodInfoGetConst = typeof(MethodClass).GetMethod("GetConst", new Type[] { });

            Type[] addArgs = new[] { typeof(int), typeof(int) };

            DynamicMethod addMethod = new DynamicMethod(
                "MyAdd",
                typeof(int),
                addArgs
            );

            var il = addMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Add);
            //il.Emit(OpCodes.Ldc_I4_S, 99);
            il.EmitCall(OpCodes.Call, methodInfoGetConst, null);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ret);

            var add = (Func<int, int, int>)addMethod.CreateDelegate(typeof(Func<int, int, int>));

            return add;
        }
    }
}
