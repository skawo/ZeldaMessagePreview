using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeldaMessage
{
    public class RGB
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public RGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    public class MajoraMsgHeader
    {
        public short MessageID;
        public DataMajora.BoxType BoxType;
        public byte MajoraIcon;
        public short MajoraNextMessage;
        public short MajoraFirstItemPrice;
        public short MajoraSecondItemPrice;


        public MajoraMsgHeader(byte[] MesgData)
        {
            if (MesgData.Length <= 11)
                return;

            BoxType = (DataMajora.BoxType)MesgData[0];
            MajoraIcon = MesgData.Skip(2).Take(1).First();
            MajoraNextMessage = BitConverter.ToInt16(MesgData, 4);
            MajoraFirstItemPrice = BitConverter.ToInt16(MesgData, 6);
            MajoraSecondItemPrice = BitConverter.ToInt16(MesgData, 8);
        }
    }
}