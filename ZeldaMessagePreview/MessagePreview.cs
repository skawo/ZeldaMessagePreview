using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaMessage
{
    public class MessagePreview
    {
        private readonly Data.BoxType Box;
        private readonly List<List<byte>> Message = new List<List<byte>>();
        public int MessageCount;
        public bool BrightenText;

        private int OUTPUT_IMAGE_X = 256;
        private int OUTPUT_IMAGE_Y = 64 + (Properties.Resources.Box_End.Width / 2);

        public byte[] FontData = null;

        public MessagePreview(Data.BoxType BoxType, byte[] MessageData)
        {
            Box = BoxType;
            SplitMsgIntoTextboxes(MessageData);

            MessageCount = Message.Count;

            try
            {
                if (System.IO.File.Exists("font.width_table"))
                {
                    byte[] widths = System.IO.File.ReadAllBytes("font.width_table");

                    for (int i = 0; i < widths.Length; i += 4)
                    {
                        byte[] width = widths.Skip(i).Take(4).Reverse().ToArray();
                        Data.FontWidths[i / 4] = BitConverter.ToSingle(width, 0);
                    }
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (System.IO.File.Exists("font.font_static"))
                {
                    FontData = System.IO.File.ReadAllBytes("font.font_static");
                }
            }
            catch (Exception)
            {
                FontData = null;
            }
        }

        public Bitmap GetPreview(int BoxNum = 0, bool brightenText = true, float outputScale = 1.75f)
        {

            BrightenText = brightenText;

            if ((int)Box == (int)Data.BoxType.None_White || (int)Box == (int)Data.BoxType.None_Black)
            {
                OUTPUT_IMAGE_X = 320;
                OUTPUT_IMAGE_Y = 64 + 8;
            }
            else if ((int)Box > (int)Data.BoxType.None_Black)
            {
                OUTPUT_IMAGE_X = 320;
                OUTPUT_IMAGE_Y = 240;
            }
            else
            {
                OUTPUT_IMAGE_X = 256;
                OUTPUT_IMAGE_Y = 64 + 8;
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

        private void SplitMsgIntoTextboxes(byte[] MessageData)
        {
            List<byte> box = new List<byte>();

            bool End = false;

            for (int i = 0; i < MessageData.Length; i++)
            {
                byte curByte = Common.GetByteFromArray(MessageData, i);

                switch (curByte)
                {
                    case (byte)Data.MsgControlCode.NEW_BOX:
                        {
                            Message.Add(box);
                            box = new List<byte>();

                            if (End)
                                return;
                            else
                                break;
                        }
                    case (byte)Data.MsgControlCode.DELAY:
                        {
                            Message.Add(box);
                            box = new List<byte>();

                            i += 1;
                            break;
                        }
                    case (byte)Data.MsgControlCode.HIGH_SCORE:
                    case (byte)Data.MsgControlCode.SPEED:
                    case (byte)Data.MsgControlCode.SHIFT:
                    case (byte)Data.MsgControlCode.COLOR:
                    case (byte)Data.MsgControlCode.ICON:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageData, i + 1));
                            i += 1;
                            break;
                        }
                    case (byte)Data.MsgControlCode.FADE:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageData, i + 1));
                            i += 1;
                            End = true;
                            break;
                        }
                    case (byte)Data.MsgControlCode.JUMP:
                    case (byte)Data.MsgControlCode.FADE2:
                    case (byte)Data.MsgControlCode.SOUND:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageData, i + 1));
                            box.Add(Common.GetByteFromArray(MessageData, i + 2));
                            i += 2;
                            break;
                        }
                    case (byte)Data.MsgControlCode.BACKGROUND:
                        {
                            box.Add(curByte);
                            box.Add(Common.GetByteFromArray(MessageData, i + 1));
                            box.Add(Common.GetByteFromArray(MessageData, i + 2));
                            box.Add(Common.GetByteFromArray(MessageData, i + 3));
                            i += 3;
                            break;
                        }
                    default:
                        box.Add(curByte); break;

                }
            }

            if (box.Count != 0)
                Message.Add(box);
        }

        private int GetBoxIconTag(int BoxNum)
        {
            int Result = -1;

            List<byte> BoxData = Message[BoxNum];

            for (int i = 0; i < BoxData.Count; i++)
            {
                byte curByte = Common.GetByteFromArray(BoxData.ToArray(), i);

                if (curByte == (byte)Data.MsgControlCode.ICON)
                {
                    Result = Common.GetByteFromArray(BoxData.ToArray(), i + 1);
                    continue;
                }
                else
                {
                    switch (curByte)
                    {
                        case (byte)Data.MsgControlCode.HIGH_SCORE:
                        case (byte)Data.MsgControlCode.DELAY:
                        case (byte)Data.MsgControlCode.SPEED:
                        case (byte)Data.MsgControlCode.SHIFT:
                        case (byte)Data.MsgControlCode.COLOR:
                            i += 1; break;
                        case (byte)Data.MsgControlCode.JUMP:
                        case (byte)Data.MsgControlCode.SOUND:
                            i += 2; break;
                        case (byte)Data.MsgControlCode.BACKGROUND:
                            i += 3; break;
                        case (byte)Data.MsgControlCode.FADE:
                        case (byte)Data.MsgControlCode.FADE2:
                            return Result;
                    }
                }
            }


            return Result;
        }

        private int GetBoxChoiceTag(int BoxNum)
        {
            int Result = 0;

            List<byte> BoxData = Message[BoxNum];

            for (int i = 0; i < BoxData.Count; i++)
            {
                byte curByte = Common.GetByteFromArray(BoxData.ToArray(), i);

                if (curByte == (byte)Data.MsgControlCode.TWO_CHOICES)
                {
                    Result = 2; 
                    continue;
                }
                else if (curByte == (byte)Data.MsgControlCode.THREE_CHOICES)
                {
                    Result = 3;
                    continue;
                }
                else
                {

                    switch (curByte)
                    {
                        case (byte)Data.MsgControlCode.HIGH_SCORE:
                        case (byte)Data.MsgControlCode.DELAY:
                        case (byte)Data.MsgControlCode.SPEED:
                        case (byte)Data.MsgControlCode.SHIFT:
                        case (byte)Data.MsgControlCode.COLOR:
                        case (byte)Data.MsgControlCode.ICON:
                            i += 1; break;
                        case (byte)Data.MsgControlCode.JUMP:
                        case (byte)Data.MsgControlCode.SOUND:
                            i += 2; break;
                        case (byte)Data.MsgControlCode.BACKGROUND:
                            i += 3; break;
                        case (byte)Data.MsgControlCode.FADE:
                        case (byte)Data.MsgControlCode.FADE2:
                            return Result;
                    }
                }
            }


            return Result;
        }

        private int GetNumberOfTags(int BoxNum, int Tag)
        {
            int numTags = 0;

            if (BoxNum < 0)
                for (int i = 0; i < Message.Count; i++)
                    numTags += GetNumberOfTags(i, Tag);
            else
            {
                List<byte> BoxData = Message[BoxNum];

                for (int i = 0; i < BoxData.Count; i++)
                {
                    byte curByte = Common.GetByteFromArray(BoxData.ToArray(), i);

                    if (curByte == (byte)Tag)
                        numTags++;
                    else
                    {

                        switch (curByte)
                        {
                            case (byte)Data.MsgControlCode.HIGH_SCORE:
                            case (byte)Data.MsgControlCode.DELAY:
                            case (byte)Data.MsgControlCode.SPEED:
                            case (byte)Data.MsgControlCode.SHIFT:
                            case (byte)Data.MsgControlCode.COLOR:
                            case (byte)Data.MsgControlCode.ICON:
                            case (byte)Data.MsgControlCode.FADE:
                                i += 1; break;
                            case (byte)Data.MsgControlCode.JUMP:
                            case (byte)Data.MsgControlCode.SOUND:
                            case (byte)Data.MsgControlCode.FADE2:
                                i += 2; break;
                            case (byte)Data.MsgControlCode.BACKGROUND:
                                i += 3; break;
                        }
                    }
                }
            }

            return numTags;
        }

        private Bitmap DrawBox(Bitmap destBmp)
        {
            Bitmap img = Properties.Resources.Box_Default;
            Color c = Color.Black;
            bool revAlpha = true;

            switch (Box)
            {
                default:
                    {
                        destBmp = new Bitmap(320, 240);
                        var g = Graphics.FromImage(destBmp);
                        g.FillRectangle(Brushes.Black, 0, 0, 320, 240);

                        return destBmp;
                    }
                case Data.BoxType.Black:
                    {
                        img = Properties.Resources.Box_Default;
                        c = Color.FromArgb(170, 0, 0, 0);
                        revAlpha = true;
                        break;
                    }
                case Data.BoxType.Ocarina:
                    {
                        img = Properties.Resources.Box_Staff;
                        c = Color.FromArgb(180, 255, 0, 0);
                        revAlpha = false;
                        break;
                    }
                case Data.BoxType.Wooden:
                    {
                        img = Properties.Resources.Box_Wooden;
                        c = Color.FromArgb(230, 70, 50, 30);
                        revAlpha = false;
                        break;
                    }
                case Data.BoxType.Blue:
                    {
                        img = Properties.Resources.Box_Blue;
                        c = Color.FromArgb(170, 0, 10, 50);
                        revAlpha = true;
                        break;
                    }
                case Data.BoxType.None_White:
                case Data.BoxType.None_Black:
                    {
                        destBmp = new Bitmap(OUTPUT_IMAGE_X, OUTPUT_IMAGE_Y);
                        destBmp.MakeTransparent();
                        return destBmp;
                    }
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

                //srcBmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                g.DrawImage(srcBmp, srcBmp.Width, 0);
            }

            return destBmp;
        }

        private Bitmap DrawText(Bitmap destBmp, int boxNum)
        {
            List<byte> BoxData = Message[boxNum];

            float xPos = Data.XPOS_DEFAULT;
            float yPos = (Box == Data.BoxType.None_White) ? 36 : Math.Max(Data.YPOS_DEFAULT, ((52 - (Data.LINEBREAK_SIZE * GetNumberOfTags(boxNum, (int)Data.MsgControlCode.LINE_BREAK))) / 2));
            float scale = Data.SCALE_DEFAULT;

            if ((int)Box > (int)Data.BoxType.None_Black)
            {
                xPos = 20;
                yPos = 48;
                scale = 0.85f;
            }

            Color textColor = (Box == Data.BoxType.None_Black) ? Color.Black : Color.White;

            int choiceType = GetBoxChoiceTag(boxNum);
            int iconType = GetBoxIconTag(boxNum);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                for (int charPos = 0; charPos < BoxData.Count; charPos++)
                {
                    switch (BoxData[charPos])
                    {
                        case (byte)Data.MsgControlCode.TWO_CHOICES:
                            {
                                Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                float xPosChoice = 16;
                                float yPosChoice = 32;

                                for (int ch = 0; ch < 2; ch++)
                                {
                                    Common.DrawImage(destBmp, imgArrow, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
                                    yPosChoice += Data.LINEBREAK_SIZE;
                                }

                                break;
                            }
                        case (byte)Data.MsgControlCode.THREE_CHOICES:
                            {
                                Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                float xPosChoice = 16;
                                float yPosChoice = 20;

                                for (int ch = 0; ch < 3; ch++)
                                {
                                    Common.DrawImage(destBmp, imgArrow, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
                                    yPosChoice += Data.LINEBREAK_SIZE;
                                }

                                break;
                            }
                        case (byte)Data.MsgControlCode.TIME:
                            {
                                char[] Setting = Data.ControlCharPresets[(Data.MsgControlCode)BoxData[charPos]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, textColor, scale, ref xPos, ref yPos);

                                break;
                            }
                        case (byte)Data.MsgControlCode.POINTS:
                        case (byte)Data.MsgControlCode.MARATHON_TIME:
                        case (byte)Data.MsgControlCode.RACE_TIME:
                        case (byte)Data.MsgControlCode.FISH_WEIGHT:
                        case (byte)Data.MsgControlCode.GOLD_SKULLTULAS:
                        case (byte)Data.MsgControlCode.PLAYER:
                            {
                                char[] Setting = Data.ControlCharPresets[(Data.MsgControlCode)BoxData[charPos]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, textColor, scale, ref xPos, ref yPos);

                                break;
                            }
                        case (byte)Data.MsgControlCode.HIGH_SCORE:
                            {
                                char[] Setting = Data.HighScoreControlCharPresets[(Data.MsgHighScore)BoxData[charPos + 1]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, textColor, scale, ref xPos, ref yPos);

                                charPos++;
                                break;
                            }
                        case (byte)Data.MsgControlCode.ICON:
                            {
                                byte IconN = Common.GetByteFromArray(BoxData.ToArray(), charPos + 1);

                                string fn = $"icon_{IconN.ToString().ToLower()}";
                                Bitmap img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

                                if (img != null)
                                {
                                    if (IconN < 102)
                                    {
                                        float xPosIcon = xPos - 0xA;
                                        float yPosIcon = Box == Data.BoxType.None_White ? 36 : 0x10;

                                        Common.DrawImage(destBmp, img, Color.White, 32, 32, ref xPosIcon, ref yPosIcon, 0, false);
                                    }
                                    else
                                    {
                                        float xPosIcon = xPos - 0x7;
                                        float yPosIcon = Box == Data.BoxType.None_White ? 36 : 0x14;

                                        Common.DrawImage(destBmp, img, Color.White, 24, 24, ref xPosIcon, ref yPosIcon, 0, false);
                                    }
                                }

                                xPos += 0x20;
                                charPos += 1;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.EVENT:
                            {
                                break;
                            }
                        case (byte)Data.MsgControlCode.PERSISTENT:
                            {
                                return destBmp;
                            }
                        case (byte)Data.MsgControlCode.BACKGROUND:
                            {
                                Bitmap left = Properties.Resources.xmes_left;
                                Bitmap right = Properties.Resources.xmes_right;

                                float xPosbg = 0;
                                float yPosbg = 0;

                                Common.DrawImage(destBmp, left, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);

                                xPosbg += left.Width;

                                Common.DrawImage(destBmp, right, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);

                                charPos += 3;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.SOUND:
                        case (byte)Data.MsgControlCode.FADE2:
                            {
                                charPos += 2;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.DELAY:
                        case (byte)Data.MsgControlCode.JUMP:
                            {
                                charPos += 2;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.SPEED:
                            {
                                charPos += 1;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.FADE:
                            {
                                charPos += 1;
                                return destBmp;
                            }
                        case (byte)Data.MsgControlCode.AWAIT_BUTTON:
                        case (byte)Data.MsgControlCode.END:
                        case (byte)Data.MsgControlCode.DC:
                        case (byte)Data.MsgControlCode.DI:
                        case (byte)Data.MsgControlCode.NS:
                            continue;
                        case (byte)Data.MsgControlCode.SHIFT:
                            {
                                byte num_shift = Common.GetByteFromArray(BoxData.ToArray(), charPos + 1);

                                xPos += num_shift;
                                charPos++;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.COLOR:
                            {
                                byte color_data_idx = Common.GetByteFromArray(BoxData.ToArray(), charPos + 1);

                                switch (color_data_idx)
                                {
                                    case (int)Data.MsgColor.R:
                                    case (int)Data.MsgColor.G:
                                    case (int)Data.MsgColor.B:
                                    case (int)Data.MsgColor.C:
                                    case (int)Data.MsgColor.M:
                                    case (int)Data.MsgColor.Y:
                                    case (int)Data.MsgColor.BLK:
                                        {
                                            RGB cl = Data.CharColors[color_data_idx - (int)Data.MsgColor.R][Convert.ToInt32(Box == Data.BoxType.Wooden)];
                                            textColor = Color.FromArgb(255, cl.R, cl.G, cl.B);
                                            break;
                                        }
                                    default:
                                        {
                                            RGB cl = Data.CharColors[7][Convert.ToInt32(Box == Data.BoxType.None_Black)];
                                            textColor = Color.FromArgb(255, cl.R, cl.G, cl.B);
                                            break;
                                        }
                                }

                                charPos++;

                                break;
                            }
                        case (byte)Data.MsgControlCode.LINE_BREAK:
                            {
                                if ((int)Box > (int)Data.BoxType.None_Black)
                                {
                                    xPos = 20;
                                    yPos += 6;
                                }
                                else
                                {
                                    xPos = Data.XPOS_DEFAULT;
                                    yPos += Data.LINEBREAK_SIZE;
                                }

                                if ((choiceType == 2 && yPos >= 32) || (choiceType == 3 && yPos >= 20) || iconType != -1 && yPos > 12)
                                    xPos = 2 * Data.XPOS_DEFAULT;

                                continue;
                            }
                        default:
                            {
                                destBmp = DrawTextInternal(destBmp, BoxData[charPos], textColor, scale, ref xPos, ref yPos);
                                break;
                            }
                    }
                }

                if (GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.FADE) == 0 &&
                    GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.DELAY) == 0 &&
                    GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.FADE2) == 0 &&
                    GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.TWO_CHOICES) == 0 &&
                    GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.THREE_CHOICES) == 0 &&
                    GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.PERSISTENT) == 0 &&
                    GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.EVENT) == 0)
                {
                    Bitmap imgend;

                    if (Message.Last() == BoxData)
                        imgend = Properties.Resources.Box_End;
                    else
                        imgend = Properties.Resources.Box_Triangle;

                    float xPosEnd = 128 - 4;
                    float yPosEnd = 64 - 4;

                    Common.DrawImage(destBmp, imgend, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosEnd, ref yPosEnd, 0);
                }
            }

            return destBmp;
        }

        private Bitmap DrawTextInternal(Bitmap destBmp, byte Char, Color cl, float scale, ref float xPos, ref float yPos)
        {
            string fn = $"char_{Char.ToString("X").ToLower()}";

            if (Char == ' ')
            {
                xPos += 6.0f;
                return destBmp;
            }

            Bitmap img;

            if (FontData != null && (Char - ' ') * 128 < FontData.Length)
                img = Common.GetBitmapFromI4FontChar(FontData.Skip((Char - ' ') * 128).Take(128).ToArray());
            else
            {
                img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

                if (img == null)
                    return destBmp;
            }

            img = Common.ReverseAlphaMask(img, BrightenText);

            Bitmap shadow = img;

            img = Common.Colorize(img, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                if (Box != Data.BoxType.None_Black)
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
                xPos += (int)Math.Floor((Data.FontWidths[Char - 0x20] * scale));
            }
            catch (Exception)
            {
                // Lazy way to ensure the program does not crash if exported message data doesn't have data for all the characters.
            }

            return destBmp;
        }
    }
}
