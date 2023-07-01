using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization;
using System.Text;
using Gtk;
using Gdk;
using static System.Net.Mime.MediaTypeNames;
using Cairo;
using Atk;
using GLib;


namespace AForge
{
    public enum PixelFormat
    {
        Indexed,
        Format1bppIndexed,
        Format4bppIndexed,
        Format8bppIndexed,
        Format16bppGrayScale,
        Format24bppRgb,
        Format32bppArgb,
        Format32bppRgb,
        Format32bppPArgb,
        Format48bppRgb,
        Format64bppArgb,
        Format64bppPArgb,
    }
    public enum RotateFlipType
    {
        Rotate180FlipNone,
        Rotate180FlipX,
        Rotate180FlipXY,
        Rotate180FlipY,
        Rotate270FlipNone,
        Rotate270FlipX,
        Rotate270FlipXY,
        Rotate270FlipY,
        Rotate90FlipNone,
        Rotate90FlipX,
        Rotate90FlipXY,
        Rotate90FlipY,
        RotateNoneFlipNone,
        RotateNoneFlipX,
        RotateNoneFlipXY,
        RotateNoneFlipY,
    }
    public struct ZCT
    {
        public int Z, C, T;
        public ZCT(int z, int c, int t)
        {
            Z = z;
            C = c;
            T = t;
        }
        public static bool operator ==(ZCT c1, ZCT c2)
        {
            if (c1.Z == c2.Z && c1.C == c2.C && c1.T == c2.T)
                return true;
            else
                return false;
        }
        public static bool operator !=(ZCT c1, ZCT c2)
        {
            if (c1.Z != c2.Z || c1.C != c2.C || c1.T != c2.T)
                return false;
            else
                return true;
        }
        public override string ToString()
        {
            return Z + "," + C + "," + T;
        }
    }
    public struct ZCTXY
    {
        public int Z, C, T, X, Y;
        public ZCTXY(int z, int c, int t, int x, int y)
        {
            Z = z;
            C = c;
            T = t;
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return Z + "," + C + "," + T + "," + X + "," + Y;
        }

