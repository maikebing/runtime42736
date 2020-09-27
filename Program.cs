using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace runtime42736
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct blackdic
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint issuerid;//4

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort cardhead;//2

        [MarshalAs(UnmanagedType.U8, SizeConst = 8)]
        public ulong cardid; //8

        [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte cardtype; //1
        [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte blacktype;//1
    }




    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BLFile
    {

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] FileVersion;

        [MarshalAs(UnmanagedType.I4, SizeConst = 4)]
        public int Total;

        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint BuildDateTime;
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Need  a file!");
            }
            else
            {
                var fi = new System.IO.FileInfo(args[0]);
                if (fi.Exists)
                {
                    using (var stm = fi.OpenRead())
                    {
                        int rawsize = Marshal.SizeOf<BLFile>();
                        byte[] rawdatas = new byte[rawsize];
                        stm.Read(rawdatas, 0, rawsize);
                        IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                        Marshal.Copy(rawdatas, 0, buffer, rawsize);
                        var head = Marshal.PtrToStructure<BLFile>(buffer);
                        Marshal.FreeHGlobal(buffer);
                        Console.WriteLine($"Version:{Encoding.Default.GetString(head.FileVersion)}, Total{head.Total} BuildDateTime:{new DateTime(1970, 1, 1).AddHours(8).AddSeconds(head.BuildDateTime) }");
                        stm.Close();
                        stm.Dispose();
                        int index = new Random(DateTime.Now.Millisecond).Next(0, head.Total);
                        using (var mmf = MemoryMappedFile.CreateFromFile(fi.FullName, FileMode.Open, "MyFile"))
                        {
                            //using (var cva = mmf.CreateViewAccessor(rawsize, fi.Length - rawsize))
                            //{
                            //    var arys = new blackdic[head.Total];
                            //    cva.ReadArray<blackdic>(0, arys, 0, head.Total);
                            //    var rnd = arys[index];
                            //    Console.WriteLine($"Good Value:{rnd.cardhead:0000}{rnd.cardid:000000000000000}");
                            //    cva.Dispose();
                            //}
                            //using (var cva = mmf.CreateViewAccessor(rawsize, fi.Length - rawsize))
                            //{
                              
                            //    int bdsize = Marshal.SizeOf<blackdic>();
                            //    cva.Read(index * bdsize, out blackdic rnd);
                            //    Console.WriteLine($"Good Value:{rnd.cardhead:0000}{rnd.cardid:000000000000000}");
                            //    cva.Dispose();
                            //}
                            using (var cva = mmf.CreateViewAccessor(rawsize, fi.Length - rawsize))
                            {
                                unsafe
                                {
                                    int bdsize = Marshal.SizeOf<blackdic>();
                                    byte* ptr = null;
                                    cva.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                                    try
                                    {
                                        ulong length = cva.SafeMemoryMappedViewHandle.ByteLength;
                                        blackdic* _bd_ptr = (blackdic*)(ptr +cva.PointerOffset);
                                        var rnd = _bd_ptr[index];
                                        Console.WriteLine($"Good Value:{rnd.cardhead:0000}{rnd.cardid:000000000000000}");
                                    }
                                    finally
                                    {
                                        cva.SafeMemoryMappedViewHandle.ReleasePointer();
                                    }
                                    cva.Dispose();
                                }
                            }
                            mmf.Dispose();
                        }
                    }
                    GC.Collect();
                }
                else
                {
                    Console.WriteLine($"File {fi.Name} is't exist!");
                }
            }
            Console.WriteLine("Pass any key to exit!");
            Console.ReadKey();
        }
    }
}
