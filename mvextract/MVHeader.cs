using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvextract
{
    unsafe struct MVHeader
    {
        public UInt32 Length;
        public UInt32 Height;
        public UInt32 FrameLength1;
        public UInt32 FrameLength2;

        public static MVHeader Read(byte[] data)
        {
            fixed (byte* pb = &data[0])
            {
                return *(MVHeader*)pb;
            }
        }
    }
}
