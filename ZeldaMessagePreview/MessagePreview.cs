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

        public MessagePreview(Data.BoxType BoxType, byte[] MessageData)
        {
            Box = BoxType;

            List<byte> box = new List<byte>();

            for (int i = 0; i < MessageData.Length; i++)
            {
                if (MessageData[i] == (byte)Data.MsgControlCode.NEW_BOX)
                {
                    Message.Add(box);
                    box = new List<byte>();
                }
                else if (MessageData[i] == (byte)Data.MsgControlCode.DELAY)
                {
                    box.Add(MessageData[i + 1]);
                    Message.Add(box);
                    box = new List<byte>();
                }
                else
                    box.Add(MessageData[i]);
            }

            if (box.Count != 0)
                Message.Add(box);

            MessageCount = Message.Count;
        }

        public Bitmap GetPreview(int BoxNum = 0)
        {
            Bitmap bmp = new Bitmap(Data.OUTPUT_IMAGE_X, Data.OUTPUT_IMAGE_Y);
            bmp.MakeTransparent();

            bmp = DrawBox(bmp);

            if (Message.Count != 0)
                if (Message[BoxNum].Count != 0)
                    bmp = DrawText(bmp, BoxNum);

            bmp = Resize(bmp, 1.5f);

            return bmp;
        }

        private int FindNumOfTag(int BoxNum, int Tag)
        {
            int numTags = 0;

            if (BoxNum < 0)
                for (int i = 0; i < Message.Count; i++)
                    numTags += FindNumOfTag(i, Tag);
            else
            {
                List<byte> BoxData = Message[BoxNum];

                for (int i = 0; i < BoxData.Count; i++)
                {
                    if (BoxData[i] == (byte)Tag)
                        numTags++;
                    else
                    {

                        switch (BoxData[i])
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
                                return numTags;
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
                        destBmp = new Bitmap(Data.OUTPUT_IMAGE_X, Data.OUTPUT_IMAGE_Y);
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
                srcBmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                g.DrawImage(srcBmp, srcBmp.Width, 0);
            }

            return destBmp;
        }

        private Bitmap DrawText(Bitmap destBmp, int boxNum)
        {
            List<byte> BoxData = Message[boxNum];

            int choiceType = 0;

            float xPos = Data.XPOS_DEFAULT;
            float yPos = Box == Data.BoxType.None_White ? Data.YPOS_DEFAULT : ((58 - (12 * FindNumOfTag(boxNum, (int)Data.MsgControlCode.LINE_BREAK))) / 2);

            if (yPos < 12)
                yPos = 12;

            float scale = Data.SCALE_DEFAULT;
            float xOffsChoice = ((Data.FontWidths[0] * scale) * 5);
            Color c = Box == Data.BoxType.None_White ? Color.Black : Color.White;

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                for (int charPos = 0; charPos < BoxData.Count; charPos++)
                {
                    switch (BoxData[charPos])
                    {
                        case (byte)Data.MsgControlCode.TWO_CHOICES:
                            {
                                choiceType = 2;

                                Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                float xPosChoice = 6;
                                float yPosChoice = 36;

                                for (int ch = 0; ch < 2; ch++)
                                {
                                    DrawImage(destBmp, imgArrow, Color.LimeGreen, scale, ref xPosChoice, ref yPosChoice, 0);
                                    yPosChoice += Data.LINEBREAK_SIZE;
                                }

                                xPos += xOffsChoice;

                                break;
                            }
                        case (byte)Data.MsgControlCode.THREE_CHOICES:
                            {
                                choiceType = 3;

                                Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                float xPosChoice = 6;
                                float yPosChoice = 24;

                                for (int ch = 0; ch < 3; ch++)
                                {
                                    DrawImage(destBmp, imgArrow, Color.LimeGreen, scale, ref xPosChoice, ref yPosChoice, 0);
                                    yPosChoice += Data.LINEBREAK_SIZE;
                                }

                                xPos += xOffsChoice;

                                break;
                            }
                        case (byte)Data.MsgControlCode.POINTS:
                        case (byte)Data.MsgControlCode.MARATHON_TIME:
                        case (byte)Data.MsgControlCode.RACE_TIME:
                            {
                                char[] Setting = Data.ControlCharPresets[(Data.MsgControlCode)BoxData[charPos]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, c, scale, ref xPos, ref yPos);

                                charPos++;
                                break;
                            }
                        case (byte)Data.MsgControlCode.FISH_WEIGHT:
                        case (byte)Data.MsgControlCode.GOLD_SKULLTULAS:
                        case (byte)Data.MsgControlCode.PLAYER:
                            {
                                char[] Setting = Data.ControlCharPresets[(Data.MsgControlCode)BoxData[charPos]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, c, scale, ref xPos, ref yPos);

                                break;
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

                                DrawImage(destBmp, left, Color.White, 1, ref xPosbg, ref yPosbg, 0);

                                xPosbg += left.Width;

                                DrawImage(destBmp, right, Color.White, 1, ref xPosbg, ref yPosbg, 0);

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
                                int num_shift = BoxData[charPos + 1];
                                xPos += Data.FontWidths[0] * scale;
                                charPos++;
                                continue;
                            }
                        case (byte)Data.MsgControlCode.COLOR:
                            {
                                int color_data_idx = BoxData[charPos + 1] - 0x40;

                                switch (color_data_idx)
                                {
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                    case 6:
                                    case 7:
                                        {
                                            RGB cl = Data.CharColors[color_data_idx - 1][Convert.ToInt32(Box == Data.BoxType.Wooden)];
                                            c = Color.FromArgb(255, cl.R, cl.G, cl.B);
                                            break;
                                        }
                                    default:
                                        {
                                            RGB cl = Data.CharColors[7][Convert.ToInt32(Box == Data.BoxType.None_Black)];
                                            c = Color.FromArgb(255, cl.R, cl.G, cl.B);
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

                                if ((choiceType == 2 && yPos >= 24) || (choiceType == 3 && yPos >= 6))
                                    xPos = Data.XPOS_DEFAULT + xOffsChoice;

                                continue;
                            }
                        default:
                            {
                                destBmp = DrawTextInternal(destBmp, BoxData[charPos], c, scale, ref xPos, ref yPos);
                                break;
                            }
                    }
                }

                if (FindNumOfTag(boxNum, (byte)Data.MsgControlCode.FADE) == 0 &&
                    FindNumOfTag(boxNum, (byte)Data.MsgControlCode.FADE2) == 0 &&
                    FindNumOfTag(boxNum, (byte)Data.MsgControlCode.TWO_CHOICES) == 0 &&
                    FindNumOfTag(boxNum, (byte)Data.MsgControlCode.THREE_CHOICES) == 0 &&
                    FindNumOfTag(boxNum, (byte)Data.MsgControlCode.EVENT) == 0)
                {
                    Bitmap imgend;

                    if (Message.Last() == BoxData)
                        imgend = Properties.Resources.Box_End;
                    else
                        imgend = Properties.Resources.Box_Triangle;

                    float xPosEnd = 128 - (imgend.Width / 2);
                    float yPosEnd = 64 - (imgend.Height / 2);

                    DrawImage(destBmp, imgend, Color.LimeGreen, scale, ref xPosEnd, ref yPosEnd, 0);
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
            img = Resize(img, scale);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                if (Box != Data.BoxType.None_White)
                {
                    shadow = Colorize(shadow, Color.Black);
                    shadow = Resize(shadow, scale);
                    g.DrawImage(shadow, xPos + 1.0f, yPos + 1.0f);
                }

                g.DrawImage(img, xPos, yPos);
            }

            xPos += (Data.FontWidths[Char - 0x20] * scale);
            return destBmp;
        }

        private Bitmap DrawImage(Bitmap destBmp, Bitmap srcBmp, Color cl, float scale, ref float xPos, ref float yPos, float xPosMove, bool revAlpha = true)
        {
            if (revAlpha)
                srcBmp = ReverseAlphaMask(srcBmp);

            Bitmap shadow = srcBmp;

            srcBmp = Colorize(srcBmp, cl);
            srcBmp = Resize(srcBmp, scale);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                if (Box != Data.BoxType.None_White)
                {
                    shadow = Colorize(shadow, Color.Black);
                    shadow = Resize(shadow, scale);
                    g.DrawImage(shadow, xPos + 1.0f, yPos + 1.0f);
                }

                g.DrawImage(srcBmp, xPos, yPos);
            }

            xPos += xPosMove;
            return destBmp;
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

        /*
                private Bitmap SetAlpha(Bitmap b, int Alpha)
                {
                    if (Alpha == 255)
                        return b;

                    b.MakeTransparent();

                    BitmapData bmpData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);

                    int bytes = Math.Abs(bmpData.Stride) * b.Height;
                    byte[] rgbaValues = new byte[bytes];

                    Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);

                    for (int i = 3; i < rgbaValues.Length; i += 4)
                    {
                        float r = rgbaValues[i] * ((float)Alpha / (float)255);
                        rgbaValues[i] = (byte)r;
                    }

                    Marshal.Copy(rgbaValues, 0, bmpData.Scan0, bytes);

                    b.UnlockBits(bmpData);

                    return b;
                }
        */

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
