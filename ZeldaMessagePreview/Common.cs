using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaMessage
{
    public class Common
    {
        public static bool RunningUnderMono = Type.GetType("Mono.Runtime") != null;
        public static Dictionary<int, object> tagExtend = new Dictionary<int, object>();

        public static void GetTagExtensions()
        {
            StringBuilder sb = new StringBuilder();
            bool Errors = false;

            if (Directory.Exists("msgextend"))
            {
                string[] files = Directory.GetFiles("msgextend");

                foreach (string file in files)
                {
                    string tagN = Path.GetFileNameWithoutExtension(file);
                    int tagNumber = 0;

                    if (Int32.TryParse(tagN, out tagNumber))
                    {
                        if (tagExtend.ContainsKey(tagNumber))
                            continue;

                        string[] code = File.ReadAllLines(file);

                        Dictionary<string, string> providerOptions = new Dictionary<string, string>{{"CompilerVersion", "v3.5"}};

                        CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

                        CompilerParameters compilerParams = new CompilerParameters
                        {
                            GenerateInMemory = true,
                            GenerateExecutable = false,
                        };

                        for (int i = 0; i < code.Length; i++)
                        {
                            string line = code[i];

                            if (line.StartsWith("include "))
                            {
                                compilerParams.ReferencedAssemblies.Add(line.Substring(8));
                                code[i] = "";
                            }
                        }

                        CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, String.Join(Environment.NewLine, code));

                        if (results.Errors.Count != 0)
                        {
                            Errors = true;

                            foreach (CompilerError m in results.Errors)
                                sb.Append($"{tagNumber}:{m.ErrorText}{Environment.NewLine}");
                        }
                        else
                        {
                            object o = results.CompiledAssembly.CreateInstance("MsgExtend.Function");
                            tagExtend.Add(tagNumber, o);
                        }

                    }
                }

                if (Errors)
                    System.Windows.Forms.MessageBox.Show(sb.ToString());
            }
        }

        public static Bitmap GetBitmapFromI4FontChar(byte[] bytes)
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

        public static Bitmap FlipBitmapX_MonoSafe(Bitmap bmp)
        {
            if (RunningUnderMono)
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
            else
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                return bmp;
            }
        }

        public static Bitmap ReverseAlphaMask(Bitmap bmp, bool Brighten = false)
        {
            bmp.MakeTransparent();

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbaValues = new byte[bytes];

            Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);

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

            Marshal.Copy(rgbaValues, 0, bmpData.Scan0, bytes);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static Bitmap Resize(Bitmap bmp, float scale)
        {
            Bitmap result = new Bitmap((int)(bmp.Width * scale), (int)(bmp.Height * scale));

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(bmp, 0, 0, (int)(bmp.Width * scale), (int)(bmp.Height * scale));
            }

            return result;
        }

        public static Bitmap Colorize(Bitmap bmp, Color cl)
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

        public static byte GetByteFromArray(byte[] array, int i)
        {
            byte outB = 0;

            if (i <= array.Length - 1)
                outB = array[i];

            return outB;
        }

        public static Bitmap DrawImage(Bitmap destBmp, Bitmap srcBmp, Color cl, int xSize, int ySize, ref float xPos, ref float yPos, float xPosMove, bool revAlpha = true)
        {
            if (revAlpha)
                srcBmp = Common.ReverseAlphaMask(srcBmp);

            srcBmp = Common.Colorize(srcBmp, cl);

            using (Graphics g = Graphics.FromImage(destBmp))
            {
                srcBmp.SetResolution(g.DpiX, g.DpiY);
                g.DrawImage(srcBmp, new Rectangle((int)xPos, (int)yPos, xSize, ySize));
            }

            xPos += xPosMove;
            return destBmp;
        }
    }
}
