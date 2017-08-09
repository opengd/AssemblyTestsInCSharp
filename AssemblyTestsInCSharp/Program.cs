using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AssemblyTestsInCSharp
{
    class Program
    {
        private delegate Int32 MyAdd(Int32 x, Int32 y, Int32 z);

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(Int32 hProcess, System.IntPtr lpAddress, int dwSize, int flNewProtect, out int lpflOldProtect);

        static void Main(string[] args)
        {
            // https://defuse.ca/online-x86-assembler.htm#disassembly2
            var byteCodeArray = new Byte[]
            {
                0x8B, 0x44, 0x24, 0x04, // mov eax,dword ptr [esp+4]
                0x03, 0x44, 0x24, 0x08, // add eax,dword ptr [esp+8]
                0x03, 0x44, 0x24, 0x0C, // add eax,dword ptr [esp+12]
                0xC2, 0x0c, 0x00 // ret eax
            };

            GCHandle pinnedArray = GCHandle.Alloc(byteCodeArray, GCHandleType.Pinned);

            IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            var hProcess = Process.GetCurrentProcess().Handle.ToInt32();

            var myAdd = (MyAdd)Marshal.GetDelegateForFunctionPointer(pointer, typeof(MyAdd));

            VirtualProtectEx(hProcess, pointer, byteCodeArray.Length, 0x40, out int flOldProtect);

            Int32 result = 0;
            try
            {
                result = myAdd(1, 2, 3);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                VirtualProtectEx(hProcess, pointer, byteCodeArray.Length, flOldProtect, out flOldProtect);
                Console.WriteLine("Result: {0}", result);

                pinnedArray.Free();
            }

            Console.ReadKey();
        }
    }
}