        public static bool operator ==(ZCTXY c1, ZCTXY c2)
        {
            if (c1.Z == c2.Z && c1.C == c2.C && c1.T == c2.T && c1.X == c2.X && c1.Y == c2.Y)
                return true;
            else
                return false;
        }
        public static bool operator !=(ZCTXY c1, ZCTXY c2)
        {
            if (c1.Z != c2.Z || c1.C != c2.C || c1.T != c2.T || c1.X != c2.X || c1.Y != c2.Y)
                return false;
            else
                return true;
        }
    }
    /*
    public enum PixelFormat
    {
        Format8bppIndexed,
        Format16bppGrayScale,
        Format24bppRgb,
        Format32bppArgb,
        Format32bppRgb,
        Format48bppRgb,
        Format64bppArgb,
    }
    */
    public struct ColorS : IDisposable
    {
        public byte[] bytes;
        public ColorS(ushort s)
        {
            bytes = new byte[6];
            R = s;
            G = s;
            B = s;
        }
        public ColorS(ushort r, ushort g, ushort b)
        {
            bytes = new byte[6];
            R = r;
            G = g;
            B = b;
        }
        public float Rf
        {
            get { return (float)R / (float)ushort.MaxValue; }
            set
            {
                Byte[] bt = BitConverter.GetBytes(value * ushort.MaxValue);
                bytes[4] = bt[1];
                bytes[5] = bt[0];
            }
        }
        public float Gf
        {
            get { return (float)G / (float)ushort.MaxValue; }
            set
            {
                Byte[] bt = BitConverter.GetBytes(value * ushort.MaxValue);
                bytes[2] = bt[1];
                bytes[3] = bt[0];
            }
        }
        public float Bf
        {
            get { return (float)B / (float)ushort.MaxValue; }
            set
            {
                Byte[] bt = BitConverter.GetBytes(value * ushort.MaxValue);
                bytes[0] = bt[1];
                bytes[1] = bt[0];
            }
        }
        public ushort R
        {
            get { return BitConverter.ToUInt16(bytes, 0); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                bytes[0] = bt[0];
                bytes[1] = bt[1];
            }
        }
        public ushort G
        {
            get { return BitConverter.ToUInt16(bytes, 2); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                bytes[2] = bt[0];
                bytes[3] = bt[1];
            }
        }
        public ushort B
        {
            get { return BitConverter.ToUInt16(bytes, 4); }
            set
            {
                byte[] bt = BitConverter.GetBytes(value);
                bytes[4] = bt[0];
                bytes[5] = bt[1];
            }
        }
        public static ColorS FromVector(float x, float y, float z)
        {
            ColorS color = new ColorS();
            color.bytes = new byte[6];
            color.Rf = x;
            color.Gf = y;
            color.Bf = z;
            return color;
        }
        public static ColorS FromColor(Color col)
        {
            ColorS color = new ColorS();
            color.bytes = new byte[6];
            color.Rf = (float)col.R / 255;
            color.Gf = (float)col.G / 255;
            color.Bf = (float)col.B / 255;
            return color;
        }
        public static Color ToColor(ColorS s)
        {
            return Color.FromArgb((int)(s.Rf * 255),(int)(s.Gf * 255),(int)(s.Bf * 255));
        }
        public byte[] GetBytes(PixelFormat px)
        {
            if (px == PixelFormat.Format8bppIndexed)
            {
                byte[] bt = new byte[1];
                bt[0] = (byte)R;
                return bt;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                return BitConverter.GetBytes(R);
            }
            else
            if (px == PixelFormat.Format24bppRgb)
            {
                byte[] bt = new byte[3];
                bt[0] = (byte)B;
                bt[1] = (byte)G;
                bt[2] = (byte)R;
                return bt;
            }
            else
            if (px == PixelFormat.Format32bppRgb || px == PixelFormat.Format32bppArgb)
            {
                byte[] bt = new byte[4];
                bt[0] = 255;
                bt[1] = (byte)R;
                bt[2] = (byte)G;
                bt[3] = (byte)B;
                return bt;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                return bytes;
            }
            throw new InvalidDataException("Pixel format: " + px.ToString() + " is not supported");

        }
        public override string ToString()
        {
            return R + "," + G + "," + B;
        }
        public override bool Equals(object obj)
        {
            ColorS s = (ColorS)obj;
            if (s.R == R && s.G == G && s.B == B)
                return true;
            else
                return false;
        }
        public static ColorS operator /(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf / b.Rf), (ushort)(a.Gf / b.Gf), (ushort)(a.Bf / b.Bf));
        }
        public static ColorS operator *(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf * b.Rf), (ushort)(a.Gf * b.Gf), (ushort)(a.Bf * b.Bf));
        }
        public static ColorS operator +(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf + b.Rf), (ushort)(a.Gf + b.Gf), (ushort)(a.Bf + b.Bf));
        }
        public static ColorS operator -(ColorS a, ColorS b)
        {
            return new ColorS((ushort)(a.Rf - b.Rf), (ushort)(a.Gf - b.Gf), (ushort)(a.Bf - b.Bf));
        }
        public static ColorS operator /(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf / b, a.Gf / b, a.Bf / b);
        }
        public static ColorS operator *(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf * b, a.Gf * b, a.Bf * b);
        }
        public static ColorS operator +(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf + b, a.Gf + b, a.Bf + b);
        }
        public static ColorS operator -(ColorS a, float b)
        {
            return ColorS.FromVector(a.Rf - b, a.Gf - b, a.Bf - b);
        }
        public static bool operator ==(ColorS a, ColorS b)
        {
            if (a.R == b.R && a.G == b.G && a.B == b.B)
                return true;
            else
                return false;
        }
        public static bool operator !=(ColorS a, ColorS b)
        {
            if (a.R == b.R && a.G == b.G && a.B == b.B)
                return false;
            else
                return true;
        }
        public void Dispose()
        {
            bytes = null;
        }
    }
    public struct Color
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;
        public static Color FromArgb(int a,int r, int g, int b)
        {
            Color col = new Color();
            col.R = (byte)r;
            col.G = (byte)g;
            col.B = (byte)b;
            return col;
        }
        public static Color FromArgb(int r, int g, int b)
        {
            Color col = new Color();
            col.R = (byte)r;
            col.G = (byte)g;
            col.B = (byte)b;
            return col;
        }
        public static Color Black = FromArgb(255, 0, 0, 0);
        public static Color White = FromArgb(255, 255, 255, 255);
        public static Color Red = FromArgb(255, 255, 0, 0);
        public static Color DarkRed = FromArgb(255, 150, 0, 0);
        public static Color Green = FromArgb(255, 0, 255, 0);
        public static Color DarkGreen = FromArgb(255, 0, 150, 0);
        public static Color Blue = FromArgb(255, 0, 0, 255);
        public static Color DarkBlue = FromArgb(255, 0, 0 ,150);
        public static Color Magenta = FromArgb(255, 255, 0, 255);
        public static Color DarkMagenta = FromArgb(255, 150, 0, 150);
        public static Color Yellow = FromArgb(255, 255, 255, 0);
        public static Color DarkYellow = FromArgb(255, 150, 150, 0);
        public static Color Cyan = FromArgb(255, 0, 255, 255);
        public static Color DarkCyan = FromArgb(255, 0, 150, 150);
        public static Color Gray = FromArgb(255, 150, 150, 150);
        public static Color DarkGray = FromArgb(255, 100, 100, 100);
        public static Color LightGray = FromArgb(255, 200, 200, 200);
        public static Color DarkKhaki = FromArgb(255, 189, 183, 107);
        //TODO Set color values
        public static Color Violet = FromArgb(255, 189, 183, 107);
        public static Color Brown = FromArgb(255, 189, 183, 107);
        public static Color Olive = FromArgb(255, 189, 183, 107);
        public static Color Gold = FromArgb(255, 189, 183, 107);
        public static Color Indigo = FromArgb(255, 189, 183, 107);
        public static Color Ivory = FromArgb(255, 189, 183, 107);
        public static Color HotPink = FromArgb(255, 189, 183, 107);
        public static Color DarkSeaGreen = FromArgb(255, 189, 183, 107);
        public static Color LimeGreen = FromArgb(255, 189, 183, 107);
        public static Color Tomato = FromArgb(255, 189, 183, 107);
        public static Color SteelBlue = FromArgb(255, 189, 183, 107);
        public static Color SkyBlue = FromArgb(255, 189, 183, 107);
        public static Color Silver = FromArgb(255, 189, 183, 107);
        public static Color Salmon = FromArgb(255, 189, 183, 107);
        public static Color SaddleBrown = FromArgb(255, 189, 183, 107);
        public static Color RosyBrown = FromArgb(255, 189, 183, 107);
        public static Color PowderBlue = FromArgb(255, 189, 183, 107);
        public static Color Plum = FromArgb(255, 189, 183, 107);
        public static Color PapayaWhip = FromArgb(255, 189, 183, 107);
        public static Color Orange = FromArgb(255, 189, 183, 107);
        public static bool operator ==(Color a, Color b)
        {
            if (a.R == b.R && a.G == b.G && a.B == b.B)
                return true;
            else
                return false;
        }
        public static bool operator !=(Color a, Color b)
        {
            if (a.R == b.R && a.G == b.G && a.B == b.B)
                return false;
            else
                return true;
        }
    }
    public struct RectangleD
    {
        private double x;
        private double y;
        private double w;
        private double h;
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public double W { get { return w; } set { w = value; } }
        public double H { get { return h; } set { h = value; } }

        public RectangleD(double X, double Y, double W, double H)
        {
            x = X;
            y = Y;
            w = W;
            h = H;
        }
        public Rectangle ToRectangleInt()
        {
            return new Rectangle((int)X, (int)Y, (int)W, (int)H);
        }
        public bool IntersectsWith(RectangleD p)
        {
            if (IntersectsWith(p.X, p.Y) || IntersectsWith(p.X + p.W, p.Y) || IntersectsWith(p.X, p.Y + p.H) || IntersectsWith(p.X + p.W, p.Y + p.H))
                return true;
            else
                return false;
        }
        public bool IntersectsWith(PointD p)
        {
            return IntersectsWith(p.X, p.Y);
        }
        public bool IntersectsWith(double x, double y)
        {
            if (X <= x && (X + W) >= x && Y <= y && (Y + H) >= y)
                return true;
            else
                return false;
        }
        public RectangleF ToRectangleF()
        {
            return new RectangleF((float)X, (float)Y, (float)W, (float)H);
        }
        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString() + ", " + W.ToString() + ", " + H.ToString();
        }

    }
    public struct RectangleF
    {
        private float x;
        private float y;
        private float w;
        private float h;
        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }
        public float Width { get { return w; } set { w = value; } }
        public float Height { get { return h; } set { h = value; } }

        public RectangleF(float X, float Y, float W, float H)
        {
            x = X;
            y = Y;
            w = W;
            h = H;
        }
        public Rectangle ToRectangleInt()
        {
            return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
        }
        public bool IntersectsWith(RectangleF p)
        {
            if (IntersectsWith(p.X, p.Y) || IntersectsWith(p.X + p.Width, p.Y) || IntersectsWith(p.X, p.Y + p.Height) || IntersectsWith(p.X + p.Width, p.Y + p.Height))
                return true;
            else
                return false;
        }
        public bool IntersectsWith(PointF p)
        {
            return IntersectsWith(p.X, p.Y);
        }
        public bool IntersectsWith(float x, float y)
        {
            if (X <= x && (X + Width) >= x && Y <= y && (Y + Height) >= y)
                return true;
            else
                return false;
        }
        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString() + ", " + Width.ToString() + ", " + Height.ToString();
        }

    }
    public struct Rectangle
    {
        private int x;
        private int y;
        private int w;
        private int h;
        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }
        public int Width { get { return w; } set { w = value; } }
        public int Height { get { return h; } set { h = value; } }
        public int Top { get { return x; } set { x = value; } }
        public int Left { get { return y; } set { y = value; } }
        public int Right { get { return x + w; } }
        public int Bottom { get { return y + h; } }
        public Rectangle(int X, int Y, int W, int H)
        {
            x = X;
            y = Y;
            w = W;
            h = H;
        }
        public Rectangle ToRectangleInt()
        {
            return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
        }
        public bool IntersectsWith(RectangleD p)
        {
            if (IntersectsWith(p.X, p.Y) || IntersectsWith(p.X + p.W, p.Y) || IntersectsWith(p.X, p.Y + p.H) || IntersectsWith(p.X + p.W, p.Y + p.H))
                return true;
            else
                return false;
        }
        public bool IntersectsWith(PointD p)
        {
            return IntersectsWith(p.X, p.Y);
        }
        public bool IntersectsWith(double x, double y)
        {
            if (X <= x && (X + Width) >= x && Y <= y && (Y + Height) >= y)
                return true;
            else
                return false;
        }
        public Rectangle Intersect(Rectangle other)
        {
            int x1 = System.Math.Max(this.X, other.X);
            int x2 = System.Math.Min(this.X + this.Width, other.X + other.Width);
            int y1 = System.Math.Max(this.Y, other.Y);
            int y2 = System.Math.Min(this.Y + this.Height, other.Y + other.Height);

            if (x2 >= x1 && y2 >= y1)
            {
                return new Rectangle
                {
                    X = x1,
                    Y = y1,
                    Width = x2 - x1,
                    Height = y2 - y1
                };
            }
            else
            {
                return new Rectangle();
            }
        }
        public static Rectangle Intersect(Rectangle r, Rectangle other)
        {
            return r.Intersect(other);
        }
        public bool Contains(int x, int y)
        {
            return (x >= this.X) &&
                   (x < this.X + this.Width) &&
                   (y >= this.Y) &&
                   (y < this.Y + this.Height);
        }
        public void Inflate(int width, int height)
        {
            this.X -= width;
            this.Y -= height;
            this.Width += 2 * width;
            this.Height += 2 * height;
        }
        public RectangleF ToRectangleF()
        {
            return new RectangleF((float)X, (float)Y, (float)Width, (float)Height);
        }
        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString() + ", " + Width.ToString() + ", " + Height.ToString();
        }

    }
    public struct Size
    {
        int x, y;
        public int Width
        {
            set
            {
                x = value;
            }
            get
            {
                return x;
            }
        }
        public int Height
        {
            set
            {
                y = value;
            }
            get
            {
                return y;
            }
        }

        public Size(int xx, int yy)
        {
            x = xx;
            y = yy;
        }

        public static Size Parse(string s)
        {
            string[] st = s.Split(',');
            int xd = int.Parse(st[0], CultureInfo.InvariantCulture);
            int yd = int.Parse(st[1], CultureInfo.InvariantCulture);
            return new Size(xd, yd);
        }
        public static bool operator ==(Size p1, Size p2)
        {
            return (p1.Width == p2.Width && p1.Height == p2.Width);
        }
        public static bool operator !=(Size p1, Size p2)
        {
            return (p1.Width != p2.Width && p1.Height != p2.Width);
        }

        public override string ToString()
        {
            return x.ToString() + "," + y.ToString();
        }
    }
    public struct SizeF
    {
        float x, y;
        public float Width
        {
            set
            {
                x = value;
            }
            get
            {
                return x;
            }
        }
        public float Height
        {
            set
            {
                y = value;
            }
            get
            {
                return y;
            }
        }

        public SizeF(float xx, float yy)
        {
            x = xx;
            y = yy;
        }

        public static SizeF Parse(string s)
        {
            string[] st = s.Split(',');
            int xd = int.Parse(st[0], CultureInfo.InvariantCulture);
            int yd = int.Parse(st[1], CultureInfo.InvariantCulture);
            return new SizeF(xd, yd);
        }
        public static bool operator ==(SizeF p1, SizeF p2)
        {
            return (p1.Width == p2.Width && p1.Height == p2.Width);
        }
        public static bool operator !=(SizeF p1, SizeF p2)
        {
            return (p1.Width != p2.Width && p1.Height != p2.Width);
        }

        public override string ToString()
        {
            return x.ToString() + "," + y.ToString();
        }
    }
    public class Statistics
    {
        private int[] values = null;
        public int[] Values
        {
            get { return values; }
            set { values = value; }
        }
        private int bitsPerPixel;
        private int min = ushort.MaxValue;
        private int max = ushort.MinValue;
        private float stackMin = ushort.MaxValue;
        private float stackMax = ushort.MinValue;
        private float stackMean = 0;
        private float stackMedian = 0;
        private float mean = 0;
        private float median = 0;
        private float meansum = 0;
        private float[] stackValues = new float[ushort.MaxValue];
        private int count = 0;
        public int Min
        {
            get { return min; }
        }
        public int Max
        {
            get { return max; }
        }
        public double Mean
        {
            get { return mean; }
        }
        public int BitsPerPixel
        {
            get { return bitsPerPixel; }
        }
        public float Median
        {
            get
            {
                return median;
            }
        }
        public float StackMedian
        {
            get
            {
                return stackMedian;
            }
        }
        public float StackMean
        {
            get
            {
                return stackMean;
            }
        }
        public float StackMax
        {
            get
            {
                return stackMax;
            }
        }
        public float StackMin
        {
            get
            {
                return stackMin;
            }
        }
        public float[] StackValues
        {
            get { return stackValues; }
        }
        public Statistics(bool bit16)
        {
            values = new int[ushort.MaxValue + 1];
            if (bit16)
            {
                bitsPerPixel = 16;
            }
            else
            {
                bitsPerPixel = 8;
            }
        }
        public static Statistics[] FromBytes(byte[] bts, int w, int h, int rGBChannels, int BitsPerPixel, int stride)
        {
            Statistics[] sts = new Statistics[rGBChannels];
            bool bit16 = false;
            if (BitsPerPixel > 8)
                bit16 = true;
            for (int i = 0; i < rGBChannels; i++)
            {
                sts[i] = new Statistics(bit16);
                sts[i].max = ushort.MinValue;
                sts[i].min = ushort.MaxValue;
                sts[i].bitsPerPixel = BitsPerPixel;
            }

            float sumr = 0;
            float sumg = 0;
            float sumb = 0;
            float suma = 0;
            if (BitsPerPixel > 8)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < stride; x += 2 * rGBChannels)
                    {
                        if (rGBChannels == 3)
                        {
                            ushort b = BitConverter.ToUInt16(bts, (y * stride + (x)));
                            ushort g = BitConverter.ToUInt16(bts, (y * stride + (x + 2)));
                            ushort r = BitConverter.ToUInt16(bts, (y * stride + (x + 4)));
                            if (sts[0].max < r)
                                sts[0].max = r;
                            if (sts[0].min > r)
                                sts[0].min = r;
                            sts[0].values[r]++;
                            sumr += r;
                            if (sts[1].max < g)
                                sts[1].max = g;
                            if (sts[1].min > g)
                                sts[1].min = g;
                            sts[1].values[g]++;
                            sumg += g;
                            if (sts[2].max < b)
                                sts[2].max = b;
                            if (sts[2].min > b)
                                sts[2].min = b;
                            sts[2].values[b]++;
                            sumb += b;
                        }
                        else
                        {
                            ushort r = BitConverter.ToUInt16(bts, (y * stride + (x)));
                            if (sts[0].max < r)
                                sts[0].max = r;
                            if (sts[0].min > r)
                                sts[0].min = r;
                            sts[0].values[r]++;
                            sumr += r;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (rGBChannels > 1)
                        {
                            byte b = bts[y * stride + x];
                            byte g = bts[y * stride + (x + 1)];
                            byte r = bts[y * stride + (x + 2)];
                            byte a = 0;
                            if (rGBChannels == 4)
                            {
                                a = bts[y * stride + x];
                                b = bts[y * stride + (x + 1)];
                                g = bts[y * stride + (x + 2)];
                                r = bts[y * stride + (x + 3)];

                                if (sts[0].max < a)
                                    sts[0].max = a;
                                if (sts[0].min > a)
                                    sts[0].min = a;
                                sts[0].values[a]++;
                                suma += a;
                                if (sts[1].max < b)
                                    sts[1].max = b;
                                if (sts[1].min > b)
                                    sts[1].min = b;
                                sts[1].values[b]++;
                                sumb += b;
                                if (sts[2].max < g)
                                    sts[2].max = g;
                                if (sts[2].min > g)
                                    sts[2].min = g;
                                sts[2].values[g]++;
                                sumg += g;
                                if (sts[3].max < r)
                                    sts[3].max = r;
                                if (sts[3].min > r)
                                    sts[3].min = r;
                                sts[3].values[r]++;
                                sumr += r;
                            }
                            else
                            {
                                if (sts[0].max < r)
                                    sts[0].max = r;
                                if (sts[0].min > r)
                                    sts[0].min = r;
                                sts[0].values[r]++;
                                sumr += r;
                                if (sts[1].max < g)
                                    sts[1].max = g;
                                if (sts[1].min > g)
                                    sts[1].min = g;
                                sts[1].values[g]++;
                                sumg += g;
                                if (sts[2].max < b)
                                    sts[2].max = b;
                                if (sts[2].min > b)
                                    sts[2].min = b;
                                sts[2].values[b]++;
                                sumb += b;
                            }
                        }
                        else
                        {
                            byte r = bts[y * stride + x];
                            if (sts[0].max < r)
                                sts[0].max = r;
                            if (sts[0].min > r)
                                sts[0].min = r;
                            sts[0].values[r]++;
                            sumr += r;
                        }
                    }
                }
            }

            sts[0].mean = sumr / (float)(w * h);
            if (rGBChannels > 1)
            {
                sts[1].mean = sumg / (float)(w * h);
                sts[2].mean = sumb / (float)(w * h);
                if (rGBChannels == 4)
                    sts[3].mean = suma / (float)(w * h);
            }

            for (int i = 0; i < sts[0].values.Length; i++)
            {
                if (sts[0].median < sts[0].values[i])
                    sts[0].median = sts[0].values[i];
            }
            if (rGBChannels > 1)
            {
                for (int i = 0; i < sts[1].values.Length; i++)
                {
                    if (sts[1].median < sts[1].values[i])
                        sts[1].median = sts[1].values[i];
                    if (sts[2].median < sts[2].values[i])
                        sts[2].median = sts[2].values[i];
                    if (rGBChannels == 4)
                    {
                        if (sts[3].median < sts[3].values[i])
                            sts[3].median = sts[3].values[i];
                    }
                }
            }
            return sts;
        }
        public static Statistics[] FromBytes(Bitmap bf)
        {
            return FromBytes(bf.Bytes, bf.SizeX, bf.SizeY, bf.RGBChannelsCount, bf.BitsPerPixel, bf.Stride);
        }
        public static Dictionary<string, Bitmap> list = new Dictionary<string, Bitmap>();
        public static void FromBytes()
        {
            string name = System.Threading.Thread.CurrentThread.Name;
            list[name].Stats = FromBytes(list[name]);
            list.Remove(name);
        }
        public static void CalcStatistics(Bitmap bf)
        {
            bf.Stats = null;
            System.Threading.Thread th = new System.Threading.Thread(FromBytes);
            th.Name = bf.ID;
            list.Add(th.Name.ToString(), bf);
            th.Start();
        }
        public static void ClearCalcBuffer()
        {
            list.Clear();
        }
        public void AddStatistics(Statistics s)
        {
            if (stackValues == null)
            {
                if (bitsPerPixel > 8)
                    stackValues = new float[ushort.MaxValue + 1];
                else
                    stackValues = new float[byte.MaxValue + 1];
            }
            if (stackMax < s.max)
                stackMax = s.max;
            if (stackMin > s.min)
                stackMin = s.min;
            meansum += s.mean;
            for (int i = 0; i < stackValues.Length; i++)
            {
                stackValues[i] += s.values[i];
            }
            values = s.values;
            count++;
        }
        public void MeanHistogram()
        {
            for (int i = 0; i < stackValues.Length; i++)
            {
                stackValues[i] /= (float)count;
            }
            stackMean = (float)meansum / (float)count;

            for (int i = 0; i < stackValues.Length; i++)
            {
                if (stackMedian < stackValues[i])
                    stackMedian = (float)stackValues[i];
            }

        }

        public PointF[] GetPoints(int bin)
        {
            PointF[] pts = new PointF[stackValues.Length];
            for (int x = 0; x < stackValues.Length; x += bin)
            {
                pts[x].X = x;
                pts[x].Y = stackValues[x];
            }
            return pts;
        }
        public void Dispose()
        {
            stackValues = null;
            values = null;
        }
        public void DisposeHistogram()
        {
            stackValues = null;
            values = null;
        }
    }
    public class Plane
    {
        private double exposure;
        private ZCT coordinate;
        private double delta;
        private Point3D location;
        public double Exposure
        {
            get { return exposure; }
            set { exposure = value; }
        }
        public ZCT Coordinate
        {
            get { return coordinate; }
            set { coordinate = value; }
        }
        public double Delta
        {
            get { return delta; }
            set { delta = value; }
        }
        public Point3D Location
        {
            get { return location; }
            set { location = value; }
        }
    }
    public class BitmapData
    {
        IntPtr ptr;
        int stride;
        int w, h;
        PixelFormat pm;
        public IntPtr Scan0
        {
            get
            {
                return ptr;
            }
        }
        public int Stride
        {
            get
            {
                return stride;
            }
        }
        public int Width
        {
            get { return w; }
            set { w = value; }
        }
        public int Height
        {
            get { return h; }
            set { h = value; }
        }
        public PixelFormat PixelFormat
        {
            get { return pm; }
        }
        public BitmapData(IntPtr p,int stride, int w, int h, PixelFormat px)
        {
            ptr = p;
            this.stride = stride;
            this.w = w;
            this.h = h;
            pm = px;
        }
    }
    public class Channel : IDisposable
    {
        public IntRange[] range;
        public ChannelInfo info;
        public Statistics[] stats;
        private string contrastMethod = null;
        private string illuminationType = null;
        private string acquisitionMode = null;
        private int index;
        [Serializable]
        public struct ChannelInfo
        {
            internal string name;
            internal string id;
            internal int index;
            internal string fluor;
            internal int samplesPerPixel;
            internal Color? color;
            internal int emission;
            internal int excitation;
            internal int exposure;
            internal string lightSource;
            internal double lightSourceIntensity;
            internal int lightSourceWavelength;
            internal string contrastMethod;
            internal string illuminationType;
            internal int bitsPerPixel;
            internal int min, max;
            internal string diodeName;
            internal string lightSourceAttenuation;
            internal string acquisitionMode;
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    index = value;
                }

            }
            public int Min
            {
                get
                {
                    return min;
                }
                set
                {
                    if (value <= 0)
                    {
                        max = 0;
                        return;
                    }
                    if (value > ushort.MaxValue)
                        min = 0;
                    else
                        min = value;
                }
            }
            public int Max
            {
                get
                {
                    return max;
                }
                set
                {
                    if (value <= 0)
                    {
                        max = 1;
                        return;
                    }
                    if (value > ushort.MaxValue)
                        max = ushort.MaxValue;
                    else
                        max = value;

                }
            }
            public string Fluor
            {
                get { return fluor; }
                set { fluor = value; }
            }
            public int SamplesPerPixel
            {
                get { return samplesPerPixel; }
                set { samplesPerPixel = value; }
            }
            public Color? Color
            {
                get { return color; }
                set { color = value; }
            }
            public int Emission
            {
                get { return emission; }
                set { emission = value; }
            }
            public int Excitation
            {
                get { return excitation; }
                set { excitation = value; }
            }
            public int Exposure
            {
                get { return exposure; }
                set { exposure = value; }
            }
            public string LightSource
            {
                get { return lightSource; }
                set { lightSource = value; }
            }
            public double LightSourceIntensity
            {
                get { return lightSourceIntensity; }
                set { lightSourceIntensity = value; }
            }
            public int LightSourceWavelength
            {
                get { return lightSourceWavelength; }
                set { lightSourceWavelength = value; }
            }
            public string ContrastMethod
            {
                get
                {
                    return contrastMethod;
                }
                set
                {
                    contrastMethod = value;
                }
            }
            public string IlluminationType
            {
                get
                {
                    return illuminationType;
                }
                set
                {
                    illuminationType = value;
                }
            }
            public string DiodeName
            {
                get { return diodeName; }
                set
                {
                    diodeName = value;
                }
            }
            public string LightSourceAttenuation
            {
                get { return lightSourceAttenuation; }
                set
                {
                    lightSourceAttenuation = value;
                }
            }
            public string AcquisitionMode
            {
                get
                {
                    return acquisitionMode;
                }
                set
                {
                    acquisitionMode = value;
                }
            }
            public string ID
            {
                get { return id; }
                set { id = value; }
            }
        }
        public static Color SpectralColor(double l) // RGB <0,1> <- lambda l <400,700> [nm]
        {
            double t;
            double r = 0;
            double g = 0;
            double b = 0;
            if ((l >= 400.0) && (l < 410.0)) { t = (l - 400.0) / (410.0 - 400.0); r = +(0.33 * t) - (0.20 * t * t); }
            else if ((l >= 410.0) && (l < 475.0)) { t = (l - 410.0) / (475.0 - 410.0); r = 0.14 - (0.13 * t * t); }
            else if ((l >= 545.0) && (l < 595.0)) { t = (l - 545.0) / (595.0 - 545.0); r = +(1.98 * t) - (t * t); }
            else if ((l >= 595.0) && (l < 650.0)) { t = (l - 595.0) / (650.0 - 595.0); r = 0.98 + (0.06 * t) - (0.40 * t * t); }
            else if ((l >= 650.0) && (l < 700.0)) { t = (l - 650.0) / (700.0 - 650.0); r = 0.65 - (0.84 * t) + (0.20 * t * t); }
            if ((l >= 415.0) && (l < 475.0)) { t = (l - 415.0) / (475.0 - 415.0); g = +(0.80 * t * t); }
            else if ((l >= 475.0) && (l < 590.0)) { t = (l - 475.0) / (590.0 - 475.0); g = 0.8 + (0.76 * t) - (0.80 * t * t); }
            else if ((l >= 585.0) && (l < 639.0)) { t = (l - 585.0) / (639.0 - 585.0); g = 0.84 - (0.84 * t); }
            if ((l >= 400.0) && (l < 475.0)) { t = (l - 400.0) / (475.0 - 400.0); b = +(2.20 * t) - (1.50 * t * t); }
            else if ((l >= 475.0) && (l < 560.0)) { t = (l - 475.0) / (560.0 - 475.0); b = 0.7 - (t) + (0.30 * t * t); }
            r *= 255;
            g *= 255;
            b *= 255;
            return AForge.Color.FromArgb(255, (int)r, (int)g, (int)b);
        }
        public string Name
        {
            get { return info.name; }
            set { info.name = value; }
        }
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }
        public IntRange RangeR
        {
            get
            {
                return range[0];
            }
        }
        public IntRange RangeG
        {
            get
            {
                if (range.Length > 1)
                    return range[1];
                else
                    return range[0];
            }
        }
        public IntRange RangeB
        {
            get
            {
                if (range.Length > 1)
                    return range[2];
                else
                    return range[0];
            }
        }
        public string Fluor
        {
            get { return info.fluor; }
            set { info.fluor = value; }
        }
        public int SamplesPerPixel
        {
            get { return info.samplesPerPixel; }
            set
            {
                info.samplesPerPixel = value;
                range = new IntRange[info.samplesPerPixel];
            }
        }
        public Color? Color
        {
            get { return info.color; }
            set { info.color = value; }
        }
        public Color EmissionColor
        {
            get { return SpectralColor(Emission); }
        }
        public int Emission
        {
            get { return info.emission; }
            set { info.emission = value; }
        }
        public int Excitation
        {
            get { return info.excitation; }
            set { info.excitation = value; }
        }
        public int Exposure
        {
            get { return info.exposure; }
            set { info.exposure = value; }
        }
        public string LightSource
        {
            get { return info.lightSource; }
            set { info.lightSource = value; }
        }
        public double LightSourceIntensity
        {
            get { return info.lightSourceIntensity; }
            set { info.lightSourceIntensity = value; }
        }
        public int LightSourceWavelength
        {
            get { return info.lightSourceWavelength; }
            set { info.lightSourceWavelength = value; }
        }
        public string LightSourceAttenuation
        {
            get { return info.lightSourceAttenuation; }
            set { info.lightSourceAttenuation = value; }
        }
        public string DiodeName
        {
            get { return info.diodeName; }
            set { info.diodeName = value; }
        }
        public string ContrastMethod
        {
            get { return contrastMethod; }
            set
            {
                contrastMethod = value;
            }
        }
        public string IlluminationType
        {
            get { return illuminationType; }
            set { illuminationType = value; }
        }
        public string AcquisitionMode
        {
            get { return acquisitionMode; }
            set { acquisitionMode = value; }
        }

        public int BitsPerPixel
        {
            get { return info.bitsPerPixel; }
            set { info.bitsPerPixel = value; }
        }
        public Channel(int ind, int bitsPerPixel, int samples)
        {
            info = new ChannelInfo();
            if (bitsPerPixel == 16)
                info.Max = 65535;
            if (bitsPerPixel == 14)
                info.Max = 16383;
            if (bitsPerPixel == 12)
                info.Max = 4096;
            if (bitsPerPixel == 10)
                info.Max = 1024;
            if (bitsPerPixel == 8)
                info.Max = byte.MaxValue;
            info.samplesPerPixel = samples;
            range = new IntRange[info.SamplesPerPixel];
            for (int i = 0; i < range.Length; i++)
            {
                range[i] = new IntRange(0, info.Max);
            }
            info.min = 0;
            info.bitsPerPixel = bitsPerPixel;
            info.Name = ind.ToString();
            contrastMethod = "BRIGHTFIELD";
            illuminationType = "TRANSMITTED";
            info.Fluor = "None";
            info.LightSource = "Unknown";
            info.diodeName = "LED";
            info.lightSourceAttenuation = "1.0";
            info.LightSource = null;
            Index = ind;
        }
        public Channel Copy()
        {
            Channel c = new Channel(info.index, info.bitsPerPixel, SamplesPerPixel);
            c.Name = Name;
            c.info.ID = info.ID;
            c.range = range;
            c.info.color = info.color;
            c.Fluor = Fluor;
            c.SamplesPerPixel = SamplesPerPixel;
            c.Emission = Emission;
            c.Excitation = Excitation;
            c.Exposure = Exposure;
            c.LightSource = LightSource;
            c.LightSourceIntensity = LightSourceIntensity;
            c.LightSourceWavelength = LightSourceWavelength;
            c.contrastMethod = contrastMethod;
            c.illuminationType = illuminationType;
            c.DiodeName = DiodeName;
            c.LightSourceAttenuation = LightSourceAttenuation;
            return c;
        }
        public override string ToString()
        {
            if (Name == "")
                return index.ToString();
            else
                return index + ", " + Name;
        }
        public void Dispose()
        {
            if (stats != null)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    if (stats[i] != null)
                        stats[i].Dispose();
                }
            }
        }
    }
    public enum ImageLockMode
    {
        ReadWrite,
        ReadOnly,
    }
    public class Bitmap : IDisposable
    {
        public ushort GetValueRGB(int x, int y, int RGBChannel)
        {
            if (bytes == null)
                return 0;
            if (x >= SizeX || x < 0)
                x = 0;
            if (y >= SizeY || y < 0)
                y = 0;
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 2 * RGBChannelsCount) + (RGBChannel * 2);
                return BitConverter.ToUInt16(bytes, index2);
            }
            else
            {
                int stride = SizeX;
                int index = ((y * stridex + x) * RGBChannelsCount) + RGBChannel;
                return bytes[index];
            }
        }
        public ColorS GetPixel(int ix, int iy)
        {
            if (isRGB)
                return new ColorS(GetValueRGB(ix, iy, 0), GetValueRGB(ix, iy, 1), GetValueRGB(ix, iy, 2));
            else
            {
                ushort s = GetValueRGB(ix, iy, 0);
                return new ColorS(s, s, s);
            }
        }
        public void SetPixel(int ix, int iy, ColorS col)
        {
            if (isRGB)
            {
                SetColorRGB(ix, iy, col);
            }
            else
                SetValue(ix, iy, col.R);
        }
        public void SetValue(int x, int y, ushort value)
        {
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 2 * RGBChannelsCount);
                byte upper = (byte)(value >> 8);
                byte lower = (byte)(value & 0xff);
                bytes[index2] = upper;
                bytes[index2 + 1] = lower;
            }
            else
            {
                int index = (y * stridex + x) * RGBChannelsCount;
                bytes[index] = (byte)value;
            }
        }
        public void SetValueRGB(int ix, int iy, int RGBChannel, ushort value)
        {
            int x = ix;
            int y = iy;
            //We invert the RGB channel parameter since pixels are in BGR order.
            if (RGBChannel == 2)
                RGBChannel = 0;
            else
            if (RGBChannel == 0)
                RGBChannel = 2;
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 2 * RGBChannelsCount) + (RGBChannel * 2);
                byte upper = (byte)(value >> 8);
                byte lower = (byte)(value & 0xff);
                bytes[index2] = upper;
                bytes[index2 + 1] = lower;
            }
            else
            {
                int index = ((y * stridex + x) * RGBChannelsCount) + RGBChannel;
                bytes[index] = (byte)value;
            }
        }
        public void SetColorRGB(int ix, int iy, ColorS value)
        {
            int x = ix;
            int y = iy;
            int stridex = SizeX;
            if (BitsPerPixel > 8)
            {
                int index2 = ((y * stridex + x) * 6);
                bytes[index2] = value.bytes[0];
                bytes[index2 + 1] = value.bytes[1];
                bytes[index2 + 2] = value.bytes[2];
                bytes[index2 + 3] = value.bytes[3];
                bytes[index2 + 4] = value.bytes[4];
                bytes[index2 + 5] = value.bytes[5];
            }
            else
            {
                int index2 = ((y * stridex + x) * RGBChannelsCount);
                bytes[index2 + 2] = (byte)value.R;
                bytes[index2 + 1] = (byte)value.G;
                bytes[index2] = (byte)value.B;
            }
        }
        public static string CreateID(string filepath, int index)
        {
            if (filepath == null)
                return "";
            const char sep = '/';
            filepath = filepath.Replace("\\", "/");
            string s = filepath + sep + 'i' + sep + index;
            return s;
        }
        float horizontalRes = 96f;
        float verticalRes = 96f;
        internal ColorPalette palette = new ColorPalette();
