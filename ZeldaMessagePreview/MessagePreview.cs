using System;
using System.Collections.Generic;
using System.Drawing;
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

        private int OUTPUT_IMAGE_X = 256;
        private int OUTPUT_IMAGE_Y = 64 + (Properties.Resources.Box_End.Width / 2);

        public MessagePreview(Data.BoxType BoxType, byte[] MessageData)
        {
            Box = BoxType;
            SplitMsgIntoTextboxes(MessageData);

            MessageCount = Message.Count;
        }

        public Bitmap GetPreview(int BoxNum = 0, float outputScale = 1.75f)
        {

            if ((int)Box >= (int)Data.BoxType.None_White)
            {
                OUTPUT_IMAGE_X = 320;
                OUTPUT_IMAGE_Y = 64 + 8;
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

            bmp = Resize(bmp, outputScale);

            return bmp;
        }

        private void SplitMsgIntoTextboxes(byte[] MessageData)
        {
            List<byte> box = new List<byte>();

            bool End = false;

            for (int i = 0; i < MessageData.Length; i++)
            {
                byte curByte = GetByteFromArray(MessageData, i);

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
                    case (byte)Data.MsgControlCode.POINTS:
                    case (byte)Data.MsgControlCode.MARATHON_TIME:
                    case (byte)Data.MsgControlCode.RACE_TIME:
                    case (byte)Data.MsgControlCode.SPEED:
                    case (byte)Data.MsgControlCode.SHIFT:
                    case (byte)Data.MsgControlCode.COLOR:
                    case (byte)Data.MsgControlCode.JUMP:
                    case (byte)Data.MsgControlCode.ICON:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageData, i + 1));
                            i += 1;
                            break;
                        }
                    case (byte)Data.MsgControlCode.FADE:
                    case (byte)Data.MsgControlCode.FADE2:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageData, i + 1));
                            i += 1;
                            End = true;
                            break;
                        }
                    case (byte)Data.MsgControlCode.SOUND:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageData, i + 1));
                            box.Add(GetByteFromArray(MessageData, i + 2));
                            i += 2;
                            break;
                        }
                    case (byte)Data.MsgControlCode.BACKGROUND:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageData, i + 1));
                            box.Add(GetByteFromArray(MessageData, i + 2));
                            box.Add(GetByteFromArray(MessageData, i + 3));
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
                byte curByte = GetByteFromArray(BoxData.ToArray(), i);

                if (curByte == (byte)Data.MsgControlCode.ICON)
                {
                    Result = GetByteFromArray(BoxData.ToArray(), i + 1);
                    continue;
                }
                else
                {
                    switch (curByte)
                    {
                        case (byte)Data.MsgControlCode.POINTS:
                        case (byte)Data.MsgControlCode.MARATHON_TIME:
                        case (byte)Data.MsgControlCode.RACE_TIME:
                        case (byte)Data.MsgControlCode.DELAY:
                        case (byte)Data.MsgControlCode.SPEED:
                        case (byte)Data.MsgControlCode.SHIFT:
                        case (byte)Data.MsgControlCode.COLOR:
                        case (byte)Data.MsgControlCode.JUMP:
                            i += 1; break;
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
                byte curByte = GetByteFromArray(BoxData.ToArray(), i);

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
                        case (byte)Data.MsgControlCode.POINTS:
                        case (byte)Data.MsgControlCode.MARATHON_TIME:
                        case (byte)Data.MsgControlCode.RACE_TIME:
                        case (byte)Data.MsgControlCode.DELAY:
                        case (byte)Data.MsgControlCode.SPEED:
                        case (byte)Data.MsgControlCode.SHIFT:
                        case (byte)Data.MsgControlCode.COLOR:
                        case (byte)Data.MsgControlCode.JUMP:
                        case (byte)Data.MsgControlCode.ICON:
                            i += 1; break;
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
                    byte curByte = GetByteFromArray(BoxData.ToArray(), i);

                    if (curByte == (byte)Tag)
                        numTags++;
                    else
                    {

                        switch (curByte)
                        {
                            case (byte)Data.MsgControlCode.POINTS:
                            case (byte)Data.MsgControlCode.MARATHON_TIME:
                            case (byte)Data.MsgControlCode.RACE_TIME:
                            case (byte)Data.MsgControlCode.DELAY:
                            case (byte)Data.MsgControlCode.SPEED:
                            case (byte)Data.MsgControlCode.SHIFT:
                            case (byte)Data.MsgControlCode.COLOR:
                            case (byte)Data.MsgControlCode.JUMP:
                            case (byte)Data.MsgControlCode.ICON:
                            case (byte)Data.MsgControlCode.FADE:
                            case (byte)Data.MsgControlCode.FADE2:
                                i += 1; break;
                            case (byte)Data.MsgControlCode.SOUND:
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
                srcBmp = ReverseAlphaMask(srcBmp);

            srcBmp = Colorize(srcBmp, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                srcBmp.SetResolution(g.DpiX, g.DpiY);
                g.DrawImage(srcBmp, 0, 0);

                srcBmp = FlipBitmapX_MonoSafe(srcBmp);

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
                                    DrawImage(destBmp, imgArrow, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
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
                                    DrawImage(destBmp, imgArrow, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
                                    yPosChoice += Data.LINEBREAK_SIZE;
                                }

                                break;
                            }
                        case (byte)Data.MsgControlCode.POINTS:
                        case (byte)Data.MsgControlCode.MARATHON_TIME:
                        case (byte)Data.MsgControlCode.RACE_TIME:
                            {
                                char[] Setting = Data.ControlCharPresets[(Data.MsgControlCode)BoxData[charPos]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, textColor, scale, ref xPos, ref yPos);

                                charPos++;
                                break;
                            }
                        case (byte)Data.MsgControlCode.FISH_WEIGHT:
                        case (byte)Data.MsgControlCode.GOLD_SKULLTULAS:
                        case (byte)Data.MsgControlCode.PLAYER:
                            {
                                char[] Setting = Data.ControlCharPresets[(Data.MsgControlCode)BoxData[charPos]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, textColor, scale, ref xPos, ref yPos);

                                break;
                            }
                        case (byte)Data.MsgControlCode.ICON:
                            {
                                byte IconN = GetByteFromArray(BoxData.ToArray(), charPos + 1);

                                string fn = $"icon_{IconN.ToString().ToLower()}";
                                Bitmap img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

                                if (img != null)
                                {
                                    if (IconN < 102)
                                    {
                                        float xPosIcon = xPos - 0xA;
                                        float yPosIcon = Box == Data.BoxType.None_White ? 36 : 0x10;

                                        DrawImage(destBmp, img, Color.White, 32, 32, ref xPosIcon, ref yPosIcon, 0, false);
                                    }
                                    else
                                    {
                                        float xPosIcon = xPos - 0x7;
                                        float yPosIcon = Box == Data.BoxType.None_White ? 36 : 0x14;

                                        DrawImage(destBmp, img, Color.White, 24, 24, ref xPosIcon, ref yPosIcon, 0, false);
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
                        case (byte)Data.MsgControlCode.BACKGROUND:
                            {
                                Bitmap left = Properties.Resources.xmes_left;
                                Bitmap right = Properties.Resources.xmes_right;

                                float xPosbg = 0;
                                float yPosbg = 0;

                                DrawImage(destBmp, left, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);

                                xPosbg += left.Width;

                                DrawImage(destBmp, right, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);

                                charPos += 3;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.SOUND:
                            {
                                charPos += 2;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.DELAY:
                        case (byte)Data.MsgControlCode.SPEED:
                        case (byte)Data.MsgControlCode.JUMP:
                            {
                                charPos += 1;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.FADE:
                        case (byte)Data.MsgControlCode.FADE2:
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
                                byte num_shift = GetByteFromArray(BoxData.ToArray(), charPos + 1);

                                xPos += num_shift;
                                charPos++;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.COLOR:
                            {
                                byte color_data_idx = GetByteFromArray(BoxData.ToArray(), charPos + 1);

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
                                xPos = Data.XPOS_DEFAULT;
                                yPos += Data.LINEBREAK_SIZE;

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
                    GetNumberOfTags(boxNum, (byte)Data.MsgControlCode.EVENT) == 0)
                {
                    Bitmap imgend;

                    if (Message.Last() == BoxData)
                        imgend = Properties.Resources.Box_End;
                    else
                        imgend = Properties.Resources.Box_Triangle;

                    float xPosEnd = 128 - 4;
                    float yPosEnd = 64 - 4;

                    DrawImage(destBmp, imgend, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosEnd, ref yPosEnd, 0);
                }
            }

            return destBmp;
        }

        private Bitmap DrawTextInternal(Bitmap destBmp, byte Char, Color cl, float scale, ref float xPos, ref float yPos)
        {
            string fn = $"char_{Char.ToString("X").ToLower()}";
            Bitmap img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

            if (img == null)
                return destBmp;

            img = ReverseAlphaMask(img);

            Bitmap shadow = img;

            img = Colorize(img, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                if (Box != Data.BoxType.None_Black)
                {
                    shadow = Colorize(shadow, Color.Black);
                    shadow.SetResolution(g.DpiX, g.DpiY);

                    g.DrawImage(shadow, new Rectangle((int)xPos + 1, (int)yPos + 1, (int)(16 * scale), (int)(16 * scale)));
                }

                img.SetResolution(g.DpiX, g.DpiY);
                g.DrawImage(img, new Rectangle((int)xPos, (int)yPos, (int)(16 * scale), (int)(16 * scale)));
            }

            xPos += (int)Math.Floor((Data.FontWidths[Char - 0x20] * scale));
            return destBmp;
        }

        private Bitmap DrawImage(Bitmap destBmp, Bitmap srcBmp, Color cl, int xSize, int ySize, ref float xPos, ref float yPos, float xPosMove, bool revAlpha = true)
        {
            if (revAlpha)
                srcBmp = ReverseAlphaMask(srcBmp);

            srcBmp = Colorize(srcBmp, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                srcBmp.SetResolution(g.DpiX, g.DpiY);
                g.DrawImage(srcBmp, new Rectangle((int)xPos, (int)yPos, xSize, ySize));
            }

            xPos += xPosMove;
            return destBmp;
        }

        private Bitmap FlipBitmapX_MonoSafe(Bitmap bmp)
        {
            Bitmap returnBitmap = new Bitmap(bmp.Width, bmp.Height);

            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                g.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
                g.ScaleTransform(-1, 1);
                g.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
                g.DrawImage(bmp, new Point(0, 0));
            }

            return returnBitmap;
        }

        private Bitmap ReverseAlphaMask(Bitmap bmp)
        {
            bmp.MakeTransparent();

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbaValues = new byte[bytes];

            Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);

            for (int i = 3; i < rgbaValues.Length; i += 4)
                rgbaValues[i] = rgbaValues[i - 3];

            Marshal.Copy(rgbaValues, 0, bmpData.Scan0, bytes);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private Bitmap Resize(Bitmap bmp, float scale)
        {
            Bitmap result = new Bitmap((int)(bmp.Width * scale), (int)(bmp.Height * scale));

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(bmp, 0, 0, (int)(bmp.Width * scale), (int)(bmp.Height * scale));
            }

            return result;
        }

        private Bitmap Colorize(Bitmap bmp, Color cl)
        {
            float R = (float)((float)cl.R / (float)255);
            float G = (float)((float)cl.G / (float)255);
            float B = (float)((float)cl.B / (float)255);
            float A = 1;


            float[][] colorMatrixElements =
            {
                new float[] {R,  0,  0,  0,  0},
                new float[] {0,  G,  0,  0,  0},
                new float[] {0,  0,  B,  0,  0},
                new float[] {0,  0,  0,  A,  0},
                new float[] {0,  0,  0,  0,  0}
            };

            ColorMatrix cm = new ColorMatrix(colorMatrixElements);

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            Bitmap bm = new Bitmap(bmp.Width, bmp.Height);

            using (Graphics gg = Graphics.FromImage(bm))
                gg.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imageAttributes);

            return bm;
        }

        private byte GetByteFromArray(byte[] array, int i)
        {
            byte outB = 0;

            if (i <= array.Length - 1)
                outB = array[i];

            return outB;
        }
    }

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
}
