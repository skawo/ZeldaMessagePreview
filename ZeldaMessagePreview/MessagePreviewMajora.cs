using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaMessage
{
    public class MessagePreviewMajora
    {
        public MajoraMsgHeader Header = new MajoraMsgHeader(new byte[0]);
        public List<List<byte>> Message = new List<List<byte>>();
        public int MessageCount = 0;
        public bool BrightenText = true;
        public bool InBombersNotebook = false;

        private int OUTPUT_IMAGE_X = 256;
        private int OUTPUT_IMAGE_Y = 64 + (Properties.Resources.Box_End.Width / 2);

        public byte[] FontDataMajora = null;
        public byte[] FontDataMajora2 = null;
        public string Lang = "";

        public MessagePreviewMajora(byte[] MessageDataMajora, bool IsBomberNotebook = false, float[] _FontWidths = null, byte[] _FontData = null, float[] _FontWidths2 = null, byte[] _FontData2 = null, string LangName = "")
        {
            if (MessageDataMajora.Length <= 11)
                return;

            Lang = LangName;
            Header = new MajoraMsgHeader(MessageDataMajora);
            InBombersNotebook = IsBomberNotebook;
            SplitMsgIntoTextboxes(MessageDataMajora.Skip(11).ToArray());

            MessageCount = Message.Count;

            if (_FontWidths == null)
            {
                try
                {
                    if (System.IO.File.Exists("font.width_table"))
                    {
                        byte[] widths = System.IO.File.ReadAllBytes("font.width_table");

                        for (int i = 0; i < widths.Length; i += 4)
                        {
                            byte[] width = widths.Skip(i).Take(4).Reverse().ToArray();
                            DataMajora.FontWidths[i / 4] = BitConverter.ToSingle(width, 0);
                        }
                    }
                }
                catch (Exception)
                { }
            }
            else
            {
                DataMajora.FontWidths = _FontWidths;
            }


            if (_FontData == null)
            {
                try
                {
                    if (System.IO.File.Exists("font.font_static"))
                    {
                        FontDataMajora = System.IO.File.ReadAllBytes("font.font_static");
                    }
                }
                catch (Exception)
                {
                    FontDataMajora = null;
                }
            }
            else
                FontDataMajora = _FontData;



            if (_FontWidths2 == null)
            {
                try
                {
                    if (System.IO.File.Exists($"{Lang}.width_table"))
                    {
                        byte[] widths = System.IO.File.ReadAllBytes($"{Lang}.width_table");

                        for (int i = 0; i < widths.Length; i += 4)
                        {
                            byte[] width = widths.Skip(i).Take(4).Reverse().ToArray();
                            DataMajora.FontWidths2[i / 4] = BitConverter.ToSingle(width, 0);
                        }
                    }
                }
                catch (Exception)
                { }
            }
            else
            {
                Data.FontWidths2 = _FontWidths;
            }

            if (_FontData2 == null)
            {
                try
                {
                    if (System.IO.File.Exists($"{Lang}.font_static"))
                    {
                        FontDataMajora2 = System.IO.File.ReadAllBytes($"{Lang}.font_static");
                    }
                }
                catch (Exception)
                {
                    FontDataMajora2 = null;
                }
            }
            else
                FontDataMajora2 = _FontData2;

        }

        public Bitmap GetPreview(int BoxNum = 0, bool brightenText = true, float outputScale = 1.75f)
        {
            BrightenText = brightenText;

            if (InBombersNotebook)
            {
                OUTPUT_IMAGE_X = 280;
                OUTPUT_IMAGE_Y = 58;
            }
            else
            {
                switch (Header.BoxType)
                {
                    case DataMajora.BoxType.None_Black:
                    case DataMajora.BoxType.None_White:
                        {
                            OUTPUT_IMAGE_X = 320;
                            OUTPUT_IMAGE_Y = 72;
                            break;
                        }
                    case DataMajora.BoxType.Credits:
                        {
                            OUTPUT_IMAGE_X = 320;
                            OUTPUT_IMAGE_Y = 240;
                            break;
                        }
                    default:
                        {
                            OUTPUT_IMAGE_X = 256;
                            OUTPUT_IMAGE_Y = 72;
                            break;
                        }
                }
            }


            Bitmap bmp = new Bitmap(OUTPUT_IMAGE_X, OUTPUT_IMAGE_Y);

            bmp.MakeTransparent();
            bmp = DrawBox(bmp);

            if (Message.Count != 0)
                if (Message[BoxNum].Count != 0)
                    bmp = DrawText(bmp, BoxNum);

            bmp = Common.Resize(bmp, outputScale);

            return bmp;
        }

        private void SplitMsgIntoTextboxes(byte[] MessageDataMajora)
        {
            List<byte> box = new List<byte>();

            bool End = false;

            for (int i = 0; i < MessageDataMajora.Length; i++)
            {
                byte curByte = Common.GetByteFromArray(MessageDataMajora, i);

                switch (curByte)
                {
                    case (byte)DataMajora.MsgControlCode.NEW_BOX:
                    case (byte)DataMajora.MsgControlCode.NEW_BOX_INCOMPL:
                        {
                            box.Add(curByte);
                            Message.Add(box);
                            box = new List<byte>();

                            if (End)
                                return;
                            else
                                break;
                        }
                    case (byte)DataMajora.MsgControlCode.PERSISTENT:
                        {
                            box.Add(curByte);
                            Message.Add(box);
                            return;
                        }
                    case (byte)DataMajora.MsgControlCode.SHIFT:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 1));
                            i += 1;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.DELAY_END:
                        {
                            Message.Add(box);

                            return;
                        }
                    case (byte)DataMajora.MsgControlCode.DELAY:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 1));
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 2));
                            i += 2;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.DELAY_NEWBOX:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 1));
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 2));
                            Message.Add(box);
                            box = new List<byte>();
                            i += 2;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.FADE:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 1));
                            i += 2;
                            End = true;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.SOUND:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 1));
                            box.Add(Common.GetByteFromArray(MessageDataMajora, i + 2));
                            i += 2;
                            break;
                        }
                    default:
                        box.Add(curByte); break;

                }
            }

            if (box.Count != 0)
                Message.Add(box);
        }

        private int GetBoxChoiceTag(int BoxNum)
        {
            int Result = 0;

            List<byte> BoxDataMajora = Message[BoxNum];

            for (int i = 0; i < BoxDataMajora.Count; i++)
            {
                byte curByte = Common.GetByteFromArray(BoxDataMajora.ToArray(), i);

                if (curByte == (byte)DataMajora.MsgControlCode.TWO_CHOICES)
                {
                    Result = 2;
                    continue;
                }
                else if (curByte == (byte)DataMajora.MsgControlCode.THREE_CHOICES)
                {
                    Result = 3;
                    continue;
                }
                else
                {

                    switch (curByte)
                    {
                        case (byte)DataMajora.MsgControlCode.SHIFT:
                            i += 1; break;
                        case (byte)DataMajora.MsgControlCode.DELAY_END:
                        case (byte)DataMajora.MsgControlCode.DELAY:
                        case (byte)DataMajora.MsgControlCode.DELAY_NEWBOX:
                        case (byte)DataMajora.MsgControlCode.SOUND:
                            i += 2; break;
                        case (byte)DataMajora.MsgControlCode.BACKGROUND:
                            i += 3; break;
                        case (byte)DataMajora.MsgControlCode.FADE:
                            return Result;
                    }
                }
            }


            return Result;
        }

        private int GetNumberOfTags(int BoxNum, List<int> Tags)
        {
            int numTags = 0;

            if (BoxNum < 0)
                for (int i = 0; i < Message.Count; i++)
                    numTags += GetNumberOfTags(i, Tags);
            else
            {
                List<byte> BoxDataMajora = Message[BoxNum];

                for (int i = 0; i < BoxDataMajora.Count; i++)
                {
                    byte curByte = Common.GetByteFromArray(BoxDataMajora.ToArray(), i);

                    if (Tags.Contains(curByte))
                        numTags++;
                    else
                    {

                        switch (curByte)
                        {
                            case (byte)DataMajora.MsgControlCode.SHIFT:
                                i += 1; break;
                            case (byte)DataMajora.MsgControlCode.DELAY_END:
                            case (byte)DataMajora.MsgControlCode.DELAY:
                            case (byte)DataMajora.MsgControlCode.DELAY_NEWBOX:
                            case (byte)DataMajora.MsgControlCode.SOUND:
                            case (byte)DataMajora.MsgControlCode.FADE:
                                i += 2; break;
                        }
                    }
                }
            }

            return numTags;
        }

        private int GetNumberOfLineBreaks(int BoxNum)
        {
            int numTags = 0;

            if (BoxNum < 0)
                for (int i = 0; i < Message.Count; i++)
                    numTags += GetNumberOfLineBreaks(i);
            else
            {
                List<byte> BoxDataMajora = Message[BoxNum];

                for (int i = 0; i < BoxDataMajora.Count; i++)
                {
                    byte curByte = Common.GetByteFromArray(BoxDataMajora.ToArray(), i);

                    if (curByte == (byte)DataMajora.MsgControlCode.LINE_BREAK)
                        numTags++;
                    if (curByte == (byte)DataMajora.MsgControlCode.BOMBER_CODE)                                 // what the fuck
                        numTags++;
                    else if (curByte == (byte)DataMajora.MsgControlCode.NEW_BOX_INCOMPL)
                    {
                        numTags--;

                        if (numTags < 0)
                            numTags = 0;
                    }
                    else
                    {

                        switch (curByte)
                        {
                            case (byte)DataMajora.MsgControlCode.SHIFT:
                                i += 1; break;
                            case (byte)DataMajora.MsgControlCode.DELAY_END:
                            case (byte)DataMajora.MsgControlCode.DELAY:
                            case (byte)DataMajora.MsgControlCode.DELAY_NEWBOX:
                            case (byte)DataMajora.MsgControlCode.SOUND:
                            case (byte)DataMajora.MsgControlCode.FADE:
                                i += 2; break;
                        }
                    }
                }
            }

            return numTags;
        }

        private byte GetLastColorTag(int BoxNum)
        {
            if (BoxNum == 0)
                return (byte)DataMajora.MsgControlCode.COLOR_DEFAULT;
            else
            {
                for (int box = BoxNum - 1; box >= 0; box--)
                {
                    List<byte> BoxDataMajora = Message[box];

                    for (int i = BoxDataMajora.Count - 1; i >= 0; i--)
                    {
                        byte curByte = BoxDataMajora[i];

                        if (i > 0)
                        {
                            byte curBytePrev = BoxDataMajora[i - 1];

                            List<byte> MultiByteCodes = new List<byte>()
                            {
                                (byte)DataMajora.MsgControlCode.SHIFT
                            };


                            if (MultiByteCodes.Contains(curBytePrev))
                                continue;
                        }

                        if (i > 1)
                        {
                            byte curBytePrev = BoxDataMajora[i - 2];

                            List<byte> MultiByteCodes = new List<byte>()
                            {

                                (byte)DataMajora.MsgControlCode.DELAY_END,
                                (byte)DataMajora.MsgControlCode.DELAY,
                                (byte)DataMajora.MsgControlCode.DELAY_NEWBOX,
                                (byte)DataMajora.MsgControlCode.SOUND,
                                (byte)DataMajora.MsgControlCode.FADE
                            };


                            if (MultiByteCodes.Contains(curBytePrev))
                            {
                                i -= 2;
                                continue;
                            }
                        }

                        if (curByte <= (byte)DataMajora.MsgControlCode.COLOR_ORANGE)
                            return curByte;
                    }
                }
            }

            return (byte)DataMajora.MsgControlCode.COLOR_DEFAULT;
        }

        private Bitmap DrawBox(Bitmap destBmp)
        {
            Bitmap img;
            Color c;
            bool revAlpha;

            if (InBombersNotebook)
            {
                c = Color.White;
                img = Properties.Resources.Box_Default;
                img = Common.ReverseAlphaMask(img);
                img = Common.Colorize(img, c);

                destBmp = new Bitmap(280, 50);

                using (Graphics g = Graphics.FromImage(destBmp))
                {
                    img.SetResolution(g.DpiX, g.DpiY);
                    g.DrawImage(img, new Rectangle(0, 0, destBmp.Width / 2, destBmp.Height));
                    img = Common.FlipBitmapX_MonoSafe(img);
                    g.DrawImage(img, new Rectangle(destBmp.Width / 2, 0, destBmp.Width / 2, destBmp.Height));
                }

                return destBmp;
            }
            else
            {
                switch (Header.BoxType)
                {
                    default:
                        {
                            destBmp = new Bitmap(320, 240);
                            var g = Graphics.FromImage(destBmp);
                            g.FillRectangle(Brushes.Black, 0, 0, 320, 240);

                            return destBmp;
                        }
                    case DataMajora.BoxType.Black:
                    case DataMajora.BoxType.Black2:
                        {
                            img = Properties.Resources.Box_Default;
                            c = Color.FromArgb(170, 0, 0, 0);
                            revAlpha = true;
                            break;
                        }
                    case DataMajora.BoxType.Ocarina:
                        {
                            img = Properties.Resources.Box_Staff;
                            c = Color.FromArgb(180, 255, 0, 0);
                            revAlpha = false;
                            break;
                        }
                    case DataMajora.BoxType.Wooden:
                        {
                            img = Properties.Resources.Box_Wooden;
                            c = Color.FromArgb(230, 70, 50, 30);
                            revAlpha = false;
                            break;
                        }
                    case DataMajora.BoxType.Blue:
                    case DataMajora.BoxType.Blue2:
                        {
                            img = Properties.Resources.Box_Blue;
                            c = Color.FromArgb(170, 0, 10, 50);
                            revAlpha = true;
                            break;
                        }
                    case DataMajora.BoxType.Bombers_Notebook:
                        {
                            img = Properties.Resources.majora_Box_Bomber;
                            c = Color.FromArgb(170, 250, 253, 213);
                            revAlpha = true;
                            break;
                        }
                    case DataMajora.BoxType.Red:
                    case DataMajora.BoxType.Red2:
                        {
                            img = Properties.Resources.Box_Default;
                            c = Color.FromArgb(170, 255, 0, 0);
                            revAlpha = true;
                            break;
                        }
                    case DataMajora.BoxType.None:
                    case DataMajora.BoxType.None2:
                    case DataMajora.BoxType.None3:
                    case DataMajora.BoxType.None4:
                    case DataMajora.BoxType.None_White:
                    case DataMajora.BoxType.None_Black:
                        {
                            destBmp = new Bitmap(OUTPUT_IMAGE_X, OUTPUT_IMAGE_Y);
                            destBmp.MakeTransparent();
                            return destBmp;
                        }
                }
            }

            if (Common.tagExtend.ContainsKey(257))
            {
                object o = Common.tagExtend[257];
                MethodInfo mi = o.GetType().GetMethod("TagProcess");

                object ret = mi.Invoke(o, new object[] { this, destBmp, img, c, revAlpha, Header.BoxType });
                object[] result = (ret as object[]);

                destBmp = (Bitmap)result[0];
                img = (Bitmap)result[1];
                c = (Color)result[2];
                revAlpha = (bool)result[3];
            }

            destBmp = DrawBoxInternal(destBmp, img, c, revAlpha);

            return destBmp;
        }

        private Bitmap DrawBoxInternal(Bitmap destBmp, Bitmap srcBmp, Color cl, bool revAlpha = true)
        {
            if (revAlpha)
                srcBmp = Common.ReverseAlphaMask(srcBmp);

            srcBmp = Common.Colorize(srcBmp, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                srcBmp.SetResolution(g.DpiX, g.DpiY);
                g.DrawImage(srcBmp, 0, 0);

                srcBmp = Common.FlipBitmapX_MonoSafe(srcBmp);
                g.DrawImage(srcBmp, srcBmp.Width, 0);
            }

            return destBmp;
        }

        private int GetColorIndex(DataMajora.BoxType BoxType)
        {
            return (InBombersNotebook ? 3 : DataMajora.CharColorIndexes[BoxType]);
        }

        private void SetStartPosition(int NumLineBreaks, int choiceType, ref float xPos, ref float yPos, ref float scale)
        {
            switch (choiceType)
            {
                case 2:
                    {
                        if (InBombersNotebook)
                            xPos = DataMajora.XPOS_DEFAULT;
                        else
                        {
                            xPos = DataMajora.XPOS_DEFAULT;
                            yPos = 26 - (6 * NumLineBreaks);
                        }
                        break;
                    }
                case 3:
                    {
                        if (InBombersNotebook)
                            xPos = DataMajora.XPOS_DEFAULT + 13;
                        else
                        {
                            xPos = DataMajora.XPOS_DEFAULT + 22;
                            yPos = 26 - (6 * NumLineBreaks);
                        }
                        break;
                    }
                default:
                    {
                        if (InBombersNotebook)
                        {
                            DataMajora.XPOS_DEFAULT = 8;

                            xPos = DataMajora.XPOS_DEFAULT;
                            yPos = Math.Max(6, 18 - (6 * NumLineBreaks));
                        }
                        else
                        {
                            DataMajora.XPOS_DEFAULT = 32;

                            switch (Header.BoxType)
                            {
                                case DataMajora.BoxType.None_White:
                                    {
                                        yPos = 36;
                                        break;
                                    }
                                case DataMajora.BoxType.Ocarina:
                                    {
                                        yPos = 2;
                                        break;
                                    }
                                case DataMajora.BoxType.Credits:
                                    {
                                        xPos = 20;
                                        yPos = 48;
                                        scale = 0.85f;
                                        break;
                                    }
                                default:
                                    {
                                        yPos = Math.Max(DataMajora.YPOS_DEFAULT, ((52 - (Data.LINEBREAK_SIZE * NumLineBreaks)) / 2));
                                        break;
                                    }
                            }
                        }
                        break;
                    }
            }
        }

        private void DrawMajoraIcon(int NumLineBreaks, ref Bitmap destBmp, ref float xPos, ref float yPos)
        {
            if (Header.MajoraIcon != 0xFE)
            {
                string fn = $"majora_icon_{Header.MajoraIcon.ToString().ToLower()}";
                Bitmap img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

                if (InBombersNotebook)
                    yPos = Math.Max(6, 18 - (6 * NumLineBreaks));
                else
                    yPos = 26 - (6 * NumLineBreaks);

                if (img != null)
                {
                    if (InBombersNotebook)
                    {
                        float xPosIcon = DataMajora.XPOS_DEFAULT - 2;
                        float yPosIcon = 12;

                        if (img.Width == 24)
                        {
                            xPosIcon += 4;
                            yPosIcon += 4;
                        }

                        Common.DrawImage(destBmp, img, Color.White, Math.Max(16, img.Width - 8), Math.Max(16, img.Height - 8), ref xPosIcon, ref yPosIcon, 0, false);
                    }
                    else
                    {
                        float xPosIcon = DataMajora.XPOS_DEFAULT - 20;
                        float yPosIcon = Header.BoxType == DataMajora.BoxType.None_White ? 32 : 16;

                        if (img.Width == 24)
                        {
                            xPosIcon += 4;
                            yPosIcon += 4;
                        }

                        Common.DrawImage(destBmp, img, Color.White, img.Width, img.Height, ref xPosIcon, ref yPosIcon, 0, false);
                    }
                }

                xPos += (InBombersNotebook ? 0x1C : 0xE);
            }
        }

        private Bitmap DrawText(Bitmap destBmp, int boxNum)
        {
            List<byte> BoxDataMajora = Message[boxNum];

            float xPos = DataMajora.XPOS_DEFAULT;
            float yPos = DataMajora.YPOS_DEFAULT;
            float scale = DataMajora.SCALE_DEFAULT;

            int NumLineBreaks = GetNumberOfLineBreaks(boxNum);
            int NumCurrentLineBreak = 0;
            int choiceType = GetBoxChoiceTag(boxNum);

            SetStartPosition(NumLineBreaks, choiceType, ref xPos, ref yPos, ref scale);
            DrawMajoraIcon(NumLineBreaks, ref destBmp, ref xPos, ref yPos);

            byte colorIdx = GetLastColorTag(boxNum);
            RGB clInitial = DataMajora.CharColors[colorIdx][GetColorIndex(Header.BoxType)];
            Color textColor = Color.FromArgb(255, clInitial.R, clInitial.G, clInitial.B);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                for (int charPos = 0; charPos < BoxDataMajora.Count; charPos++)
                {
                    // Draw and colorize buttons
                    if (BoxDataMajora[charPos] >= 0xB0 && BoxDataMajora[charPos] <= 0xBC)
                    {
                        RGB clButtonTag = DataMajora.SpecificTagTextColor[(DataMajora.MsgControlCode)BoxDataMajora[charPos]][DataMajora.CharColorIndexes[Header.BoxType]];
                        Color textColorButtonTag = Color.FromArgb(255, clButtonTag.R, clButtonTag.G, clButtonTag.B);


                        destBmp = DrawTextInternal(destBmp, BoxDataMajora[charPos], textColorButtonTag, scale, ref xPos, ref yPos);
                        continue;
                    }
                    // Draw and colorize presets
                    else if (DataMajora.ControlCharPresets.ContainsKey((DataMajora.MsgControlCode)BoxDataMajora[charPos]))
                    {
                        string Setting = DataMajora.ControlCharPresets[(DataMajora.MsgControlCode)BoxDataMajora[charPos]];

                        foreach (char ch in Setting.ToCharArray())
                            DrawTextInternal(destBmp, (byte)ch, textColor, scale, ref xPos, ref yPos);

                        continue;
                    }
                    else
                    {
                        if (Common.tagExtend.ContainsKey(BoxDataMajora[charPos]))
                        {
                            object o = Common.tagExtend[BoxDataMajora[charPos]];
                            MethodInfo mi = o.GetType().GetMethod("TagProcess");

                            object ret = mi.Invoke(o, new object[] { this, destBmp, BoxDataMajora, textColor, scale, xPos, yPos, charPos, Header.BoxType });
                            object[] result = (ret as object[]);

                            destBmp = (Bitmap)result[0];
                            BoxDataMajora = (List<byte>)result[1];
                            textColor = (Color)result[2];
                            scale = (float)result[3];
                            xPos = (float)result[4];
                            yPos = (float)result[5];
                            charPos = (int)result[6];
                        }
                        else
                        {
                            switch (BoxDataMajora[charPos])
                            {
                                case (byte)DataMajora.MsgControlCode.TWO_CHOICES:
                                    {
                                        if (!InBombersNotebook)
                                        {
                                            Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                            float xPosChoice = 13;
                                            float yPosChoice = 25;

                                            if (NumLineBreaks >= 3)
                                                yPosChoice += 7;

                                            for (int ch = 0; ch < 2; ch++)
                                            {
                                                Common.DrawImage(destBmp, imgArrow, Color.RoyalBlue, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
                                                yPosChoice += DataMajora.LINEBREAK_SIZE;
                                            }
                                        }

                                        break;
                                    }
                                case (byte)DataMajora.MsgControlCode.THREE_CHOICES:
                                    {
                                        if (!InBombersNotebook)
                                        {
                                            Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                            float xPosChoice = 13;
                                            float yPosChoice = 13;

                                            if (NumLineBreaks >= 3)
                                                yPosChoice += 7;


                                            for (int ch = 0; ch < 3; ch++)
                                            {
                                                Common.DrawImage(destBmp, imgArrow, Color.RoyalBlue, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
                                                yPosChoice += DataMajora.LINEBREAK_SIZE;
                                            }
                                        }

                                        break;
                                    }
                                case (byte)DataMajora.MsgControlCode.BACKGROUND:
                                    {
                                        Bitmap left = Properties.Resources.xmes_left;
                                        Bitmap right = Properties.Resources.xmes_right;

                                        float xPosbg = 0;
                                        float yPosbg = 0;

                                        Common.DrawImage(destBmp, left, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);

                                        xPosbg += left.Width;

                                        Common.DrawImage(destBmp, right, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);
                                        continue;
                                    }
                                case (byte)DataMajora.MsgControlCode.SOUND:
                                case (byte)DataMajora.MsgControlCode.DELAY:
                                case (byte)DataMajora.MsgControlCode.DELAY_NEWBOX:
                                    {
                                        charPos += 2;
                                        continue;
                                    }
                                case (byte)DataMajora.MsgControlCode.DELAY_END:
                                    {
                                        charPos += 2;
                                        return destBmp;
                                    }
                                case (byte)DataMajora.MsgControlCode.FADE:
                                    {
                                        charPos += 2;
                                        return destBmp;
                                    }
                                case (byte)DataMajora.MsgControlCode.PERSISTENT:
                                    {
                                        return destBmp;
                                    }
                                case (byte)DataMajora.MsgControlCode.END:
                                case (byte)DataMajora.MsgControlCode.DC:
                                case (byte)DataMajora.MsgControlCode.DI:
                                case (byte)DataMajora.MsgControlCode.NOSKIP:
                                case (byte)DataMajora.MsgControlCode.NOSKIP_SOUND:
                                    continue;
                                case (byte)DataMajora.MsgControlCode.SHIFT:
                                    {
                                        byte num_shift = Common.GetByteFromArray(BoxDataMajora.ToArray(), charPos + 1);

                                        xPos += num_shift;
                                        charPos++;
                                        continue;
                                    }
                                case (byte)DataMajora.MsgControlCode.COLOR_DEFAULT:
                                case (byte)DataMajora.MsgControlCode.COLOR_RED:
                                case (byte)DataMajora.MsgControlCode.COLOR_GREEN:
                                case (byte)DataMajora.MsgControlCode.COLOR_BLUE:
                                case (byte)DataMajora.MsgControlCode.COLOR_YELLOW:
                                case (byte)DataMajora.MsgControlCode.COLOR_NAVY:
                                case (byte)DataMajora.MsgControlCode.COLOR_PINK:
                                case (byte)DataMajora.MsgControlCode.COLOR_SILVER:
                                case (byte)DataMajora.MsgControlCode.COLOR_ORANGE:
                                    {
                                        byte color_DataMajora_idx = (byte)(BoxDataMajora[charPos] - (byte)DataMajora.MsgControlCode.COLOR_DEFAULT);

                                        RGB cl = DataMajora.CharColors[color_DataMajora_idx][GetColorIndex(Header.BoxType)];
                                        textColor = Color.FromArgb(255, cl.R, cl.G, cl.B);

                                        break;
                                    }
                                case (byte)DataMajora.MsgControlCode.LINE_BREAK:
                                    {
                                        NumCurrentLineBreak++;
                                        yPos += DataMajora.LINEBREAK_SIZE;


                                        if (Header.MajoraIcon != 0xFE && NumCurrentLineBreak > 1 || Header.MajoraIcon != 0xFE && choiceType == 0)
                                            xPos = DataMajora.XPOS_DEFAULT + (InBombersNotebook ? 0x1C : 0xE);
                                        else
                                            xPos = DataMajora.XPOS_DEFAULT;


                                        if (choiceType == 2 && NumCurrentLineBreak >= (NumLineBreaks - 1))
                                            xPos = DataMajora.XPOS_DEFAULT + (InBombersNotebook ? 30 : 10);

                                        if (choiceType == 3 && NumCurrentLineBreak >= (NumLineBreaks - 2))
                                            xPos = DataMajora.XPOS_DEFAULT + (InBombersNotebook ? 13 : 22);

                                        continue;
                                    }
                                default:
                                    {
                                        bool drawNormal = true;

                                        if (Common.tagExtend.ContainsKey(300))
                                        {
                                            object o = Common.tagExtend[300];
                                            MethodInfo mi = o.GetType().GetMethod("TagProcess");

                                            object ret = mi.Invoke(o, new object[] { this, destBmp, BoxDataMajora, charPos, textColor, scale, xPos, yPos });
                                            object[] result = (ret as object[]);

                                            destBmp = (Bitmap)result[0];
                                            BoxDataMajora = (List<byte>)result[1];
                                            charPos = (int)result[2];
                                            textColor = (Color)result[3];
                                            scale = (float)result[4];
                                            xPos = (float)result[5];
                                            yPos = (float)result[6];
                                            drawNormal = (bool)result[7];
                                        }

                                        if (drawNormal)
                                            destBmp = DrawTextInternal(destBmp, BoxDataMajora[charPos], textColor, scale, ref xPos, ref yPos);
    
                                        break;
                                    }
                            }
                        }
                    }
                }

                if (GetNumberOfTags(boxNum,
                        new List<int>()
                        {
                            (byte)DataMajora.MsgControlCode.FADE,
                            (byte)DataMajora.MsgControlCode.DELAY_END,
                            (byte)DataMajora.MsgControlCode.TWO_CHOICES,
                            (byte)DataMajora.MsgControlCode.THREE_CHOICES,
                            (byte)DataMajora.MsgControlCode.PERSISTENT
                        }
                   ) == 0)
                {
                    Bitmap imgend;

                    if (Message.Count == boxNum + 1)
                        imgend = Properties.Resources.Box_End;
                    else
                        imgend = Properties.Resources.Box_Triangle;

                    float xPosEnd = 124;
                    float yPosEnd = 60;
                    Color endColor = Color.RoyalBlue;

                    if (Common.tagExtend.ContainsKey(256))
                    {
                        object o = Common.tagExtend[256];
                        MethodInfo mi = o.GetType().GetMethod("TagProcess");

                        object ret = mi.Invoke(o, new object[] { this, imgend, BoxDataMajora, endColor, xPosEnd, yPosEnd, InBombersNotebook, Header.BoxType });
                        object[] result = (ret as object[]);

                        imgend = (Bitmap)result[0];
                        BoxDataMajora = (List<byte>)result[1];
                        endColor = (Color)result[2];
                        xPosEnd = (float)result[3];
                        yPosEnd = (float)result[4];
                        InBombersNotebook = (bool)result[5];
                    }

                    if (InBombersNotebook)
                    {
                        // Dunno weird stuff happens here
                        return destBmp;
                    }

                    Common.DrawImage(destBmp, imgend, endColor, (int)(16 * scale), (int)(16 * scale), ref xPosEnd, ref yPosEnd, 0);
                }
            }

            return destBmp;
        }

        private Bitmap DrawTextInternal(Bitmap destBmp, byte Char, Color cl, float scale, ref float xPos, ref float yPos)
        {
            string fn = $"majora_char_{Char.ToString("X").ToLower()}";

            if (Char == ' ')
            {
                xPos += (InBombersNotebook ? 3.0f : 6.0f);
                return destBmp;
            }

            Bitmap img;

            if (FontDataMajora != null && (Char - ' ') * 128 < FontDataMajora.Length)
                img = Common.GetBitmapFromI4FontChar(FontDataMajora.Skip((Char - ' ') * 128).Take(128).ToArray());
            else
            {
                img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

                if (img == null)
                {
                    fn = $"char_{Char.ToString("X").ToLower()}";
                    img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

                    if (img == null)
                        return destBmp;
                }
            }

            img = Common.ReverseAlphaMask(img, BrightenText);
            Bitmap shadow = img;
            img = Common.Colorize(img, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                if (Header.BoxType != DataMajora.BoxType.None_Black &&
                    Header.BoxType != DataMajora.BoxType.Bombers_Notebook &&
                    !InBombersNotebook)
                {
                    shadow = Common.Colorize(shadow, Color.Black);
                    shadow.SetResolution(g.DpiX, g.DpiY);

                    g.DrawImage(shadow, new Rectangle((int)xPos + 1, (int)yPos + 1, (int)(16 * scale), (int)(16 * scale)));
                }

                img.SetResolution(g.DpiX, g.DpiY);
                g.DrawImage(img, new Rectangle((int)xPos, (int)yPos, (int)(16 * scale), (int)(16 * scale)));
            }

            try
            {
                xPos += (int)(DataMajora.FontWidths[Char - 0x20] * scale);
            }
            catch (Exception)
            {
                // Lazy way to ensure the program does not crash if exported message DataMajora doesn't have DataMajora for all the characters.
            }

            return destBmp;
        }
    }
}