#if DEBUG
        public Gdk.Pixbuf ClipboardImage
        {
            get
            {
                Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(RGBBytes, true, 8, Width, Height, Width * 4);
                // Get the default clipboard
                var clipboard = Clipboard.Get(Gdk.Selection.Clipboard);
                // Convert the image to a byte array
                var imageData = pixbuf.SaveToBuffer("png");
                // Convert the image data to Base64-encoded string
                var imageString = Convert.ToBase64String(imageData);
                // Set the image data as text to the clipboard
                clipboard.Text = "data:image/png;base64," + imageString;
                return pixbuf;
            }
        }
#endif
        public string ID;
        public string File
        {
            get { return file; }
            set { file = value; }
        }
        public int HashID
        {
            get
            {
                return ID.GetHashCode();
            }
        }
        public int SizeX, SizeY;
        public int Width
        {
            get { return SizeX; }
        }
        public int Height
        {
            get { return SizeY; }
        }
        public int Stride
        {
            get
            {
                int s = 0;
                if (pixelFormat == PixelFormat.Format8bppIndexed)
                    s = SizeX;
                else
                if (pixelFormat == PixelFormat.Format16bppGrayScale)
                    s = SizeX * 2;
                else
                if (pixelFormat == PixelFormat.Format24bppRgb)
                    s = SizeX * 3;
                else
                    if (pixelFormat == PixelFormat.Format32bppRgb || pixelFormat == PixelFormat.Format32bppArgb)
                    s = SizeX * 4;
                else
                    s = SizeX * 3 * 2;
                return s;
            }
        }
        public int PaddedStride
        {
            get
            {
                return GetStridePadded(Stride);
            }
        }
        public byte[] PaddedBuffer
        {
            get
            {
                return GetPaddedBuffer(Bytes, SizeX, SizeY, Stride, PixelFormat);
            }
        }
        public bool LittleEndian
        {
            get
            {
                return littleEndian;
            }
            set
            {
                littleEndian = value;
            }
        }
        public long Length
        {
            get
            {
                return bytes.Length;
            }
        }
        public int RGBChannelsCount
        {
            get
            {
                if (PixelFormat == PixelFormat.Format24bppRgb || PixelFormat == PixelFormat.Format48bppRgb)
                    return 3;
                else
                if (PixelFormat == PixelFormat.Format8bppIndexed || PixelFormat == PixelFormat.Format16bppGrayScale)
                    return 1;
                else
                    return 4;
            }
        }
        public int BitsPerPixel
        {
            get
            {
                if (PixelFormat == PixelFormat.Format16bppGrayScale || PixelFormat == PixelFormat.Format48bppRgb)
                {
                    return 16;
                }
                else
                    return 8;
            }
        }
        public ZCT Coordinate;
        public PixelFormat PixelFormat
        {
            get
            {
                return pixelFormat;
            }
            set
            {
                pixelFormat = value;
            }
        }
        public byte[] Bytes
        {
            get { return bytes; }
            set
            {
                bytes = value;
            }
        }
        public byte[] PaddedBytes
        {
            get
            {
                return GetPaddedBuffer(bytes, SizeX, SizeY, Stride, PixelFormat);
            }
        }
        public unsafe AForge.Imaging.UnmanagedImage Image
        {
            get
            {
                fixed(byte* dat = PaddedBytes)
                {
                    return new AForge.Imaging.UnmanagedImage((IntPtr)dat, SizeX, SizeY, PaddedStride, PixelFormat);
                }
            }
            set
            {
                bytes = new byte[value.Stride * value.Height];
                Marshal.Copy(value.ImageData, Bytes, 0, value.Stride * value.Height);
                PixelFormat = value.PixelFormat;
                SizeX = value.Width;
                SizeY = value.Height;
            }
        }
        public byte[] RGBBytes
        {
            get
            {
                return GetBitmapRGB(SizeX, SizeY, PixelFormat, Bytes).Bytes;
            }
        }
        public Bitmap ImageRGB
        {
            get
            {
                return GetBitmapRGB(SizeX, SizeY, PixelFormat, Bytes);
            }
            set
            {
                Marshal.Copy(value.Bytes, 0, Marshal.UnsafeAddrOfPinnedArrayElement(Bytes,0), value.Bytes.Length);
                PixelFormat = value.PixelFormat;
                SizeX = value.Width;
                SizeY = value.Height;
            }
        }
        public unsafe IntPtr Data
        {
            get
            {
                fixed (byte* dat = Bytes)
                {
                    return (IntPtr)dat;
                }
            }
        }
        public IntPtr RGBData
        {
            get
            {
                return GetRGB32Data(SizeX, SizeY, PixelFormat, bytes);
            }
        }
        public Plane Plane
        {
            get { return plane; }
            set { plane = value; }
        }
        public int PixelFormatSize
        {
            get
            {
                if (pixelFormat == PixelFormat.Format8bppIndexed)
                    return 1;
                else if (pixelFormat == PixelFormat.Format16bppGrayScale)
                    return 2;
                else if (pixelFormat == PixelFormat.Format24bppRgb)
                    return 3;
                else if (pixelFormat == PixelFormat.Format32bppRgb || pixelFormat == PixelFormat.Format32bppArgb)
                    return 4;
                else if (pixelFormat == PixelFormat.Format48bppRgb)
                    return 6;
                throw new InvalidDataException("Bio only supports 8, 16, 24, 32, and 48 bit images.");
            }
        }
        public static int GetPixelFormatSize(PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormat.Format1bppIndexed)
                return 1;
            else if (pixelFormat == PixelFormat.Format4bppIndexed)
                return 4;
            else if (pixelFormat == PixelFormat.Format8bppIndexed)
                return 8;
            else if (pixelFormat == PixelFormat.Format16bppGrayScale)
                return 16;
            else if (pixelFormat == PixelFormat.Format24bppRgb)
                return 24;
            else if (pixelFormat == PixelFormat.Format32bppRgb || pixelFormat == PixelFormat.Format32bppArgb || pixelFormat == PixelFormat.Format32bppPArgb)
                return 32;
            else if (pixelFormat == PixelFormat.Format48bppRgb)
                return 48;
            else if (pixelFormat == PixelFormat.Format64bppArgb || pixelFormat == PixelFormat.Format64bppPArgb)
                return 64;
            throw new NotSupportedException();
        }
        public class ColorPalette
        {
            internal Color[] entries;
            public Color[] Entries { get { return entries; } set { entries = value; } }
            public ColorPalette()
            {
                entries = new Color[256];
            }
        }
        public ColorPalette Palette
        {
            get { return palette; }
            set { palette = value; }
        }
        public float VerticalResolution
        {
            get { return verticalRes; }
            set { verticalRes = value; }
        }
        public float HorizontalResolution
        {
            get { return horizontalRes; }
            set { horizontalRes = value; }
        }
        private PixelFormat pixelFormat;
        public Statistics[] Stats
        {
            get { return stats; }
            set { stats = value; }
        }
        Statistics[] stats;
        byte[] bytes;
        string file;
        bool littleEndian = BitConverter.IsLittleEndian;
        Plane plane = null;
        public void SetImage(Bitmap Bitmap, bool switchRGB)
        {
            if (switchRGB)
                Bitmap = Bitmap.SwitchRedBlue(Bitmap);
            //Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            PixelFormat = Bitmap.PixelFormat;
            SizeX = Bitmap.Width;
            SizeY = Bitmap.Height;
            bytes = GetBuffer((Bitmap)Bitmap, Stride);
        }
        public void SetResolution(float w, float h)
        {
            VerticalResolution = w;
            HorizontalResolution = h;
        }
        public BitmapData LockBits()
        {
            return new BitmapData(Data,Stride, Width, Height, PixelFormat);
        }
        public BitmapData LockBits(Rectangle r, ImageLockMode l, PixelFormat p)
        {
            return new BitmapData(Data, Stride, r.Width, r.Height,p);
        }
        public void UnlockBits(BitmapData d)
        {

        }
        private static int GetStridePadded(int stride)
        {
            if (stride % 4 == 0)
                return stride;
            int newstride = stride + 2;
            if (stride % 3 == 0 && stride % 2 != 0)
            {
                newstride = stride + 1;
                if (newstride % 4 != 0)
                    newstride = stride + 3;
            }
            if (newstride % 4 != 0)
                return stride + 5;
            return newstride;
        }
        private static byte[] GetPaddedBuffer(byte[] bts, int w, int h, int stride, PixelFormat px)
        {
            int newstride = GetStridePadded(stride);
            if (newstride == stride)
                return bts;
            byte[] newbts = new byte[newstride * h];
            if (px == PixelFormat.Format24bppRgb || px == PixelFormat.Format32bppArgb || px == PixelFormat.Format32bppRgb)
            {
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        int index = (y * stride) + x;
                        int index2 = (y * newstride) + x;
                        newbts[index2] = bts[index];
                    }
                }
            }
            else
            {
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w * 2; ++x)
                    {
                        int index = (y * stride) + x;
                        int index2 = (y * newstride) + x;
                        newbts[index2] = bts[index];
                    }
                }
            }
            return newbts;
        }
        public static Bitmap[] RGB48To16(string file, int w, int h, int stride, byte[] bts, ZCT coord, int index, Plane plane)
        {
            Bitmap[] bfs = new Bitmap[3];
            Bitmap bmpr = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            Bitmap bmpg = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            Bitmap bmpb = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            unsafe
            {
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* rowr = (byte*)bmpr.Data + (y * bmpr.Stride);
                    byte* rowg = (byte*)bmpg.Data + (y * bmpg.Stride);
                    byte* rowb = (byte*)bmpb.Data + (y * bmpb.Stride);
                    int rowRGB = y * stride;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        rowr[index16 + 1] = bts[rowRGB + indexRGB];
                        rowr[index16] = bts[rowRGB + indexRGB + 1];
                        //G
                        rowg[index16 + 1] = bts[rowRGB + indexRGB + 2];
                        rowg[index16] = bts[rowRGB + indexRGB + 3];
                        //B
                        rowb[index16 + 1] = bts[rowRGB + indexRGB + 4];
                        rowb[index16] = bts[rowRGB + indexRGB + 5];

                    }
                }
            }
            
            bmpr.RotateFlip(RotateFlipType.Rotate180FlipNone);
            bmpg.RotateFlip(RotateFlipType.Rotate180FlipNone);
            bmpb.RotateFlip(RotateFlipType.Rotate180FlipNone);
            bfs[0] = bmpr;
            bfs[1] = bmpg;
            bfs[2] = bmpb;
            bfs[0].file = file;
            if(plane!=null)
                bfs[0].plane = plane;
            bfs[0].ID = CreateID(file, 0);
            bfs[1].file = file;
            if (plane != null)
                bfs[1].plane = plane;
            bfs[1].ID = CreateID(file, 0);
            bfs[2].file = file;
            if (plane != null)
                bfs[2].plane = plane;
            bfs[2].ID = CreateID(file, 0);
            bfs[0].stats = Statistics.FromBytes(bfs[0]);
            bfs[1].stats = Statistics.FromBytes(bfs[1]);
            bfs[2].stats = Statistics.FromBytes(bfs[2]);
            return bfs;
        }
        public static Bitmap RGB16To48(Bitmap[] bfs)
        {
            //If this is a 2 channel image we fill the last channel with black.
            if (bfs[2] == null)
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 2 * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 2 * 3);
                    int row16 = y * (bfs[0].SizeX * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        bt[rowRGB + indexRGB] = 0;
                        bt[rowRGB + indexRGB + 1] = 0;
                        //G
                        bt[rowRGB + indexRGB + 2] = bfs[1].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 3] = bfs[1].Bytes[row16 + index16 + 1];
                        //B
                        bt[rowRGB + indexRGB + 4] = bfs[0].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 5] = bfs[0].Bytes[row16 + index16 + 1];
                    }
                }
                Bitmap bf = new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format48bppRgb, bt, bfs[0].Coordinate, 0, bfs[0].Plane);
                return bf;
            }
            else
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 2 * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 2 * 3);
                    int row16 = y * (bfs[0].SizeX * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        bt[rowRGB + indexRGB] = bfs[2].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 1] = bfs[2].Bytes[row16 + index16 + 1];
                        //G
                        bt[rowRGB + indexRGB + 2] = bfs[1].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 3] = bfs[1].Bytes[row16 + index16 + 1];
                        //B
                        bt[rowRGB + indexRGB + 4] = bfs[0].Bytes[row16 + index16];
                        bt[rowRGB + indexRGB + 5] = bfs[0].Bytes[row16 + index16 + 1];
                    }
                }
                Bitmap bf = new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format48bppRgb, bt, bfs[0].Coordinate, 0, bfs[0].Plane);
                return bf;
            }
        }
        public static Bitmap RGB16To48(Bitmap bfs)
        {
            byte[] bt = new byte[bfs.SizeY * (bfs.SizeX * 2 * 3)];
            //iterating through all the pixels in y direction
            for (int y = 0; y < bfs.SizeY; y++)
            {
                //getting the pixels of current row
                int rowRGB = y * (bfs.SizeX * 2 * 3);
                int row16 = y * (bfs.SizeX * 2);
                //iterating through all the pixels in x direction
                for (int x = 0; x < bfs.SizeX; x++)
                {
                    int indexRGB = x * 6;
                    int index16 = x * 2;
                    //R
                    bt[rowRGB + indexRGB] = bfs.Bytes[row16 + index16];
                    bt[rowRGB + indexRGB + 1] = bfs.Bytes[row16 + index16 + 1];
                    //G
                    bt[rowRGB + indexRGB + 2] = bfs.Bytes[row16 + index16];
                    bt[rowRGB + indexRGB + 3] = bfs.Bytes[row16 + index16 + 1];
                    //B
                    bt[rowRGB + indexRGB + 4] = bfs.Bytes[row16 + index16];
                    bt[rowRGB + indexRGB + 5] = bfs.Bytes[row16 + index16 + 1];
                }
            }
            Bitmap bf = new Bitmap(bfs.ID, bfs.SizeX, bfs.SizeY, PixelFormat.Format48bppRgb, bt, bfs.Coordinate, 0, bfs.Plane);
            return bf;
        }
        /// It takes a buffer of RGB data, and returns a bitmap
        /// 
        /// @param bfs an array of Bitmap objects.  The first one is the red channel, the second is the green channel, and the third is
        /// the blue channel.
        /// @param IntRange 
        /// @param IntRange 
        /// @param IntRange 
        /// 
        /// @return A Bitmap object.
        public static Bitmap GetRGBBitmap(Bitmap[] bfs, IntRange rr, IntRange rg, IntRange rb)
        {
            if (bfs[0].BitsPerPixel > 8)
            {
                if (bfs[0].isRGB)
                {
                    LevelsLinear16bpp filter16 = new LevelsLinear16bpp();
                    filter16.InRed = rr;
                    filter16.InGreen = rg;
                    filter16.InBlue = rb;
                    Bitmap[] bms = new Bitmap[3];
                    bms[0] = filter16.Apply(bfs[0]);
                    bms[1] = filter16.Apply(bfs[1]);
                    bms[2] = filter16.Apply(bfs[2]);
                    Bitmap bm = Bitmap.RGB16To48(bms);
                    bm.SwitchRedBlue();
                    return bm;
                }
                else
                {
                    LevelsLinear16bpp filter16 = new LevelsLinear16bpp();
                    Bitmap bm = Bitmap.RGB16To48(bfs);
                    filter16.InRed = rr;
                    filter16.InGreen = rg;
                    filter16.InBlue = rb;
                    Bitmap bmp = filter16.Apply(bm);
                    bmp.SwitchRedBlue();
                    return bmp;
                }
            }
            else
            {
                if (bfs[0].isRGB)
                {
                    LevelsLinear filter8 = new LevelsLinear();
                    filter8.InRed = rr;
                    filter8.InGreen = rg;
                    filter8.InBlue = rb;
                    Bitmap[] bms = new Bitmap[3];
                    bms[0] = filter8.Apply(bfs[0]);
                    bms[1] = filter8.Apply(bfs[1]);
                    bms[2] = filter8.Apply(bfs[2]);
                    Bitmap bm = Bitmap.RGB8To24(bms);
                    bm.SwitchRedBlue();
                    return bm;
                }
                else
                {
                    LevelsLinear filter8 = new LevelsLinear();
                    Bitmap bm = Bitmap.RGB8To24(bfs);
                    filter8.InRed = rr;
                    filter8.InGreen = rg;
                    filter8.InBlue = rb;
                    Bitmap bmp = filter8.Apply(bm);
                    return bmp;
                }
            }
        }
        public static Bitmap GetEmissionBitmap(Bitmap bfs, IntRange rr, Color col)
        {
            int stride;
            if (bfs.BitsPerPixel > 8)
                stride = bfs.SizeX * 3 * 2;
            else
                stride = bfs.SizeX * 3;
            float r = (col.R / 255f);
            float g = (col.G / 255f);
            float b = (col.B / 255f);

            int w = bfs.SizeX;
            int h = bfs.SizeY;
            byte[] bts = new byte[h * stride];

            if (bfs.BitsPerPixel > 8)
            {
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (w * 2 * 3);
                    int row16 = y * (w * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;

                        float rf = (BitConverter.ToUInt16(bfs.Bytes, row16 + index16) / (float)ushort.MaxValue) * r;
                        float gf = (BitConverter.ToUInt16(bfs.Bytes, row16 + index16) / (float)ushort.MaxValue) * g;
                        float bf = (BitConverter.ToUInt16(bfs.Bytes, row16 + index16) / (float)ushort.MaxValue) * b;
                        ushort rs = (ushort)(rf * ushort.MaxValue);
                        ushort gs = (ushort)(gf * ushort.MaxValue);
                        ushort bs = (ushort)(bf * ushort.MaxValue);
                        byte[] rbb = BitConverter.GetBytes(rs);
                        byte[] gbb = BitConverter.GetBytes(gs);
                        byte[] bbb = BitConverter.GetBytes(bs);
                        //R
                        bts[rowRGB + indexRGB] = bbb[0];
                        bts[rowRGB + indexRGB + 1] = bbb[1];
                        //G
                        bts[rowRGB + indexRGB + 2] = gbb[0];
                        bts[rowRGB + indexRGB + 3] = gbb[1];
                        //B
                        bts[rowRGB + indexRGB + 4] = rbb[0];
                        bts[rowRGB + indexRGB + 5] = rbb[1];
                    }
                }
            }
            else
            {
                for (int y = 0; y < bfs.SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs.SizeX * 3);
                    int row8 = y * (bfs.SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs.SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;

                        float rf = (bfs.Bytes[row8 + index8] / 255f) * r;
                        float gf = (bfs.Bytes[row8 + index8] / 255f) * g;
                        float bf = (bfs.Bytes[row8 + index8] / 255f) * b;
                        byte rs = (byte)(rf * byte.MaxValue);
                        byte gs = (byte)(gf * byte.MaxValue);
                        byte bs = (byte)(bf * byte.MaxValue);
                        //R
                        bts[rowRGB + indexRGB] = rs;
                        //G
                        bts[rowRGB + indexRGB + 1] = gs;
                        //B
                        bts[rowRGB + indexRGB + 2] = bs;
                    }
                }
            }
            byte[] bt = new byte[h * (w * 3)];
            if (bfs.BitsPerPixel == 8)
            {
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    int row = y * stride;
                    int rowRGB = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 3;
                        int indexRGBA = x * 3;
                        float ri = ((float)bts[rowRGB + indexRGB] - rr.Min);
                        if (ri < 0)
                            ri = 0;
                        ri = ri / rr.Max;
                        float gi = ((float)bts[rowRGB + indexRGB + 1] - rr.Min);
                        if (gi < 0)
                            gi = 0;
                        gi = gi / rr.Max;
                        float bi = ((float)bts[rowRGB + indexRGB + 2] - rr.Min);
                        if (bi < 0)
                            bi = 0;
                        bi = bi / rr.Max;
                        bt[rowRGB + indexRGBA + 2] = (byte)(ri * 255);//byte R
                        bt[rowRGB + indexRGBA + 1] = (byte)(gi * 255);//byte G
                        bt[rowRGB + indexRGBA] = (byte)(bi * 255);//byte B
                    }
                }
            }
            else
            {
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * w * 3 * 2;
                    int row = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int indexRGBA = x * 3;
                        float ri = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) - rr.Min);
                        if (ri < 0)
                            ri = 0;
                        ri = ri / rr.Max;
                        float gi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) - rr.Min);
                        if (gi < 0)
                            gi = 0;
                        gi = gi / rr.Max;
                        float bi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) - rr.Min);
                        if (bi < 0)
                            bi = 0;
                        bi = bi / rr.Max;
                        bt[row + indexRGBA + 2] = (byte)(ri * 255);//byte R
                        bt[row + indexRGBA + 1] = (byte)(gi * 255);//byte G
                        bt[row + indexRGBA] = (byte)(bi * 255);//byte B
                    }
                }
            }
            bts = null;
            Bitmap bmp;
            if (bfs.BitsPerPixel > 8)
                return GetBitmap(w, h, w * 3, PixelFormat.Format24bppRgb, bt, bfs.Coordinate);
            else
                return GetBitmap(w, h, w * 3 * 2, PixelFormat.Format24bppRgb, bt, bfs.Coordinate);
        }
        public static Bitmap GetEmissionBitmap(Bitmap[] bfs, Channel[] chans)
        {
            Bitmap bm = new Bitmap(bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb);
            Merge m = new Merge(bm);
            for (int i = 0; i < chans.Length; i++)
            {
                Bitmap b = GetEmissionBitmap(bfs[i], chans[i].range[0], chans[i].EmissionColor);
                m.OverlayImage = b;
                m.ApplyInPlace(bm);
            }
            return bm;
        }
        public static Bitmap RGB8To24(Bitmap[] bfs)
        {
            //If this is a 2 channel image we fill the last channel with black.
            if (bfs[2] == null)
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 3);
                    int row8 = y * (bfs[0].SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;
                        //R
                        bt[rowRGB + indexRGB] = 0;
                        //G
                        bt[rowRGB + indexRGB + 1] = bfs[1].Bytes[row8 + index8];
                        //B
                        bt[rowRGB + indexRGB + 2] = bfs[0].Bytes[row8 + index8];
                    }
                }
                return new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb, bt, bfs[0].Coordinate, 0);
            }
            else
            {
                byte[] bt = new byte[bfs[0].SizeY * (bfs[0].SizeX * 3)];
                //iterating through all the pixels in y direction
                for (int y = 0; y < bfs[0].SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (bfs[0].SizeX * 3);
                    int row8 = y * (bfs[0].SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < bfs[0].SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;
                        //R
                        bt[rowRGB + indexRGB] = bfs[2].Bytes[row8 + index8];
                        //G
                        bt[rowRGB + indexRGB + 1] = bfs[1].Bytes[row8 + index8];
                        //B
                        bt[rowRGB + indexRGB + 2] = bfs[0].Bytes[row8 + index8];
                    }
                }
                return new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb, bt, bfs[0].Coordinate, 0);
            }
        }
        public static Bitmap RGB8To24(Bitmap bfs)
        {
            byte[] bt = new byte[bfs.SizeY * (bfs.SizeX * 3)];
            //iterating through all the pixels in y direction
            for (int y = 0; y < bfs.SizeY; y++)
            {
                //getting the pixels of current row
                int rowRGB = y * (bfs.SizeX * 3);
                int row8 = y * (bfs.SizeX);
                //iterating through all the pixels in x direction
                for (int x = 0; x < bfs.SizeX; x++)
                {
                    int indexRGB = x * 3;
                    int index8 = x;
                    //R
                    bt[rowRGB + indexRGB] = bfs.Bytes[row8 + index8];
                    //G
                    bt[rowRGB + indexRGB + 1] = bfs.Bytes[row8 + index8];
                    //B
                    bt[rowRGB + indexRGB + 2] = bfs.Bytes[row8 + index8];
                }
            }
            Bitmap bf = new Bitmap(bfs.ID, bfs.SizeX, bfs.SizeY, PixelFormat.Format24bppRgb, bt, bfs.Coordinate, 0);
            return bf;
        }
        public static Bitmap[] RGB24To8(Bitmap info)
        {
            Bitmap[] bfs = new Bitmap[3];
            ExtractChannel cr = new ExtractChannel((short)0);
            ExtractChannel cg = new ExtractChannel((short)1);
            ExtractChannel cb = new ExtractChannel((short)2);
            bfs[0] = cr.Apply(info);
            bfs[1] = cg.Apply(info);
            bfs[2] = cb.Apply(info);
            cr = null;
            cg = null;
            cb = null;
            return bfs;
        }
        public static unsafe Bitmap GetBitmap(int w, int h, int stride, PixelFormat px, byte[] bts, ZCT coord)
        {
                if (stride % 4 == 0)
                {
                    return new Bitmap(w, h, px, bts, coord, "");//new UnmanagedImage(new IntPtr((void*)numPtr1), w, h, stride, px);
                }
                int newstride = GetStridePadded(stride);
                byte[] newbts = GetPaddedBuffer(bts, w, h, stride, px);
                return new Bitmap(w, h, px, newbts, coord, "");
        }
        public void RotateFlip(RotateFlipType rot)
        {
            byte[] rotatedBuffer = new byte[Bytes.Length];
            int ps = (GetPixelFormatSize(pixelFormat) / 8);
            if (rot == RotateFlipType.Rotate180FlipNone || rot == RotateFlipType.Rotate180FlipX || rot == RotateFlipType.Rotate180FlipY || rot == RotateFlipType.Rotate180FlipXY)
            {
                for (int i = 0; i < Bytes.Length; i += PixelFormatSize)
                {
                    int rotatedIndex = Bytes.Length - PixelFormatSize - i;
                    for (int p = 0; p < PixelFormatSize; p++)
                    {
                        rotatedBuffer[i + p] = Bytes[rotatedIndex + p];
                    }
                }
                Bytes = rotatedBuffer;
            }
            else if (rot == RotateFlipType.Rotate90FlipNone || rot == RotateFlipType.Rotate90FlipX || rot == RotateFlipType.Rotate90FlipY || rot == RotateFlipType.Rotate90FlipXY)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int sourceIndex = (y * Width + x) * ps;
                        int targetIndex = ((Width - x - 1) * Height + y) * ps;
                        Array.Copy(Bytes, sourceIndex, rotatedBuffer, targetIndex, ps);
                    }
                }
                Bytes = rotatedBuffer;
                int w = Width;
                SizeX = Height;
                SizeY = w;
            }
            else if (rot == RotateFlipType.Rotate270FlipNone || rot == RotateFlipType.Rotate270FlipX || rot == RotateFlipType.Rotate270FlipY || rot == RotateFlipType.Rotate270FlipXY)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int sourceIndex = (y * Width + x) * ps;
                        int targetIndex = (x * Height + Height - y - 1) * ps;
                        Array.Copy(Bytes, sourceIndex, rotatedBuffer, targetIndex, ps);
                    }
                }
                int w = Width;
                SizeX = Height;
                SizeY = w;
                Bytes = rotatedBuffer;
            }
            
            if (rot == RotateFlipType.RotateNoneFlipY || rot == RotateFlipType.Rotate90FlipY || rot == RotateFlipType.Rotate180FlipY || rot == RotateFlipType.Rotate270FlipY)
            {
                rotatedBuffer = new byte[Bytes.Length];
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int sourceIndex = (y * Width + x) * ps;
                        int targetIndex = (y * Width + Width - x - 1) * ps;
                        Array.Copy(Bytes, sourceIndex, rotatedBuffer, targetIndex, ps);
                    }
                }
                Bytes = rotatedBuffer;
            }
            else if (rot == RotateFlipType.RotateNoneFlipX || rot == RotateFlipType.Rotate90FlipX || rot == RotateFlipType.Rotate180FlipX || rot == RotateFlipType.Rotate270FlipX)
            {
                rotatedBuffer = new byte[Bytes.Length];
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int sourceIndex = (y * Width + x) * ps;
                        int targetIndex = ((Height - y - 1) * Width + x) * ps;
                        Array.Copy(Bytes, sourceIndex, rotatedBuffer, targetIndex, ps);
                    }
                }
                Bytes = rotatedBuffer;
            }
            
        }
        public static unsafe Bitmap GetBitmapRGB(int w, int h, PixelFormat px, byte[] bts)
        {
            if (px == PixelFormat.Format32bppArgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    int rowRGB = y * w * 4;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 4;
                        int indexRGBA = x * 4;
                        row[indexRGBA + 3] = bts[rowRGB + indexRGB + 3];//byte A
                        row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                        row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                        row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else if (px == PixelFormat.Format24bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    int rowRGB = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 3;
                        int indexRGBA = x * 4;
                        row[indexRGBA + 3] = byte.MaxValue;//byte A
                        row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                        row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                        row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 6;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 6;
                            int indexRGBA = x * 4;
                            int b = (int)( ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / (float)ushort.MaxValue) * 255);
                            int g = (int)( ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) / (float)ushort.MaxValue) * 255);
                            int r = (int)( ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) / (float)ushort.MaxValue) * 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x;
                            int indexRGBA = x * 4;
                            byte b = bts[rowRGB + indexRGB];
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 2;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 2;
                            int indexRGBA = x * 4;
                            int b = (int)(((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / (float)ushort.MaxValue) * 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }

            throw new NotSupportedException("Pixelformat " + px + " is not supported.");
        }
        public static unsafe IntPtr GetRGB32Data(int w, int h, PixelFormat px, byte[] bts)
        {
            if(px == PixelFormat.Format32bppArgb || px == PixelFormat.Format32bppRgb)
            {
                fixed (byte* dat = bts)
                {
                    return (IntPtr)dat;
                }
            }
            if (px == PixelFormat.Format24bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    int rowRGB = y * w * 3;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 3;
                        int indexRGBA = x * 4;
                        row[indexRGBA + 3] = byte.MaxValue;//byte A
                        row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                        row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                        row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 6;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 6;
                            int indexRGBA = x * 4;
                            int b = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            int g = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) / 255);
                            int r = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) / 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x;
                            int indexRGBA = x * 4;
                            byte b = bts[rowRGB + indexRGB];
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 2;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 2;
                            int indexRGBA = x * 4;
                            ushort b = (ushort)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmd.Scan0;
            }
            throw new NotSupportedException("Pixelformat " + px + " is not supported.");
        }
        public static unsafe Bitmap GetRGB24Data(int w, int h, PixelFormat px, byte[] bts)
        {
            if (px == PixelFormat.Format24bppRgb)
            {
                return new Bitmap(w, h, px, bts, new ZCT(), "");
            }
            if (px == PixelFormat.Format32bppArgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                //iterating through all the pixels in y direction
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                    int rowRGB = y * w * 4;
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 4;
                        int indexRGBA = x * 3;
                        row[indexRGBA + 2] = bts[rowRGB + indexRGB + 0];//byte B
                        row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                        row[indexRGBA] = bts[rowRGB + indexRGB + 2];//byte R
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 6;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 6;
                            int indexRGBA = x * 3;
                            int b = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            int g = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) / 255);
                            int r = (int)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) / 255);
                            //row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 0] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA + 2] = (byte)(r);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x;
                            int indexRGBA = x * 3;
                            byte b = bts[rowRGB + indexRGB];
                            row[indexRGBA] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA + 2] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * w * 2;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 2;
                            int indexRGBA = x * 3;
                            ushort b = (ushort)((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) / 255);
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            throw new NotSupportedException("Pixelformat " + px + " is not supported.");
        }
        public static Bitmap GetFiltered(int w, int h, int stride, PixelFormat px, byte[] bts, IntRange rr, IntRange rg, IntRange rb)
        {
            if (px == PixelFormat.Format24bppRgb)
            {
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 3;
                            int indexRGBA = x * 3;
                            row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                            row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                            row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                            float ri = ((float)bts[rowRGB + indexRGB] - rr.Min);
                            if (ri < 0)
                                ri = 0;
                            ri = ri / (float)rr.Max;
                            float gi = ((float)bts[rowRGB + indexRGB + 1] - rg.Min);
                            if (gi < 0)
                                gi = 0;
                            gi = gi / (float)rg.Max;
                            float bi = ((float)bts[rowRGB + indexRGB + 2] - rb.Min);
                            if (bi < 0)
                                bi = 0;
                            bi = bi / (float)rb.Max;
                            int b = (int)(ri * 255f);
                            int g = (int)(gi * 255f);
                            int r = (int)(bi * 255f);
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format32bppArgb)
            {
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppRgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 4;
                            int indexRGBA = x * 4;
                            row[indexRGBA + 2] = bts[rowRGB + indexRGB + 2];//byte R
                            row[indexRGBA + 1] = bts[rowRGB + indexRGB + 1];//byte G
                            row[indexRGBA] = bts[rowRGB + indexRGB];//byte B
                            float ri = ((float)bts[rowRGB + indexRGB] - rr.Min);
                            if (ri < 0)
                                ri = 0;
                            ri = ri / (float)rr.Max;
                            float gi = ((float)bts[rowRGB + indexRGB + 1] - rg.Min);
                            if (gi < 0)
                                gi = 0;
                            gi = gi / (float)rg.Max;
                            float bi = ((float)bts[rowRGB + indexRGB + 2] - rb.Min);
                            if (bi < 0)
                                bi = 0;
                            bi = bi / (float)rb.Max;
                            int b = (int)(ri * 255f);
                            int g = (int)(gi * 255f);
                            int r = (int)(bi * 255f);
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }
                //unlocking bits and disposing image
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format48bppRgb)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 6;
                            int indexRGBA = x * 4;
                            float ri = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) - rr.Min);
                            if (ri < 0)
                                ri = 0;
                            ri = ri / (float)rr.Max;
                            float gi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 2) - rg.Min);
                            if (gi < 0)
                                gi = 0;
                            gi = gi / (float)rg.Max;
                            float bi = ((float)BitConverter.ToUInt16(bts, rowRGB + indexRGB + 4) - rb.Min);
                            if (bi < 0)
                                bi = 0;
                            bi = bi / (float)rb.Max;
                            int b = (int)(ri * 255f);
                            int g = (int)(gi * 255f);
                            int r = (int)(bi * 255f);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(g);//byte G
                            row[indexRGBA] = (byte)(r);//byte B
                        }
                    }
                }

                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format16bppGrayScale)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x * 2;
                            int indexRGBA = x * 4;
                            float ri = (float)BitConverter.ToUInt16(bts, rowRGB + indexRGB) - rr.Min;
                            if (ri < 0)
                                ri = 0;
                            ri = ri / rr.Max;
                            int b = (int)(ri * 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }
                bmp.UnlockBits(bmd);
                return bmp;
            }
            else
            if (px == PixelFormat.Format8bppIndexed)
            {
                //opening a 8 bit per pixel jpg image
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                //creating the Bitmapdata and lock bits
                Rectangle rec = new Rectangle(0, 0, w, h);
                BitmapData bmd = bmp.LockBits(rec, ImageLockMode.ReadWrite, bmp.PixelFormat);
                unsafe
                {
                    //iterating through all the pixels in y direction
                    for (int y = 0; y < h; y++)
                    {
                        //getting the pixels of current row
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        int rowRGB = y * stride;
                        //iterating through all the pixels in x direction
                        for (int x = 0; x < w; x++)
                        {
                            int indexRGB = x;
                            int indexRGBA = x * 4;
                            float ri = (float)bts[rowRGB + indexRGB] - rr.Min;
                            if (ri < 0)
                                ri = 0;
                            ri = ri / rr.Max;
                            int b = (int)(ri * 255);
                            row[indexRGBA + 3] = 255;//byte A
                            row[indexRGBA + 2] = (byte)(b);//byte R
                            row[indexRGBA + 1] = (byte)(b);//byte G
                            row[indexRGBA] = (byte)(b);//byte B
                        }
                    }
                }

                bmp.UnlockBits(bmd);
                return bmp;
            }
            throw new InvalidDataException("Bio supports only 8, 16 24, 32, 48 bit images.");
        }
        public Bitmap GetFiltered(IntRange rr, IntRange rg, IntRange rb)
        {
            return Bitmap.GetFiltered(SizeX, SizeY, Stride, PixelFormat, Bytes, rr, rg, rb);
        }
        public void Crop(Rectangle r)
        {
            //This crop function supports 16 bit images unlike Bitmap class.
            if (BitsPerPixel > 8)
            {
                if (RGBChannelsCount == 1)
                {
                    byte[] bts = null;
                    int bytesPer = 2;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                        }
                    }
                    bytes = bts;
                }
                else
                {
                    byte[] bts = null;
                    int bytesPer = 6;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                            bts[indexnew + 2] = bytes[indexold + 2];
                            bts[indexnew + 3] = bytes[indexold + 3];
                            bts[indexnew + 4] = bytes[indexold + 4];
                            bts[indexnew + 5] = bytes[indexold + 5];
                        }
                    }
                    bytes = bts;
                }
            }
            else
            {
                AForge.Imaging.Filters.Crop cr = new Crop(r);
                Image = cr.Apply(Image);
            }
            SizeX = r.Width;
            SizeY = r.Height;
        }
        public UnmanagedImage GetCropBitmap(Rectangle r)
        {
            //This crop function supports 16 bit images unlike Bitmap class.
            if (BitsPerPixel > 8)
            {
                byte[] bts = null;
                if (RGBChannelsCount == 1)
                {
                    int bytesPer = 2;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x) * RGBChannelsCount;
                            int indexold = (((y + r.Y) * strideold + (x + (r.X * bytesPer))) * RGBChannelsCount);// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                        }
                    }
                    return new UnmanagedImage(Marshal.UnsafeAddrOfPinnedArrayElement(bts, 0),r.Width, r.Height, stridenew, PixelFormat.Format16bppGrayScale);
                }
                else
                {
                    int bytesPer = 6;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                            bts[indexnew + 2] = bytes[indexold + 2];
                            bts[indexnew + 3] = bytes[indexold + 3];
                            bts[indexnew + 4] = bytes[indexold + 4];
                            bts[indexnew + 5] = bytes[indexold + 5];
                        }
                    }
                    //bytes = bts;
                    return new UnmanagedImage( Marshal.UnsafeAddrOfPinnedArrayElement(bts, 0),r.Width, r.Height, stridenew, PixelFormat.Format48bppRgb);
                }
            }
            else
            {
                AForge.Imaging.Filters.Crop cr = new Crop(r);
                return cr.Apply(Image);
            }

        }
        public Bitmap GetCropBuffer(Rectangle r)
        {
            //This crop function supports 16 bit images unlike Bitmap class.
            if (BitsPerPixel > 8)
            {
                byte[] bts = null;
                if (RGBChannelsCount == 1)
                {
                    int bytesPer = 2;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x) * RGBChannelsCount;
                            int indexold = (((y + r.Y) * strideold + (x + (r.X * bytesPer))) * RGBChannelsCount);// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                        }
                    }
                    Bitmap bf = new Bitmap(r.Width, r.Height, PixelFormat.Format16bppGrayScale, bts, Coordinate, ID);
                    return bf;
                }
                else
                {
                    int bytesPer = 6;
                    int stridenew = r.Width * bytesPer;
                    int strideold = Stride;
                    bts = new byte[(stridenew * r.Height)];
                    for (int y = 0; y < r.Height; y++)
                    {
                        for (int x = 0; x < stridenew; x += bytesPer)
                        {
                            int indexnew = (y * stridenew + x);
                            int indexold = ((y + r.Y) * strideold + (x + (r.X * bytesPer)));// + r.X;
                            bts[indexnew] = bytes[indexold];
                            bts[indexnew + 1] = bytes[indexold + 1];
                            bts[indexnew + 2] = bytes[indexold + 2];
                            bts[indexnew + 3] = bytes[indexold + 3];
                            bts[indexnew + 4] = bytes[indexold + 4];
                            bts[indexnew + 5] = bytes[indexold + 5];
                        }
                    }
                    Bitmap bf = new Bitmap(r.Width, r.Height, PixelFormat.Format48bppRgb, bts, Coordinate, ID);
                    return bf;
                }
            }
            else
            {
                AForge.Imaging.Filters.Crop cr = new Crop(r);
                return new Bitmap(cr.Apply(Image));
            }
        }
        void Initialize(string file, int w, int h, PixelFormat px, byte[] byts, ZCT coord, int index , Plane plane, bool littleEndian = true, bool interleaved = true)
        {
            ID = CreateID(file, index);
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = coord;
            Bytes = byts;
            if (!interleaved)
            {
                byte[] bts = new byte[Length];
                int strplane = 0;
                if (BitsPerPixel > 8)
                    strplane = w * 2;
                else
                    strplane = w;
                if (RGBChannelsCount == 1)
                {
                    for (int y = 0; y < h; y++)
                    {
                        int x = 0;
                        int str1 = Stride * y;
                        int str2 = strplane * y;
                        for (int st = 0; st < strplane; st++)
                        {
                            bts[str1 + x] = bytes[str2 + st];
                            x++;
                        }
                    }
                }
                else
                {
                    int ind = strplane * h;
                    int indb = ind * 2;
                    for (int y = 0; y < h; y++)
                    {
                        int x = 0;
                        int str1 = Stride * y;
                        int str2 = strplane * y;
                        for (int st = 0; st < strplane; st++)
                        {
                            bts[str1 + x + 2] = bytes[str2 + st];
                            bts[str1 + x + 1] = bytes[ind + str2 + st];
                            bts[str1 + x] = bytes[indb + str2 + st];
                            x += 3;
                        }
                    }
                }
                bytes = bts;
            }
            if (!littleEndian)
            {
                Array.Reverse(Bytes);
                RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            stats = Statistics.FromBytes(this);
        }
        public Bitmap(string file, int w, int h, PixelFormat px, byte[] bts, ZCT coord, int index)
        {
            Initialize(file, w, h, px, bts, coord, index, null);
        }
        public Bitmap(int w, int h, PixelFormat px)
        {
            SizeX = w;
            SizeY = h;
            pixelFormat = px;
            Coordinate = new ZCT();
            Bytes = new byte[h * Stride];
        }
        public Bitmap(string file, int w, int h, PixelFormat px, byte[] byts, ZCT coord, int index, Plane plane, bool littleEndian = true, bool interleaved = true)
        {
            Initialize(file,w,h,px,byts,coord,index,plane,littleEndian,interleaved);
        }
        public Bitmap(string file, UnmanagedImage im, ZCT coord, int index)
        {
            ID = CreateID(file, index);
            SizeX = im.Width;
            SizeY = im.Height;
            pixelFormat = im.PixelFormat;
            Coordinate = coord;
            Image = im;
            stats = Statistics.FromBytes(this);
        }
        public Bitmap(string file, UnmanagedImage im, ZCT coord, int index, Plane pl)
        {
            ID = CreateID(file, index);
            SizeX = im.Width;
            SizeY = im.Height;
            pixelFormat = im.PixelFormat;
            Coordinate = coord;
            Image = im;
            Plane = pl;
            stats = Statistics.FromBytes(this);
        }
        public Bitmap(int width, int height, int stride, PixelFormat pixelFormat, IntPtr imageData)
        {
            SizeX = width;
            SizeY = height;
            this.pixelFormat= pixelFormat;
            bytes = new byte[stride * height];
            Marshal.Copy(imageData, bytes, 0, stride * height);
            stats = Statistics.FromBytes(this);
        }
        public Bitmap(UnmanagedImage im)
        {
            SizeX = im.Width;
            SizeY = im.Height;
            pixelFormat = im.PixelFormat;
            Coordinate = new ZCT();
            Image = im;
            stats = Statistics.FromBytes(this);
        }
        public Bitmap(int w, int h, PixelFormat px, byte[] bts, ZCT coord, string id)
        {
            Initialize(id, w, h, px, bts, coord, 0, null);
        }
        public static UnmanagedImage SwitchRedBlue(UnmanagedImage image)
        {
            return SwitchRedBlue(new Bitmap(image)).Image;
        }
        public static Bitmap SwitchRedBlue(Bitmap image)
        {
            ExtractChannel cr = new ExtractChannel(AForge.Imaging.RGB.R);
            ExtractChannel cb = new ExtractChannel(AForge.Imaging.RGB.B);
            // apply the filter
            UnmanagedImage rImage = cr.Apply(image.Image);
            UnmanagedImage bImage = cb.Apply(image.Image);

            ReplaceChannel replaceRFilter = new ReplaceChannel(AForge.Imaging.RGB.R, bImage);
            replaceRFilter.ApplyInPlace(image.Image);

            ReplaceChannel replaceBFilter = new ReplaceChannel(AForge.Imaging.RGB.B, rImage);
            replaceBFilter.ApplyInPlace(image.Image);
            rImage.Dispose();
            bImage.Dispose();
            return image;
        }
        public void SwitchRedBlue()
        {
            if (PixelFormat == PixelFormat.Format8bppIndexed || PixelFormat == PixelFormat.Format16bppGrayScale || Bytes == null)
                return;
            //Bitmap bf = new Bitmap(SizeX, SizeY,PixelFormat, bytes, Coordinate, ID);
            if (PixelFormat == PixelFormat.Format24bppRgb)
                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < Stride; x += 3)
                    {
                        int i = y * Stride + x;
                        byte bb = bytes[i + 2];
                        bytes[i + 2] = bytes[i];
                        bytes[i] = bb;
                    }
                }
            else
            if (PixelFormat == PixelFormat.Format32bppArgb || PixelFormat == PixelFormat.Format32bppRgb)
                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < Stride; x += 4)
                    {
                        int i = y * Stride + x;
                        byte bb = bytes[i + 2];
                        bytes[i + 2] = bytes[i];
                        bytes[i] = bb;
                    }
                }
            else
            if (PixelFormat == PixelFormat.Format48bppRgb)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (Stride);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < Stride; x += 6)
                    {
                        int indexRGB = x;
                        byte b1 = bytes[rowRGB + indexRGB];
                        byte b2 = bytes[rowRGB + indexRGB + 1];
                        //B
                        bytes[rowRGB + indexRGB] = bytes[rowRGB + indexRGB + 4];
                        bytes[rowRGB + indexRGB + 1] = bytes[rowRGB + indexRGB + 5];
                        //R
                        bytes[rowRGB + indexRGB + 4] = b1;
                        bytes[rowRGB + indexRGB + 5] = b2;
                    }
                }
            }
        }
        public byte[] GetSaveBytes(bool littleEndian)
        {
            Bitmap bf = this.Copy();
            if (!littleEndian)
            {
                bf.SwitchRedBlue();
                bf.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            int length = this.bytes.Length;
            byte[] bytes = new byte[length];
            Marshal.Copy(bf.Data, bytes, 0, length);
            if (!littleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
        public static byte[] GetBuffer(Bitmap bmp, int stride)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), AForge.ImageLockMode.ReadOnly, bmp.PixelFormat);
            IntPtr ptr = data.Scan0;
            int length = data.Stride * bmp.Height;
            byte[] bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            Array.Reverse(bytes);
            bmp.UnlockBits(data);

            return bytes;
        }
        public static Bitmap To24Bit(Bitmap b)
        {
            return GetRGB24Data(b.Width, b.Height, b.PixelFormat, b.Bytes);
        }
        public static Bitmap To32Bit(Bitmap b)
        {
            return b.ImageRGB;
        }
        public static Bitmap SwitchChannels(Bitmap image, int c1, int c2)
        {
            ExtractChannel cr = new ExtractChannel((short)c1);
            ExtractChannel cb = new ExtractChannel((short)c2);
            // apply the filter
            UnmanagedImage rImage = cr.Apply(image.Image);
            UnmanagedImage bImage = cb.Apply(image.Image);
            ReplaceChannel replaceRFilter = new ReplaceChannel((short)c1, bImage);
            replaceRFilter.ApplyInPlace(image.Image);
            ReplaceChannel replaceBFilter = new ReplaceChannel((short)c2, rImage);
            replaceBFilter.ApplyInPlace(image.Image);
            rImage.Dispose();
            bImage.Dispose();
            return image;
        }
        public Bitmap Copy()
        {
            byte[] bt = new byte[Bytes.Length];
            for (int i = 0; i < bt.Length; i++)
            {
                bt[i] = bytes[i];
            }
            Bitmap bf = new Bitmap(SizeX, SizeY, PixelFormat, bt, Coordinate, ID);
            bf.plane = Plane;
            return bf;
        }
        public Bitmap CopyInfo()
        {
            Bitmap bf = new Bitmap(SizeX, SizeY, PixelFormat, new byte[Stride * SizeY], Coordinate, ID);
            bf.bytes = new byte[bf.Stride * bf.SizeY];
            bf.plane = Plane;
            return bf;
        }
        public void To8Bit()
        {
            Bitmap bm = AForge.Imaging.Image.Convert16bppTo8bpp(this);
            bm.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Image = bm;
        }
        public void To16Bit()
        {
            Bitmap bm = AForge.Imaging.Image.Convert8bppTo16bpp(this);
            Image = bm;
        }
        /*
        public static explicit operator Bitmap(UnmanagedImage v)
        {
            return new Bitmap(v);
        }
        */
        public void ToRGB()
        {
            int stride;
            if (BitsPerPixel > 8)
                stride = SizeX * 3 * 2;
            else
                stride = SizeX * 3;

            int w = SizeX;
            int h = SizeY;
            byte[] bts = null;
            if (PixelFormat == PixelFormat.Format48bppRgb)
            {
                return;
            }
            else
            if (PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                bts = new byte[h * SizeX * 3 * 2];
                for (int y = 0; y < h; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (w * 2 * 3);
                    int row16 = y * (w * 2);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < w; x++)
                    {
                        int indexRGB = x * 6;
                        int index16 = x * 2;
                        //R
                        bts[rowRGB + indexRGB] = Bytes[row16 + index16];
                        bts[rowRGB + indexRGB + 1] = Bytes[row16 + index16 + 1];
                        //G
                        bts[rowRGB + indexRGB + 2] = Bytes[row16 + index16];
                        bts[rowRGB + indexRGB + 3] = Bytes[row16 + index16 + 1];
                        //B
                        bts[rowRGB + indexRGB + 4] = Bytes[row16 + index16];
                        bts[rowRGB + indexRGB + 5] = Bytes[row16 + index16 + 1];
                    }
                }
                Bytes = bts;
                PixelFormat = PixelFormat.Format48bppRgb;
            }
            else
            if (PixelFormat == PixelFormat.Format24bppRgb)
            {
                return;
            }
            else
            if (PixelFormat == PixelFormat.Format8bppIndexed)
            {
                bts = new byte[h * SizeX * 3];
                for (int y = 0; y < SizeY; y++)
                {
                    //getting the pixels of current row
                    int rowRGB = y * (SizeX * 3);
                    int row8 = y * (SizeX);
                    //iterating through all the pixels in x direction
                    for (int x = 0; x < SizeX; x++)
                    {
                        int indexRGB = x * 3;
                        int index8 = x;
                        bts[rowRGB + indexRGB] = Bytes[row8 + index8];
                        bts[rowRGB + indexRGB + 1] = Bytes[row8 + index8];
                        bts[rowRGB + indexRGB + 2] = Bytes[row8 + index8];
                    }
                }
                Bytes = bts;
                PixelFormat = PixelFormat.Format24bppRgb;
            }
        }
        public bool isRGB
        {
            get
            {
                if (pixelFormat == PixelFormat.Format8bppIndexed || pixelFormat == PixelFormat.Format16bppGrayScale)
                    return false;
                else
                    return true;
            }
        }
        public override string ToString()
        {
            return ID;
        }
        public void Dispose()
        {
            bytes = null;
            if (stats != null)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    if (stats[i] != null)
                        stats[i].Dispose();
                }
            }
            ID = null;
            file = null;
            GC.Collect();
        }
        public static Bitmap operator /(Bitmap a, Bitmap b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) / b.GetPixel(x, y));
                }
            }
            return bf;
        }
        public static Bitmap operator *(Bitmap a, Bitmap b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) * b.GetPixel(x, y));
                }
            }
            return bf;
        }
        public static Bitmap operator +(Bitmap a, Bitmap b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) + b.GetPixel(x, y));
                }
            }
            return bf;
        }
        public static Bitmap operator -(Bitmap a, Bitmap b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) - b.GetPixel(x, y));
                }
            }
            return bf;
        }

        public static Bitmap operator /(Bitmap a, float b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) / b);
                }
            }
            return bf;
        }
        public static Bitmap operator *(Bitmap a, float b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) * b);
                }
            }
            return bf;
        }
        public static Bitmap operator +(Bitmap a, float b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) + b);
                }
            }
            return bf;
        }
        public static Bitmap operator -(Bitmap a, float b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) - b);
                }
            }
            return bf;
        }

        public static Bitmap operator /(Bitmap a, ColorS b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) / b);
                }
            }
            return bf;
        }
        public static Bitmap operator *(Bitmap a, ColorS b)
        {
            Bitmap bf = a.Copy();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) * b);
                }
            }
            return bf;
        }
        public static Bitmap operator +(Bitmap a, ColorS b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) + b);
                }
            }
            return bf;
        }
        public static Bitmap operator -(Bitmap a, ColorS b)
        {
            Bitmap bf = a.CopyInfo();
            for (int y = 0; y < a.SizeY; y++)
            {
                for (int x = 0; x < a.SizeX; x++)
                {
                    bf.SetPixel(x, y, a.GetPixel(x, y) - b);
                }
            }
            return bf;
        }
    }
}
