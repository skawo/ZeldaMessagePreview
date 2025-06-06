﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

namespace ZeldaMessage
{
    public class MessagePreview
    {
        public Data.BoxType Box;
        public List<List<byte>> Message = new List<List<byte>>();
        public int MessageCount;
        public bool BrightenText;
        public bool UseRealSpaceWidth;

        private int OUTPUT_IMAGE_X = 256;
        private int OUTPUT_IMAGE_Y = 64 + (Properties.Resources.Box_End.Width / 2);

        public byte[] FontData = null;

        public MessagePreview(Data.BoxType BoxType, byte[] MessageData, float[] _FontWidths = null, byte[] _FontData = null, bool _UseRealSpaceWidth = false)
        {
            UseRealSpaceWidth = _UseRealSpaceWidth;
            Box = BoxType;
            SplitMsgIntoTextboxes(MessageData);

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
                            Data.FontWidths[i / 4] = BitConverter.ToSingle(width, 0);
                        }
                    }
                }
                catch (Exception)
                { }
            }
            else
            {
                Data.FontWidths = _FontWidths;
            }


            if (_FontData == null)
            {
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
            else
                FontData = _FontData;

            Common.GetTagExtensions();
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

            string Errmsg = $"ERROR! - Textbox larger than 200 bytes.";

            for (int i = 0; i < Message.Count; i++)
            {
                if (Message[i].Count > 200)
                {
                    Message[i].Clear();
                    Message[i].AddRange(Encoding.GetEncoding("UTF-8").GetBytes(Errmsg.ToCharArray()));
                }
            }
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

        private int GetNumberOfTags(int BoxNum, List<int> Tags)
        {
            int numTags = 0;

            if (BoxNum < 0)
                for (int i = 0; i < Message.Count; i++)
                    numTags += GetNumberOfTags(i, Tags);
            else
            {
                List<byte> BoxData = Message[BoxNum];

                for (int i = 0; i < BoxData.Count; i++)
                {
                    byte curByte = Common.GetByteFromArray(BoxData.ToArray(), i);

                    if (Tags.Contains(curByte))
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
            Bitmap img;
            Color c;
            bool revAlpha;

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

            if (Common.tagExtend.ContainsKey(257))
            {
                object o = Common.tagExtend[257];
                MethodInfo mi = o.GetType().GetMethod("TagProcess");

                object ret = mi.Invoke(o, new object[] { destBmp, img, c, revAlpha, Box });
                object[] result = (ret as object[]);

                destBmp = (Bitmap)result[0];
                img = (Bitmap)result[1];
                c = (Color)result[2];
                revAlpha = (bool)result[3];
            }

            return DrawBoxInternal(destBmp, img, c, revAlpha);
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

        private Bitmap DrawText(Bitmap destBmp, int boxNum)
        {
            List<byte> BoxData = Message[boxNum];

            float xPos = Data.XPOS_DEFAULT;
            float yPos = (Box == Data.BoxType.None_White) ? 36 : Math.Max(Data.YPOS_DEFAULT, ((52 - (Data.LINEBREAK_SIZE * GetNumberOfTags(boxNum, new List<int>() { (int)Data.MsgControlCode.LINE_BREAK }))) / 2));
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
                    if (Common.tagExtend.ContainsKey(BoxData[charPos]))
                    {
                        object o = Common.tagExtend[BoxData[charPos]];
                        MethodInfo mi = o.GetType().GetMethod("TagProcess");

                        object ret = mi.Invoke(o, new object[] { destBmp, BoxData, textColor, scale, xPos, yPos, charPos, Box });
                        object[] result = (ret as object[]);

                        destBmp = (Bitmap)result[0];
                        BoxData = (List<byte>)result[1];
                        textColor = (Color)result[2];
                        scale = (float)result[3];
                        xPos = (float)result[4];
                        yPos = (float)result[5];
                        charPos = (int)result[6];
                    }
                    else
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
                }

                if (GetNumberOfTags(boxNum,
                        new List<int>()
                        {
                            (byte)Data.MsgControlCode.FADE,
                            (byte)Data.MsgControlCode.FADE2,
                            (byte)Data.MsgControlCode.DELAY,
                            (byte)Data.MsgControlCode.TWO_CHOICES,
                            (byte)Data.MsgControlCode.THREE_CHOICES,
                            (byte)Data.MsgControlCode.PERSISTENT,
                            (byte)Data.MsgControlCode.EVENT,
                        }
                   ) == 0)
                {
                    Bitmap imgend;
                    Color endColor = Color.LimeGreen;

                    if (Message.Count == boxNum + 1)
                        imgend = Properties.Resources.Box_End;
                    else
                        imgend = Properties.Resources.Box_Triangle;

                    float xPosEnd = 128 - 4;
                    float yPosEnd = 64 - 4;

                    if (Common.tagExtend.ContainsKey(256))
                    {
                        object o = Common.tagExtend[256];
                        MethodInfo mi = o.GetType().GetMethod("TagProcess");

                        object ret = mi.Invoke(o, new object[] { imgend, BoxData, endColor, xPosEnd, yPosEnd, Box});
                        object[] result = (ret as object[]);

                        imgend = (Bitmap)result[0];
                        BoxData = (List<byte>)result[1];
                        endColor = (Color)result[2];
                        xPosEnd = (float)result[3];
                        yPosEnd = (float)result[4];
                    }

                    Common.DrawImage(destBmp, imgend, endColor, (int)(16 * scale), (int)(16 * scale), ref xPosEnd, ref yPosEnd, 0);
                }
            }

            return destBmp;
        }

        private Bitmap DrawTextInternal(Bitmap destBmp, byte Char, Color cl, float scale, ref float xPos, ref float yPos)
        {
            string fn = $"char_{Char.ToString("X").ToLower()}";

            if (Char == ' ')
            {
                xPos += (UseRealSpaceWidth ? (int)(Data.FontWidths[0] * scale) : 6.0f);
                return destBmp;
            }

            Bitmap img;
            int startByte = (Char - ' ') * 128;

            if (FontData != null && startByte + 128 <= FontData.Length)
            {
                img = Common.GetBitmapFromI4FontChar(FontData.Skip(startByte).Take(128).ToArray());
            }
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
                xPos += (int)(Data.FontWidths[Char - 0x20] * scale);
            }
            catch (Exception)
            {
                xPos += 16 * scale;
                // Lazy way to ensure the program does not crash if exported message data doesn't have data for all the characters.
            }

            return destBmp;
        }
    }
}
