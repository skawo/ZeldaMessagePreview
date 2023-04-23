﻿using System;
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
    public class MessagePreviewMajora
    {
        private readonly MajoraMsgHeader Header;
        private readonly List<List<byte>> Message = new List<List<byte>>();
        public int MessageCount;
        public bool BrightenText;

        private int OUTPUT_IMAGE_X = 256;
        private int OUTPUT_IMAGE_Y = 64 + (Properties.Resources.Box_End.Width / 2);

        public byte[] FontDataMajora = null;

        public MessagePreviewMajora(byte[] MessageDataMajora)
        {
            Header = new MajoraMsgHeader(MessageDataMajora);
            MessageDataMajora = MessageDataMajora.Skip(11).ToArray();

            SplitMsgIntoTextboxes(MessageDataMajora);

            MessageCount = Message.Count;

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
            {
            }

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

        public Bitmap GetPreview(int BoxNum = 0, bool brightenText = true, float outputScale = 1.75f)
        {

            BrightenText = brightenText;

            if ((int)Header.BoxType == (int)DataMajora.BoxType.None_White || (int)Header.BoxType == (int)DataMajora.BoxType.None_Black)
            {
                OUTPUT_IMAGE_X = 320;
                OUTPUT_IMAGE_Y = 64 + 8;
            }
            else if ((int)Header.BoxType == (int)DataMajora.BoxType.Credits)
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

            bmp = Resize(bmp, outputScale);

            return bmp;
        }

        private void SplitMsgIntoTextboxes(byte[] MessageDataMajora)
        {
            List<byte> box = new List<byte>();

            bool End = false;

            for (int i = 0; i < MessageDataMajora.Length; i++)
            {
                byte curByte = GetByteFromArray(MessageDataMajora, i);

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
                    case (byte)DataMajora.MsgControlCode.SHIFT:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageDataMajora, i + 1));
                            i += 1;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.DELAY_END:
                        {
                            Message.Add(box);
                            box = new List<byte>();

                            i += 2;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.DELAY_DC:
                    case (byte)DataMajora.MsgControlCode.DELAY_DI:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageDataMajora, i + 1));
                            box.Add(GetByteFromArray(MessageDataMajora, i + 2));
                            i += 2;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.FADE:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageDataMajora, i + 1));
                            i += 2;
                            End = true;
                            break;
                        }
                    case (byte)DataMajora.MsgControlCode.SOUND:
                        {
                            box.Add(curByte);
                            box.Add(GetByteFromArray(MessageDataMajora, i + 1));
                            box.Add(GetByteFromArray(MessageDataMajora, i + 2));
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
                byte curByte = GetByteFromArray(BoxDataMajora.ToArray(), i);

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
                        case (byte)DataMajora.MsgControlCode.DELAY_DC:
                        case (byte)DataMajora.MsgControlCode.DELAY_DI:
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

        private int GetNumberOfTags(int BoxNum, int Tag)
        {
            int numTags = 0;

            if (BoxNum < 0)
                for (int i = 0; i < Message.Count; i++)
                    numTags += GetNumberOfTags(i, Tag);
            else
            {
                List<byte> BoxDataMajora = Message[BoxNum];

                for (int i = 0; i < BoxDataMajora.Count; i++)
                {
                    byte curByte = GetByteFromArray(BoxDataMajora.ToArray(), i);

                    if (curByte == (byte)Tag)
                        numTags++;
                    else
                    {

                        switch (curByte)
                        {
                            case (byte)DataMajora.MsgControlCode.SHIFT:
                                i += 1; break;
                            case (byte)DataMajora.MsgControlCode.DELAY_END:
                            case (byte)DataMajora.MsgControlCode.DELAY_DC:
                            case (byte)DataMajora.MsgControlCode.DELAY_DI:
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
                    byte curByte = GetByteFromArray(BoxDataMajora.ToArray(), i);

                    if (curByte == (byte)DataMajora.MsgControlCode.LINE_BREAK)
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
                            case (byte)DataMajora.MsgControlCode.DELAY_DC:
                            case (byte)DataMajora.MsgControlCode.DELAY_DI:
                            case (byte)DataMajora.MsgControlCode.SOUND:
                            case (byte)DataMajora.MsgControlCode.FADE:
                                i += 2; break;
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
                        img = Properties.Resources.Box_Blue;
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
            List<byte> BoxDataMajora = Message[boxNum];

            float xPos = DataMajora.XPOS_DEFAULT;
            float yPos = (Header.BoxType == DataMajora.BoxType.None_White) ? 
                    36 
                : 
                    Math.Max(
                                DataMajora.YPOS_DEFAULT
                                ,
                                ((52 - (Data.LINEBREAK_SIZE * GetNumberOfLineBreaks(boxNum))) / 2)
                            );

            float scale = DataMajora.SCALE_DEFAULT;

            if ((int)Header.BoxType == (int)DataMajora.BoxType.Credits)
            {
                xPos = 20;
                yPos = 48;
                scale = 0.85f;
            }

            Color textColor = (Header.BoxType == DataMajora.BoxType.None_Black) ? Color.Black : Color.White;

            int choiceType = GetBoxChoiceTag(boxNum);

            if (Header.MajoraIcon != 0xFE)
            {
                string fn = $"majora_icon_{Header.MajoraIcon.ToString().ToLower()}";
                Bitmap img = (Bitmap)Properties.Resources.ResourceManager.GetObject(fn);

                if (img != null)
                {
                    float xPosIcon = xPos - 0x7;
                    float yPosIcon = Header.BoxType == DataMajora.BoxType.None_White ? 36 : 0x14;

                    DrawImage(destBmp, img, Color.White, img.Width, img.Height, ref xPosIcon, ref yPosIcon, 0, false);
                }

                xPos += 0x20;
            }

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                for (int charPos = 0; charPos < BoxDataMajora.Count; charPos++)
                {
                    switch (BoxDataMajora[charPos])
                    {
                        case (byte)DataMajora.MsgControlCode.TWO_CHOICES:
                            {
                                Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                float xPosChoice = 16;
                                float yPosChoice = 32;

                                for (int ch = 0; ch < 2; ch++)
                                {
                                    DrawImage(destBmp, imgArrow, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
                                    yPosChoice += DataMajora.LINEBREAK_SIZE;
                                }

                                break;
                            }
                        case (byte)DataMajora.MsgControlCode.THREE_CHOICES:
                            {
                                Bitmap imgArrow = Properties.Resources.Box_Arrow;
                                float xPosChoice = 16;
                                float yPosChoice = 20;

                                for (int ch = 0; ch < 3; ch++)
                                {
                                    DrawImage(destBmp, imgArrow, Color.LimeGreen, (int)(16 * scale), (int)(16 * scale), ref xPosChoice, ref yPosChoice, 0);
                                    yPosChoice += DataMajora.LINEBREAK_SIZE;
                                }

                                break;
                            }
                        case (byte)DataMajora.MsgControlCode.PLAYER:
                            {
                                char[] Setting = DataMajora.ControlCharPresets[(DataMajora.MsgControlCode)BoxDataMajora[charPos]].ToArray();

                                foreach (char ch in Setting)
                                    DrawTextInternal(destBmp, (byte)ch, textColor, scale, ref xPos, ref yPos);

                                break;
                            }
                        case (byte)DataMajora.MsgControlCode.BACKGROUND:
                            {
                                Bitmap left = Properties.Resources.xmes_left;
                                Bitmap right = Properties.Resources.xmes_right;

                                float xPosbg = 0;
                                float yPosbg = 0;

                                DrawImage(destBmp, left, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);

                                xPosbg += left.Width;

                                DrawImage(destBmp, right, Color.White, left.Width, left.Height, ref xPosbg, ref yPosbg, 0);
                                continue;
                            }
                        case (byte)DataMajora.MsgControlCode.SOUND:
                        case (byte)DataMajora.MsgControlCode.DELAY_DC:
                        case (byte)DataMajora.MsgControlCode.DELAY_DI:
                        case (byte)DataMajora.MsgControlCode.DELAY_END:
                            {
                                charPos += 2;
                                continue;
                            }
                        case (byte)DataMajora.MsgControlCode.FADE:
                            {
                                charPos += 2;
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
                                byte num_shift = GetByteFromArray(BoxDataMajora.ToArray(), charPos + 1);

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

                                


                                RGB cl = DataMajora.CharColors[color_DataMajora_idx][Convert.ToInt32(Header.BoxType == DataMajora.BoxType.Wooden)];
                                textColor = Color.FromArgb(255, cl.R, cl.G, cl.B);

                                break;
                            }
                        case (byte)DataMajora.MsgControlCode.LINE_BREAK:
                            {
                                if ((int)Header.BoxType == (int)DataMajora.BoxType.Credits)
                                {
                                    xPos = 20;
                                    yPos += 6;
                                }
                                else
                                {
                                    xPos = DataMajora.XPOS_DEFAULT;
                                    yPos += DataMajora.LINEBREAK_SIZE;
                                }

                                if ((choiceType == 2 && yPos >= 32) || (choiceType == 3 && yPos >= 20) || Header.MajoraIcon != 0xFE && yPos > 12)
                                    xPos = 2 * DataMajora.XPOS_DEFAULT;

                                continue;
                            }
                        default:
                            {
                                destBmp = DrawTextInternal(destBmp, BoxDataMajora[charPos], textColor, scale, ref xPos, ref yPos);
                                break;
                            }
                    }
                }

                if (GetNumberOfTags(boxNum, (byte)DataMajora.MsgControlCode.FADE) == 0 &&
                    GetNumberOfTags(boxNum, (byte)DataMajora.MsgControlCode.DELAY_END) == 0 &&
                    GetNumberOfTags(boxNum, (byte)DataMajora.MsgControlCode.TWO_CHOICES) == 0 &&
                    GetNumberOfTags(boxNum, (byte)DataMajora.MsgControlCode.THREE_CHOICES) == 0)
                {
                    Bitmap imgend;

                    if (Message.Last() == BoxDataMajora)
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

        public Bitmap GetBitmapFromI4FontChar(byte[] bytes)
        {
            List<Color> Pixels = new List<Color>();

            foreach (byte b in bytes)
            {
                byte ab = (byte)((b >> 4) * 0x11);
                byte bb = (byte)((b & 0x0F) * 0x11);

                Pixels.Add(Color.FromArgb(255, ab, ab, ab));
                Pixels.Add(Color.FromArgb(255, bb, bb, bb));
            }

            Bitmap img = new Bitmap(16, 16);
            int i = 0;

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    img.SetPixel(x, y, Pixels[i]);
                    i++;
                }
            }

            return img;
        }

        private Bitmap DrawTextInternal(Bitmap destBmp, byte Char, Color cl, float scale, ref float xPos, ref float yPos)
        {
            // Change button characters to match Ocarina's
            if (Char >= 0xB0 && Char <= 0xBC)
                Char = (byte)(0x9F + (Char - 0xB0));

            string fn = $"majora_char_{Char.ToString("X").ToLower()}";

            if (Char == ' ')
            {
                xPos += 6.0f;
                return destBmp;
            }

            Bitmap img;

            if (FontDataMajora != null && (Char - ' ') * 128 < FontDataMajora.Length)
                img = GetBitmapFromI4FontChar(FontDataMajora.Skip((Char - ' ') * 128).Take(128).ToArray());
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

            img = ReverseAlphaMask(img, BrightenText);

            Bitmap shadow = img;

            img = Colorize(img, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                if (Header.BoxType != DataMajora.BoxType.None_Black)
                {
                    shadow = Colorize(shadow, Color.Black);
                    shadow.SetResolution(g.DpiX, g.DpiY);

                    g.DrawImage(shadow, new Rectangle((int)xPos + 1, (int)yPos + 1, (int)(16 * scale), (int)(16 * scale)));
                }

                img.SetResolution(g.DpiX, g.DpiY);
                g.DrawImage(img, new Rectangle((int)xPos, (int)yPos, (int)(16 * scale), (int)(16 * scale)));
            }

            try
            {
                xPos += (int)Math.Floor((DataMajora.FontWidths[Char - 0x20] * scale));
            }
            catch (Exception)
            {
                // Lazy way to ensure the program does not crash if exported message DataMajora doesn't have DataMajora for all the characters.
            }

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

        private Bitmap ReverseAlphaMask(Bitmap bmp, bool Brighten = false)
        {
            bmp.MakeTransparent();

            BitmapData bmpDataMajora = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            int bytes = Math.Abs(bmpDataMajora.Stride) * bmp.Height;
            byte[] rgbaValues = new byte[bytes];

            Marshal.Copy(bmpDataMajora.Scan0, rgbaValues, 0, bytes);

            for (int i = 3; i < rgbaValues.Length; i += 4)
            {
                rgbaValues[i] = rgbaValues[i - 3];

                if (Brighten)
                {
                    rgbaValues[i - 1] = 255;
                    rgbaValues[i - 2] = 255;
                    rgbaValues[i - 3] = 255;
                }
            }

            Marshal.Copy(rgbaValues, 0, bmpDataMajora.Scan0, bytes);

            bmp.UnlockBits(bmpDataMajora);

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
}
