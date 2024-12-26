using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Cairo;
using Gtk;
using Pango;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Channels;

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
        Float,
        UInt,
        Int,
        Short,
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
        public static Color Violet = FromArgb(255, 238, 130, 238);
        public static Color Brown = FromArgb(255, 165, 42, 42);
        public static Color Olive = FromArgb(255, 128, 128, 0);
        public static Color Gold = FromArgb(255, 255, 215, 0);
        public static Color Indigo = FromArgb(255, 75, 0, 130);
        public static Color Ivory = FromArgb(255, 255, 255, 240);
        public static Color HotPink = FromArgb(255, 255, 105, 180);
        public static Color DarkSeaGreen = FromArgb(255, 143, 188, 143);
        public static Color LimeGreen = FromArgb(255, 50, 205, 50);
        public static Color Tomato = FromArgb(255, 255, 99, 71);
        public static Color SteelBlue = FromArgb(255, 70, 130, 180);
        public static Color SkyBlue = FromArgb(255, 135, 206, 235);
        public static Color Silver = FromArgb(255, 192, 192, 192);
        public static Color Salmon = FromArgb(255, 250, 128, 114);
        public static Color SaddleBrown = FromArgb(255, 139, 69, 19);
        public static Color RosyBrown = FromArgb(255, 188, 143, 143);
        public static Color PowderBlue = FromArgb(255, 176, 224, 230);
        public static Color Plum = FromArgb(255, 221, 160, 221);
        public static Color PapayaWhip = FromArgb(255, 255, 239, 213);
        public static Color Orange = FromArgb(255, 255, 165, 0);

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
        public bool IntersectsWith(RectangleD other)
        {
            return !(other.X > this.X + this.W ||   // Other is to the right
                     other.X + other.W < this.X || // Other is to the left
                     other.Y > this.Y + this.H || // Other is below
                     other.Y + other.H < this.Y); // Other is above
        }
        public bool IntersectsWith(PointD point)
        {
            return (point.X >= X &&
                    point.X <= X + W &&
                    point.Y >= Y &&
                    point.Y <= Y + H);
        }
        public bool IntersectsWith(double x, double y)
        {
            return IntersectsWith(new PointD(x, y));
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
        public bool IntersectsWith(RectangleD other)
        {
            return !(other.X > this.X + this.Width ||   // Other is to the right
                     other.X + other.W < this.X || // Other is to the left
                     other.Y > this.Y + this.Height || // Other is below
                     other.Y + other.H < this.Y); // Other is above
        }
        public bool IntersectsWith(RectangleF other)
        {
            return !(other.X > this.X + this.Width ||   // Other is to the right
                     other.X + other.Width < this.X || // Other is to the left
                     other.Y > this.Y + this.Height || // Other is below
                     other.Y + other.Height < this.Y); // Other is above
        }
        public bool IntersectsWith(PointD point)
        {
            return (point.X >= X &&
                    point.X <= X + Width &&
                    point.Y >= Y &&
                    point.Y <= Y + Height);
        }
        public bool IntersectsWith(double x, double y)
        {
            return IntersectsWith(new PointD(x, y));
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
        public bool IntersectsWith(RectangleD other)
        {
            return !(other.X > this.X + this.Width ||   // Other is to the right
                     other.X + other.W < this.X || // Other is to the left
                     other.Y > this.Y + this.Height || // Other is below
                     other.Y + other.H < this.Y); // Other is above
        }
        public bool IntersectsWith(PointD point)
        {
            return (point.X >= X &&
                    point.X <= X + Width &&
                    point.Y >= Y &&
                    point.Y <= Y + Height);
        }
        public bool IntersectsWith(double x, double y)
        {
            return IntersectsWith(new PointD(x, y));
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
        private float min = ushort.MaxValue;
        private float max = ushort.MinValue;
        private float stackMin = ushort.MaxValue;
        private float stackMax = ushort.MinValue;
        private float stackMean = 0;
        private float stackMedian = 0;
        private float mean = 0;
        private float meansum = 0;
        private float[] stackValues = new float[ushort.MaxValue];
        private int count = 0;
        public IntRange Range
        {
            get
            {
                return new IntRange((int)Min, (int)Max);
            }
        }
        public float Min
        {
            get { return min; }
        }
        public float Max
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
                if (values != null)
                {
                    float max = float.MinValue;
                    for (int i = 0; i < values.Length; i++)
                    {
                        if(values[i] > max)
                            max = values[i];
                    }
                    return max;
                }
                else
                {
                    float max = float.MinValue;
                    for (int i = 0; i < stackValues.Length; i++)
                    {
                        if (values[i] > max)
                            max = values[i];
                    }
                    return max;
                }
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
        public Statistics()
        {
            values = new int[ushort.MaxValue + 1];
        }
        public Statistics(int bitsPerPixel)
        {
            values = new int[ushort.MaxValue + 1];
            this.bitsPerPixel = bitsPerPixel;
        }
        public Statistics(bool val)
        {
            values = new int[ushort.MaxValue + 1];
            if (val)
                this.bitsPerPixel = 16;
            else
                this.bitsPerPixel = 8;
        }
        public static Statistics[] FromBytes(byte[] bytes, int width, int height, int rgbChannels, int bitsPerPixel, int stride, PixelFormat pixelFormat)
        {
            try
            {
                Statistics[] stats = new Statistics[rgbChannels];

                // Initialize each channel's statistics
                for (int i = 0; i < rgbChannels; i++)
                {
                    stats[i] = new Statistics(bitsPerPixel)
                    {
                        max = ushort.MinValue,
                        min = ushort.MaxValue,
                        bitsPerPixel = bitsPerPixel
                    };
                }

                // Sum values to compute mean
                double sumR = 0, sumG = 0, sumB = 0, sumA = 0;

                // Process different pixel formats
                switch (pixelFormat)
                {
                    case PixelFormat.Format16bppGrayScale:
                    case PixelFormat.Format48bppRgb:
                        {
                            int bytesPerChannel = bitsPerPixel / 8;
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < stride; x += bytesPerChannel * rgbChannels)
                                {
                                    if (rgbChannels == 3) // RGB
                                    {
                                        ushort b = BitConverter.ToUInt16(bytes, y * stride + x);
                                        ushort g = BitConverter.ToUInt16(bytes, y * stride + x + 2);
                                        ushort r = BitConverter.ToUInt16(bytes, y * stride + x + 4);

                                        UpdateStatistics(stats[0], r, ref sumR);
                                        UpdateStatistics(stats[1], g, ref sumG);
                                        UpdateStatistics(stats[2], b, ref sumB);
                                    }
                                    else // Grayscale
                                    {
                                        ushort gray = BitConverter.ToUInt16(bytes, y * stride + x);
                                        UpdateStatistics(stats[0], gray, ref sumR);
                                    }
                                }
                            }
                        }
                        break;

                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppArgb:
                        {
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width * rgbChannels; x += rgbChannels)
                                {
                                    if (rgbChannels == 4) // ARGB
                                    {
                                        byte a = bytes[y * stride + x];
                                        byte b = bytes[y * stride + x + 1];
                                        byte g = bytes[y * stride + x + 2];
                                        byte r = bytes[y * stride + x + 3];

                                        UpdateStatistics(stats[0], a, ref sumA);
                                        UpdateStatistics(stats[1], b, ref sumB);
                                        UpdateStatistics(stats[2], g, ref sumG);
                                        UpdateStatistics(stats[3], r, ref sumR);
                                    }
                                    else if (rgbChannels == 3) // RGB
                                    {
                                        byte b = bytes[y * stride + x];
                                        byte g = bytes[y * stride + x + 1];
                                        byte r = bytes[y * stride + x + 2];

                                        UpdateStatistics(stats[0], r, ref sumR);
                                        UpdateStatistics(stats[1], g, ref sumG);
                                        UpdateStatistics(stats[2], b, ref sumB);
                                    }
                                    else // Grayscale
                                    {
                                        byte gray = bytes[y * stride + x];
                                        UpdateStatistics(stats[0], gray, ref sumR);
                                    }
                                }
                            }
                        }
                        break;

                    case PixelFormat.Short:
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                short value = BitConverter.ToInt16(bytes, y * stride + x);
                                UpdateStatistics(stats[0], value, ref sumR);
                            }
                        }
                        break;

                    case PixelFormat.Float:
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < stride; x+=4)
                            {
                                float f = BitConverter.ToSingle(bytes, y * stride + x);
                                float value = System.Math.Abs(f);
                                UpdateStatistics(stats[0], value, ref sumR);
                            }
                        }
                        break;
                    case PixelFormat.UInt:
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                uint value = BitConverter.ToUInt32(bytes, y * stride + x);
                                UpdateStatistics(stats[0], value, ref sumR);
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException($"{pixelFormat} is not supported.");
                }

                // Calculate mean values
                stats[0].mean = (float)sumR / (width * height);
                if (rgbChannels > 1)
                {
                    stats[1].mean = (float)sumG / (width * height);
                    stats[2].mean = (float)sumB / (width * height);
                    if (rgbChannels == 4) stats[3].mean = (float)sumA / (width * height);
                }
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates the min, max, and sum of statistics for a given channel.
        /// </summary>
        private static void UpdateStatistics(Statistics stat, dynamic value, ref double sum)
        {
            if (value < 0)
                value = 0;
            if(value > ushort.MaxValue)
                value = ushort.MaxValue;
            if (stat.max < value)
                stat.max = value;
            if (stat.min > value) 
                stat.min = value;
            if(value < stat.values.Length)
                stat.values[(int)value]++;
            sum += System.Math.Abs(value);
        }
        public static Statistics[] FromBytes(Bitmap bf)
        {
            return FromBytes(bf.Bytes, bf.SizeX, bf.SizeY, bf.RGBChannelsCount, bf.BitsPerPixel, bf.Stride, bf.PixelFormat);
        }
        public static Dictionary<string, Bitmap> list = new Dictionary<string, Bitmap>();
        public static void FromBytes()
        {
            string name = System.Threading.Thread.CurrentThread.Name;
            list[name].Stats = FromBytes(list[name]);
            list.Remove(name);
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
            return AForge.Color.FromArgb((ushort)r, (ushort)g, (ushort)b);
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
            if (bitsPerPixel == 32)
                info.Max = 1;
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
        WriteOnly,
    }
    public class LUT
    {
        Bitmap ApplyLut(Bitmap originalBitmap, byte[] lut)
        {
            // Lock the bitmap's bits
            Rectangle rect = new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height);
            BitmapData bmpData = originalBitmap.LockBits(rect, ImageLockMode.ReadWrite, originalBitmap.PixelFormat);

            // Get the address of the first line
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap
            int bytes = System.Math.Abs(bmpData.Stride) * originalBitmap.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Apply the LUT to each pixel
            for (int i = 0; i < rgbValues.Length; i++)
            {
                rgbValues[i] = lut[rgbValues[i]];
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits
            originalBitmap.UnlockBits(bmpData);

            return originalBitmap;
        }
        Bitmap ApplyLut16Bit(Bitmap originalBitmap, ushort[] lut)
        {
            // Lock the bitmap's bits
            Rectangle rect = new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height);
            BitmapData bmpData = originalBitmap.LockBits(rect, ImageLockMode.ReadWrite, originalBitmap.PixelFormat);

            // Get the address of the first line
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap
            int bytes = System.Math.Abs(bmpData.Stride) * originalBitmap.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Apply the LUT to each pixel (16 bit)
            for (int i = 0; i < rgbValues.Length; i += 2)
            {
                ushort pixelValue = BitConverter.ToUInt16(rgbValues, i);
                ushort newPixelValue = lut[pixelValue];

                byte[] newPixelBytes = BitConverter.GetBytes(newPixelValue);
                rgbValues[i] = newPixelBytes[0];
                rgbValues[i + 1] = newPixelBytes[1];
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits
            originalBitmap.UnlockBits(bmpData);

            return originalBitmap;
        }
    }
    public class Bitmap : IDisposable
    {
        private float horizontalRes = 96f;
        private float verticalRes = 96f;
        internal Bitmap.ColorPalette palette = new Bitmap.ColorPalette();
        string id;
        public string ID
        {
            get 
            {
                if (id == "" || id == null)
                    return Guid.NewGuid().ToString();
                else return id;
            }
            set
            {
                id = value;
            }
        }
        public int SizeX;
        public int SizeY;
        public ZCT Coordinate;
        private PixelFormat pixelFormat;
        private Statistics[] stats;
        private byte[] bytes;
        private string file;
        private bool littleEndian = BitConverter.IsLittleEndian;
        public Plane Plane;

        public ColorS GetPixel(int ix, int iy)
        {
            if (RGBChannelsCount == 3 || RGBChannelsCount == 4)
                return new ColorS((ushort)GetValue(ix, iy, 0), (ushort)GetValue(ix, iy, 1), (ushort)GetValue(ix, iy, 2));
            else if (RGBChannelsCount == 1)
                return new ColorS((ushort)GetValue(ix, iy, 0));
            else
                throw new NotSupportedException("PixelFormat " + PixelFormat.ToString());
        }

        public void SetPixel(int ix, int iy, ColorS col)
        {
            if (BitsPerPixel > 8)
            {
                this.SetValue(ix, iy, 0, (ushort)col.R);
                this.SetValue(ix, iy, 2, (ushort)col.G);
                this.SetValue(ix, iy, 4, (ushort)col.B);
            }
            else
            {
                this.SetValue(ix, iy, 0, (byte)col.R);
                this.SetValue(ix, iy, 1, (byte)col.G);
                this.SetValue(ix, iy, 2, (byte)col.B);
            }
        }
        public void SetPixel(int ix, int iy, Color col)
        {
            if (BitsPerPixel > 8)
            {
                this.SetValue(ix, iy, 0, (ushort)col.R);
                this.SetValue(ix, iy, 2, (ushort)col.G);
                this.SetValue(ix, iy, 4, (ushort)col.B);
            }
            else
            {
                this.SetValue(ix, iy, 0, (byte)col.R);
                this.SetValue(ix, iy, 1, (byte)col.G);
                this.SetValue(ix, iy, 2, (byte)col.B);
            }
        }

        public void SetPixel(int ix, int iy, ushort f)
        {
            this.SetValue(ix, iy, f);
        }

        public void SetPixel(int ix, int iy, float f)
        {
            this.SetValue(ix, iy, f);
        }
        public void SetValue(int x, int y, uint value)
        {
            SetValue(x, y, 0, value);
        }
        public void SetValue(int x, int y, byte value)
        {
            SetValue(x, y, 0, value);
        }
        public void SetValue(int x, int y, ushort value)
        {
            SetValue(x, y, 0, value);
        }
        public void SetValue(int x, int y, float value)
        {
            SetValue(x, y, 0, value);
        }
        public void SetValue(int x, int y, int channel, uint value)
        {
            try
            {
                byte[] bts = BitConverter.GetBytes(value);
                int index = y * Stride + (x * PixelFormatSize) + (channel * 4);
                for (int i = 0; i < bts.Length; i++)
                {
                    bytes[index + i] = bts[i];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public void SetValue(int x, int y, int channel, float value)
        {
            try
            {
                byte[] bts = BitConverter.GetBytes(value);
                int index = y * Stride + (x * PixelFormatSize) + (channel * 4);
                for (int i = 0; i < bts.Length; i++)
                {
                    bytes[index + i] = bts[i];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public void SetValue(int x, int y, int channel, ushort value)
        {
            try
            {
                byte[] bts = BitConverter.GetBytes(value);
                int index = y * Stride + (x * PixelFormatSize) + channel;
                for (int i = 0; i < bts.Length; i++)
                {
                    bytes[index + i] = bts[i];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public void SetValue(int x, int y, int channel, byte value)
        {
            try
            {
                // Calculate the index in the byte array
                int index = y * Stride + (x * PixelFormatSize) + channel;

                // Set the value at the calculated index
                bytes[index] = value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public float GetValue(int x, int y, int RGBChannel = 0)
        {
            int index;
            try
            {
                index = y * Stride + (x * PixelFormatSize) + RGBChannel;
                byte[] bts = new byte[4];
                if (PixelFormat == PixelFormat.Format16bppGrayScale)
                    bts = new byte[2];
                else if (PixelFormat == PixelFormat.Float)
                    bts = new byte[4];
                else if (PixelFormat == PixelFormat.Short)
                    bts = new byte[2];
                for (int i = 0; i < bts.Length; i++)
                {
                    bts[i] = bytes[index + i];
                }
                if (PixelFormat == PixelFormat.Format8bppIndexed)
                    return bytes[index];
                else if (PixelFormat == PixelFormat.Format16bppGrayScale)
                    return BitConverter.ToUInt16(bts);
                else if (PixelFormat == PixelFormat.Float)
                    return BitConverter.ToSingle(bts);
                else if (PixelFormat == PixelFormat.Short)
                    return BitConverter.ToInt16(bts);
                else if (PixelFormat == PixelFormat.Format24bppRgb || PixelFormat == PixelFormat.Format32bppRgb || PixelFormat == PixelFormat.Format32bppArgb)
                    return bytes[index];
                else if (PixelFormat == PixelFormat.Format48bppRgb)
                {
                    index = y * Stride + (x * PixelFormatSize) + (RGBChannel * 2);
                    return BitConverter.ToUInt16(bytes, index);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return 0;
        }

        public static string CreateID(string filepath, int index)
        {
            if (filepath == null)
                return "";
            filepath = filepath.Replace("\\", "/");
            return filepath + "/i/" + index.ToString();
        }

        public string File
        {
            get => this.file;
            set => this.file = value;
        }

        public int Width => this.SizeX;

        public int Height => this.SizeY;

        public int Stride
        {
            get
            {
                switch (PixelFormat)
                {
                    case PixelFormat.Indexed:
                        return SizeX;
                    case PixelFormat.Format1bppIndexed:
                        return SizeX;
                    case PixelFormat.Format4bppIndexed:
                        return SizeX;
                    case PixelFormat.Format8bppIndexed:
                        return SizeX;
                    case PixelFormat.Format16bppGrayScale:
                        return SizeX * 2;
                    case PixelFormat.Format24bppRgb:
                        return SizeX * 3;
                    case PixelFormat.Format32bppArgb:
                        return SizeX * 4;
                    case PixelFormat.Format32bppRgb:
                        return SizeX * 4;
                    case PixelFormat.Format32bppPArgb:
                        return SizeX * 4;
                    case PixelFormat.Format48bppRgb:
                        return SizeX * 6;
                    case PixelFormat.Format64bppArgb:
                        return SizeX * 8;
                    case PixelFormat.Format64bppPArgb:
                        return SizeX * 8;
                    case PixelFormat.Float:
                        return SizeX * 4;
                    case PixelFormat.Short: 
                        return SizeX * 2;
                    default:
                        return SizeX;
                }
            }
        }
        public int PaddedStride => Bitmap.GetStridePadded(this.Stride);

        public bool LittleEndian
        {
            get => this.littleEndian;
            set => this.littleEndian = value;
        }

        public Gdk.Pixbuf GetPixbuf()
        {
            // Create the Pixbuf object with the interleaved data
            Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(GetRGBABytes(), true, 8, Width, Height, Width * 4);
            return pixbuf;
        }

        public void BinarizeOtsu()
        {
            Bitmap bm;
            OtsuThreshold otsu = new OtsuThreshold();
            ExtractChannel ch = new ExtractChannel(0);
            bm = ch.Apply(this.GetImageRGBA());
            otsu.ApplyInPlace(bm);
            this.Bytes = bm.bytes;
            this.PixelFormat = bm.PixelFormat;
            this.SizeX = bm.SizeX;
            this.SizeY = bm.SizeY;
        }

        public long Length => (long)this.bytes.Length;

        public int RGBChannelsCount
        {
            get
            {
                switch (PixelFormat)
                {
                    case PixelFormat.Indexed:
                        return 1;
                    case PixelFormat.Format1bppIndexed:
                        return 1;
                    case PixelFormat.Format4bppIndexed:
                        return 1;
                    case PixelFormat.Format8bppIndexed:
                        return 1;
                    case PixelFormat.Format16bppGrayScale:
                        return 1;
                    case PixelFormat.Format24bppRgb:
                        return 3;
                    case PixelFormat.Format32bppArgb:
                        return 4;
                    case PixelFormat.Format32bppRgb:
                        return 3;
                    case PixelFormat.Format32bppPArgb:
                        return 4;
                    case PixelFormat.Format48bppRgb:
                        return 3;
                    case PixelFormat.Format64bppArgb:
                        return 4;
                    case PixelFormat.Format64bppPArgb:
                        return 4;
                    case PixelFormat.Float:
                        return 1;
                    case PixelFormat.Short:
                        return 1;
                    default:
                        throw new NotSupportedException("Pixel format not supported.");
                }
            }
        }

        public int BitsPerPixel
        {
            get
            {
                switch (PixelFormat)
                {
                    case PixelFormat.Indexed:
                        return 8;
                    case PixelFormat.Format1bppIndexed:
                        return 1;
                    case PixelFormat.Format4bppIndexed:
                        return 4;
                    case PixelFormat.Format8bppIndexed:
                        return 8;
                    case PixelFormat.Format16bppGrayScale:
                        return 16;
                    case PixelFormat.Format24bppRgb:
                        return 8;
                    case PixelFormat.Format32bppArgb:
                        return 8;
                    case PixelFormat.Format32bppRgb:
                        return 8;
                    case PixelFormat.Format32bppPArgb:
                        return 8;
                    case PixelFormat.Format48bppRgb:
                        return 16;
                    case PixelFormat.Format64bppArgb:
                        return 16;
                    case PixelFormat.Format64bppPArgb:
                        return 16;
                    case PixelFormat.Float:
                        return 32;
                    case PixelFormat.Int:
                        return 32;
                    case PixelFormat.UInt:
                        return 32;
                    case PixelFormat.Short:
                        return 16;
                    default:
                        throw new NotSupportedException("Pixel format not supported.");
                }
            }
        }

        public PixelFormat PixelFormat
        {
            get => this.pixelFormat;
            set => this.pixelFormat = value;
        }

        public byte[] Bytes
        {
            get => this.bytes;
            set => this.bytes = value;
        }

        public byte[] GetPaddedBytes()
        {
           return Bitmap.GetPaddedBuffer(this.bytes, this.SizeX, this.SizeY, this.Stride, this.PixelFormat);
        }
        public unsafe UnmanagedImage Image
        {
            get
            {
                fixed (byte* numPtr = this.GetPaddedBytes())
                    return new UnmanagedImage((IntPtr)(void*)numPtr, this.SizeX, this.SizeY, this.PaddedStride, this.PixelFormat);
            }
            set
            {
                this.PixelFormat = value.PixelFormat;
                this.SizeX = value.Width;
                this.SizeY = value.Height;
                this.bytes = new byte[this.PaddedStride * value.Height];
                Marshal.Copy(value.ImageData, this.Bytes, 0, this.PaddedStride * value.Height);
            }
        }

        public byte[] GetRGBABytes()
        {
            return Bitmap.GetBitmapRGBA(this.SizeX, this.SizeY, this.PixelFormat, this.Bytes).Bytes;
        }
        public Bitmap GetImageRGBA(bool normalized = false)
        {
            return Bitmap.GetBitmapRGBA(this.SizeX, this.SizeY, this.PixelFormat, this.Bytes, normalized);
        }
        public Bitmap GetImageRGB(bool normalized = false)
        {
            return Bitmap.GetBitmapRGB(this.SizeX, this.SizeY, this.PixelFormat, this.Bytes, normalized);
        }
        public unsafe IntPtr Data
        {
            get
            {
                fixed (byte* numPtr = this.Bytes)
                    return (IntPtr)(void*)numPtr;
            }
        }

        public IntPtr GetRGBData()
        {
            return Bitmap.GetRGB32Data(this.SizeX, this.SizeY, this.PixelFormat, this.bytes);
        }
       
        public int PixelFormatSize
        {
            get
            {
                switch (PixelFormat)
                {
                    case PixelFormat.Indexed:
                        return 1;
                    case PixelFormat.Format1bppIndexed:
                        return 1;
                    case PixelFormat.Format4bppIndexed:
                        return 1;
                    case PixelFormat.Format8bppIndexed:
                        return 1;
                    case PixelFormat.Format16bppGrayScale:
                        return 2;
                    case PixelFormat.Format24bppRgb:
                        return 3;
                    case PixelFormat.Format32bppArgb:
                        return 4;
                    case PixelFormat.Format32bppRgb:
                        return 4;
                    case PixelFormat.Format32bppPArgb:
                        return 4;
                    case PixelFormat.Format48bppRgb:
                        return 6;
                    case PixelFormat.Format64bppArgb:
                        return 8;
                    case PixelFormat.Format64bppPArgb:
                        return 8;
                    case PixelFormat.Float:
                        return 4;
                    case PixelFormat.UInt:
                        return 4;
                    case PixelFormat.Int:
                        return 4;
                    case PixelFormat.Short:
                        return 2;
                    default:
                        return 1;
                }
            }
        }

        public static int GetPixelFormatSize(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format4bppIndexed:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 8;
                case PixelFormat.Format16bppGrayScale:
                    return 16;
                case PixelFormat.Format24bppRgb:
                    return 24;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppPArgb:
                    return 32;
                case PixelFormat.Format48bppRgb:
                    return 48;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;
                case PixelFormat.Float:
                    return 32;
                case PixelFormat.Short:
                    return 16;
                default:
                    throw new NotSupportedException();
            }
        }

        public Bitmap.ColorPalette Palette
        {
            get => this.palette;
            set => this.palette = value;
        }

        public float VerticalResolution
        {
            get => this.verticalRes;
            set => this.verticalRes = value;
        }

        public float HorizontalResolution
        {
            get => this.horizontalRes;
            set => this.horizontalRes = value;
        }

        public Statistics[] Stats
        {
            get => this.stats;
            set => this.stats = value;
        }

        public void SetImage(Bitmap Bitmap, bool switchRGB)
        {
            if (switchRGB)
                Bitmap = Bitmap.SwitchRedBlue(Bitmap);
            this.PixelFormat = Bitmap.PixelFormat;
            this.SizeX = Bitmap.Width;
            this.SizeY = Bitmap.Height;
            this.bytes = Bitmap.GetBuffer(Bitmap, this.Stride);
        }

        public void SetResolution(float w, float h)
        {
            this.VerticalResolution = w;
            this.HorizontalResolution = h;
        }

        public BitmapData LockBits() => new BitmapData(this.Data, this.Stride, this.Width, this.Height, this.PixelFormat);

        public BitmapData LockBits(Rectangle r, ImageLockMode l, PixelFormat p) => new BitmapData(this.Data, this.Stride, r.Width, r.Height, p);

        public void UnlockBits(BitmapData d)
        {
        }

        private static int GetStridePadded(int stride)
        {
            if (stride % 4 == 0)
                return stride;
            int num = stride + 2;
            if (stride % 3 == 0 && stride % 2 != 0)
            {
                num = stride + 1;
                if (num % 4 != 0)
                    num = stride + 3;
            }
            return num % 4 != 0 ? stride + 5 : num;
        }

        private static byte[] GetPaddedBuffer(byte[] bts, int w, int h, int stride, PixelFormat px)
        {
            int stridePadded = Bitmap.GetStridePadded(stride);
            if (stridePadded == stride)
                return bts;
            byte[] numArray = new byte[stridePadded * h];
            if (px == PixelFormat.Format24bppRgb || px == PixelFormat.Format32bppArgb || px == PixelFormat.Format32bppRgb || px == PixelFormat.Format8bppIndexed)
            {
                for (int index1 = 0; index1 < h; ++index1)
                {
                    for (int index2 = 0; index2 < w; ++index2)
                    {
                        int index3 = index1 * stride + index2;
                        int index4 = index1 * stridePadded + index2;
                        numArray[index4] = bts[index3];
                    }
                }
            }
            else
            {
                for (int index5 = 0; index5 < h; ++index5)
                {
                    for (int index6 = 0; index6 < w * 2; ++index6)
                    {
                        int index7 = index5 * stride + index6;
                        int index8 = index5 * stridePadded + index6;
                        numArray[index8] = bts[index7];
                    }
                }
            }
            return numArray;
        }

        public static unsafe Bitmap[] RGB48To16(
            string file,
            int w,
            int h,
            int stride,
            byte[] bts,
            ZCT coord,
            int index,
            Plane plane)
        {
            Bitmap[] bitmapArray = new Bitmap[3];
            Bitmap bitmap1 = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            Bitmap bitmap2 = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            Bitmap bitmap3 = new Bitmap(w, h, PixelFormat.Format16bppGrayScale);
            for (int index1 = 0; index1 < h; ++index1)
            {
                byte* numPtr1 = (byte*)((IntPtr)(void*)bitmap1.Data + index1 * bitmap1.Stride);
                byte* numPtr2 = (byte*)((IntPtr)(void*)bitmap2.Data + index1 * bitmap2.Stride);
                byte* numPtr3 = (byte*)((IntPtr)(void*)bitmap3.Data + index1 * bitmap3.Stride);
                int num1 = index1 * stride;
                for (int index2 = 0; index2 < w; ++index2)
                {
                    int num2 = index2 * 6;
                    int index3 = index2 * 2;
                    numPtr1[index3 + 1] = bts[num1 + num2];
                    numPtr1[index3] = bts[num1 + num2 + 1];
                    numPtr2[index3 + 1] = bts[num1 + num2 + 2];
                    numPtr2[index3] = bts[num1 + num2 + 3];
                    numPtr3[index3 + 1] = bts[num1 + num2 + 4];
                    numPtr3[index3] = bts[num1 + num2 + 5];
                }
            }
            bitmap1.RotateFlip(RotateFlipType.Rotate180FlipNone);
            bitmap2.RotateFlip(RotateFlipType.Rotate180FlipNone);
            bitmap3.RotateFlip(RotateFlipType.Rotate180FlipNone);
            bitmapArray[0] = bitmap1;
            bitmapArray[1] = bitmap2;
            bitmapArray[2] = bitmap3;
            bitmapArray[0].file = file;
            if (plane != null)
                bitmapArray[0].Plane = plane;
            bitmapArray[0].ID = Bitmap.CreateID(file, 0);
            bitmapArray[1].file = file;
            if (plane != null)
                bitmapArray[1].Plane = plane;
            bitmapArray[1].ID = Bitmap.CreateID(file, 0);
            bitmapArray[2].file = file;
            if (plane != null)
                bitmapArray[2].Plane = plane;
            bitmapArray[2].ID = Bitmap.CreateID(file, 0);
            bitmapArray[0].stats = Statistics.FromBytes(bitmapArray[0]);
            bitmapArray[1].stats = Statistics.FromBytes(bitmapArray[1]);
            bitmapArray[2].stats = Statistics.FromBytes(bitmapArray[2]);
            return bitmapArray;
        }

        public static Bitmap RGB16To48(Bitmap[] bfs)
        {
            if (bfs[2] == null)
            {
                byte[] byts = new byte[bfs[0].SizeY * (bfs[0].SizeX * 2 * 3)];
                for (int index1 = 0; index1 < bfs[0].SizeY; ++index1)
                {
                    int num1 = index1 * (bfs[0].SizeX * 2 * 3);
                    int num2 = index1 * (bfs[0].SizeX * 2);
                    for (int index2 = 0; index2 < bfs[0].SizeX; ++index2)
                    {
                        int num3 = index2 * 6;
                        int num4 = index2 * 2;
                        byts[num1 + num3] = bfs[0].Bytes[num2 + num4];
                        byts[num1 + num3 + 1] = bfs[0].Bytes[num2 + num4 + 1];
                        byts[num1 + num3 + 2] = bfs[1].Bytes[num2 + num4];
                        byts[num1 + num3 + 3] = bfs[1].Bytes[num2 + num4 + 1];
                        byts[num1 + num3 + 4] = (byte)0;
                        byts[num1 + num3 + 5] = (byte)0;
                    }
                }
                return new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format48bppRgb, byts, bfs[0].Coordinate, 0, bfs[0].Plane);
            }
            byte[] byts1 = new byte[bfs[0].SizeY * (bfs[0].SizeX * 2 * 3)];
            for (int index3 = 0; index3 < bfs[0].SizeY; ++index3)
            {
                int num5 = index3 * (bfs[0].SizeX * 2 * 3);
                int num6 = index3 * (bfs[0].SizeX * 2);
                for (int index4 = 0; index4 < bfs[0].SizeX; ++index4)
                {
                    int num7 = index4 * 6;
                    int num8 = index4 * 2;
                    byts1[num5 + num7] = bfs[0].Bytes[num6 + num8];
                    byts1[num5 + num7 + 1] = bfs[0].Bytes[num6 + num8 + 1];
                    byts1[num5 + num7 + 2] = bfs[1].Bytes[num6 + num8];
                    byts1[num5 + num7 + 3] = bfs[1].Bytes[num6 + num8 + 1];
                    byts1[num5 + num7 + 4] = bfs[2].Bytes[num6 + num8];
                    byts1[num5 + num7 + 5] = bfs[2].Bytes[num6 + num8 + 1];
                }
            }
            return new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format48bppRgb, byts1, bfs[0].Coordinate, 0, bfs[0].Plane);
        }

        public static Bitmap RGB16To48(Bitmap bfs)
        {
            byte[] byts = new byte[bfs.SizeY * (bfs.SizeX * 2 * 3)];
            for (int index1 = 0; index1 < bfs.SizeY; ++index1)
            {
                int num1 = index1 * (bfs.SizeX * 2 * 3);
                int num2 = index1 * (bfs.SizeX * 2);
                for (int index2 = 0; index2 < bfs.SizeX; ++index2)
                {
                    int num3 = index2 * 6;
                    int num4 = index2 * 2;
                    byts[num1 + num3] = bfs.Bytes[num2 + num4];
                    byts[num1 + num3 + 1] = bfs.Bytes[num2 + num4 + 1];
                    byts[num1 + num3 + 2] = bfs.Bytes[num2 + num4];
                    byts[num1 + num3 + 3] = bfs.Bytes[num2 + num4 + 1];
                    byts[num1 + num3 + 4] = bfs.Bytes[num2 + num4];
                    byts[num1 + num3 + 5] = bfs.Bytes[num2 + num4 + 1];
                }
            }
            return new Bitmap(bfs.ID, bfs.SizeX, bfs.SizeY, PixelFormat.Format48bppRgb, byts, bfs.Coordinate, 0, bfs.Plane);
        }

        public static Bitmap GetRGBBitmap(Bitmap[] bfs, IntRange rr, IntRange rg, IntRange rb)
        {
            if (bfs[0].PixelFormat == PixelFormat.Float)
            {
                Bitmap[] bf = new Bitmap[bfs.Length];
                for (int i = 0; i < bf.Length; i++)
                {
                    bf[i] = (Bitmap)bfs[i].MemberwiseClone();
                    bf[i].To8Bit(true);
                }
                LevelsLinear levelsLinear = new LevelsLinear();
                levelsLinear.InRed = rr;
                levelsLinear.InGreen = rg;
                levelsLinear.InBlue = rb;
                Bitmap bitmap = Bitmap.RGB8To24(new Bitmap[3]
                {
                    levelsLinear.Apply(bf[0]),
                    levelsLinear.Apply(bf[1]),
                    levelsLinear.Apply(bf[2])
                });
                bitmap.SwitchRedBlue();
                return bitmap;
            }
            if (bfs[0].BitsPerPixel > 8)
            {
                if (bfs[0].isRGB)
                {
                    LevelsLinear16bpp levelsLinear16bpp = new LevelsLinear16bpp();
                    levelsLinear16bpp.InRed = rr;
                    levelsLinear16bpp.InGreen = rg;
                    levelsLinear16bpp.InBlue = rb;
                    Bitmap bitmap = Bitmap.RGB16To48(new Bitmap[3]
                    {
                        levelsLinear16bpp.Apply(bfs[0]),
                        levelsLinear16bpp.Apply(bfs[1]),
                        levelsLinear16bpp.Apply(bfs[2])
                    });
                    bitmap.SwitchRedBlue();
                    return bitmap;
                }
                LevelsLinear16bpp levelsLinear16bpp1 = new LevelsLinear16bpp();
                Bitmap image = Bitmap.RGB16To48(bfs);
                levelsLinear16bpp1.InRed = rr;
                levelsLinear16bpp1.InGreen = rg;
                levelsLinear16bpp1.InBlue = rb;
                Bitmap bitmap1 = levelsLinear16bpp1.Apply(image);
                bitmap1.SwitchRedBlue();
                return bitmap1;
            }
            if (bfs[0].isRGB)
            {
                LevelsLinear levelsLinear = new LevelsLinear();
                levelsLinear.InRed = rr;
                levelsLinear.InGreen = rg;
                levelsLinear.InBlue = rb;
                Bitmap bitmap = Bitmap.RGB8To24(new Bitmap[3]
                {
                    levelsLinear.Apply(bfs[0]),
                    levelsLinear.Apply(bfs[1]),
                    levelsLinear.Apply(bfs[2])
                });
                bitmap.SwitchRedBlue();
                return bitmap;
            }
            LevelsLinear levelsLinear1 = new LevelsLinear();
            Bitmap image1 = Bitmap.RGB8To24(bfs);
            levelsLinear1.InRed = rr;
            levelsLinear1.InGreen = rg;
            levelsLinear1.InBlue = rb;
            return levelsLinear1.Apply(image1);
        }
        /// It takes a buffer, a color, and a range, and returns a bitmap with the color applied to the
        /// buffer
        /// 
        /// @param BufferInfo This is a class that contains the following properties:
        /// @param IntRange This is a struct that contains a min and max value.
        /// @param col The color to use for the emission.
        /// 
        /// @return A bitmap.
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
                        bts[rowRGB + indexRGB] = rbb[0];
                        bts[rowRGB + indexRGB + 1] = rbb[1];
                        //G
                        bts[rowRGB + indexRGB + 2] = gbb[0];
                        bts[rowRGB + indexRGB + 3] = gbb[1];
                        //B
                        bts[rowRGB + indexRGB + 4] = bbb[0];
                        bts[rowRGB + indexRGB + 5] = bbb[1];
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
                        bt[row + indexRGBA] = (byte)(ri * 255);//byte R
                        bt[row + indexRGBA + 1] = (byte)(gi * 255);//byte G
                        bt[row + indexRGBA + 2] = (byte)(bi * 255);//byte B
                    }
                }
            }
            bts = null;
            Bitmap bmp;
            return new Bitmap("", w, h, PixelFormat.Format24bppRgb, bt, bfs.Coordinate, 0);
        }
        /// It takes a list of buffer info objects and a list of channel objects and returns a bitmap of
        /// the emission data
        /// 
        /// @param bfs an array of BufferInfo objects, each of which contains a buffer of data (a
        /// float[]) and the size of the buffer (SizeX and SizeY).
        /// @param chans an array of Channel objects, which are defined as:
        /// 
        /// @return A bitmap of the emission image.
        public static Bitmap GetEmissionBitmap(Bitmap[] bfs, Channel[] chans)
        {
            Bitmap bm;
            bm = new Bitmap(bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb);
            Merge m = new Merge(bm);
            for (int i = 0; i < chans.Length; i++)
            {
                m.OverlayImage = bm;
                Bitmap b = GetEmissionBitmap(bfs[i], chans[i].range[0], chans[i].EmissionColor);
                bm = m.Apply(b);
            }
            return bm;
        }
        public static Bitmap RGB8To24(Bitmap[] bfs)
        {
            if (bfs[2] == null)
            {
                byte[] bts = new byte[bfs[0].SizeY * (bfs[0].SizeX * 3)];
                for (int index1 = 0; index1 < bfs[0].SizeY; ++index1)
                {
                    int num1 = index1 * (bfs[0].SizeX * 3);
                    int num2 = index1 * bfs[0].SizeX;
                    for (int index2 = 0; index2 < bfs[0].SizeX; ++index2)
                    {
                        int num3 = index2 * 3;
                        int num4 = index2;
                        bts[num1 + num3 + 2] = (byte)0;
                        bts[num1 + num3 + 1] = bfs[1].Bytes[num2 + num4];
                        bts[num1 + num3] = bfs[0].Bytes[num2 + num4];
                    }
                }
                return new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb, bts, bfs[0].Coordinate, 0);
            }
            byte[] bts1 = new byte[bfs[0].SizeY * (bfs[0].SizeX * 3)];
            for (int index3 = 0; index3 < bfs[0].SizeY; ++index3)
            {
                int num5 = index3 * (bfs[0].SizeX * 3);
                int num6 = index3 * bfs[0].SizeX;
                for (int index4 = 0; index4 < bfs[0].SizeX; ++index4)
                {
                    int num7 = index4 * 3;
                    int num8 = index4;
                    bts1[num5 + num7 + 2] = bfs[2].Bytes[num6 + num8];
                    bts1[num5 + num7 + 1] = bfs[1].Bytes[num6 + num8];
                    bts1[num5 + num7] = bfs[0].Bytes[num6 + num8];
                }
            }
            return new Bitmap(bfs[0].ID, bfs[0].SizeX, bfs[0].SizeY, PixelFormat.Format24bppRgb, bts1, bfs[0].Coordinate, 0);
        }

        public static Bitmap RGB8To24(Bitmap bfs)
        {
            byte[] bts = new byte[bfs.SizeY * (bfs.SizeX * 3)];
            for (int index1 = 0; index1 < bfs.SizeY; ++index1)
            {
                int num1 = index1 * (bfs.SizeX * 3);
                int num2 = index1 * bfs.SizeX;
                for (int index2 = 0; index2 < bfs.SizeX; ++index2)
                {
                    int num3 = index2 * 3;
                    int num4 = index2;
                    bts[num1 + num3 + 2] = bfs.Bytes[num2 + num4];
                    bts[num1 + num3 + 1] = bfs.Bytes[num2 + num4];
                    bts[num1 + num3] = bfs.Bytes[num2 + num4];
                }
            }
            return new Bitmap(bfs.ID, bfs.SizeX, bfs.SizeY, PixelFormat.Format24bppRgb, bts, bfs.Coordinate, 0);
        }

        public static Bitmap[] RGB24To8(Bitmap info)
        {
            Bitmap[] bitmapArray = new Bitmap[3];
            ExtractChannel extractChannel1 = new ExtractChannel((short)0);
            ExtractChannel extractChannel2 = new ExtractChannel((short)1);
            ExtractChannel extractChannel3 = new ExtractChannel((short)2);
            bitmapArray[0] = extractChannel1.Apply(info);
            bitmapArray[1] = extractChannel2.Apply(info);
            bitmapArray[2] = extractChannel3.Apply(info);
            return bitmapArray;
        }

        public static Bitmap GetBitmap(
            int w,
            int h,
            int stride,
            PixelFormat px,
            byte[] bts,
            ZCT coord)
        {
            if (stride % 4 == 0)
                return new Bitmap(w, h, px, bts, coord, "");
            Bitmap.GetStridePadded(stride);
            byte[] paddedBuffer = Bitmap.GetPaddedBuffer(bts, w, h, stride, px);
            return new Bitmap(w, h, px, paddedBuffer, coord, "");
        }

        public void RotateFlip(RotateFlipType rot)
        {
            byte[] numArray1 = new byte[this.Bytes.Length];
            int length = Bitmap.GetPixelFormatSize(this.pixelFormat) / 8;
            if (rot == RotateFlipType.Rotate180FlipNone || rot == RotateFlipType.Rotate180FlipX || rot == RotateFlipType.Rotate180FlipY || rot == RotateFlipType.Rotate180FlipXY)
            {
                for (int index1 = 0; index1 < this.Bytes.Length; index1 += this.PixelFormatSize)
                {
                    int num = this.Bytes.Length - this.PixelFormatSize - index1;
                    for (int index2 = 0; index2 < this.PixelFormatSize; ++index2)
                        numArray1[index1 + index2] = this.Bytes[num + index2];
                }
                this.Bytes = numArray1;
            }
            else if (rot == RotateFlipType.Rotate90FlipNone || rot == RotateFlipType.Rotate90FlipX || rot == RotateFlipType.Rotate90FlipY || rot == RotateFlipType.Rotate90FlipXY)
            {
                for (int index3 = 0; index3 < this.Height; ++index3)
                {
                    for (int index4 = 0; index4 < this.Width; ++index4)
                    {
                        int sourceIndex = (index3 * this.Width + index4) * length;
                        int destinationIndex = ((this.Width - index4 - 1) * this.Height + index3) * length;
                        Array.Copy((Array)this.Bytes, sourceIndex, (Array)numArray1, destinationIndex, length);
                    }
                }
                this.Bytes = numArray1;
                int width = this.Width;
                this.SizeX = this.Height;
                this.SizeY = width;
            }
            else if (rot == RotateFlipType.Rotate270FlipNone || rot == RotateFlipType.Rotate270FlipX || rot == RotateFlipType.Rotate270FlipY || rot == RotateFlipType.Rotate270FlipXY)
            {
                for (int index5 = 0; index5 < this.Height; ++index5)
                {
                    for (int index6 = 0; index6 < this.Width; ++index6)
                    {
                        int sourceIndex = (index5 * this.Width + index6) * length;
                        int destinationIndex = (index6 * this.Height + this.Height - index5 - 1) * length;
                        Array.Copy((Array)this.Bytes, sourceIndex, (Array)numArray1, destinationIndex, length);
                    }
                }
                int width = this.Width;
                this.SizeX = this.Height;
                this.SizeY = width;
                this.Bytes = numArray1;
            }
            if (rot == RotateFlipType.RotateNoneFlipY || rot == RotateFlipType.Rotate90FlipY || rot == RotateFlipType.Rotate180FlipY || rot == RotateFlipType.Rotate270FlipY)
            {
                byte[] numArray2 = new byte[this.Bytes.Length];
                for (int index7 = 0; index7 < this.Height; ++index7)
                {
                    for (int index8 = 0; index8 < this.Width; ++index8)
                    {
                        int sourceIndex = (index7 * this.Width + index8) * length;
                        int destinationIndex = (index7 * this.Width + this.Width - index8 - 1) * length;
                        Array.Copy((Array)this.Bytes, sourceIndex, (Array)numArray2, destinationIndex, length);
                    }
                }
                this.Bytes = numArray2;
            }
            else
            {
                if (rot != RotateFlipType.RotateNoneFlipX && rot != RotateFlipType.Rotate90FlipX && rot != RotateFlipType.Rotate180FlipX && rot != RotateFlipType.Rotate270FlipX)
                    return;
                byte[] numArray3 = new byte[this.Bytes.Length];
                for (int index9 = 0; index9 < this.Height; ++index9)
                {
                    for (int index10 = 0; index10 < this.Width; ++index10)
                    {
                        int sourceIndex = (index9 * this.Width + index10) * length;
                        int destinationIndex = ((this.Height - index9 - 1) * this.Width + index10) * length;
                        Array.Copy((Array)this.Bytes, sourceIndex, (Array)numArray3, destinationIndex, length);
                    }
                }
                this.Bytes = numArray3;
            }
        }
        public static byte[] RotateFlip(RotateFlipType rot, Bitmap bm)
        {
            byte[] numArray1 = new byte[bm.Bytes.Length];
            int length = Bitmap.GetPixelFormatSize(bm.pixelFormat) / 8;
            if (rot == RotateFlipType.Rotate180FlipNone || rot == RotateFlipType.Rotate180FlipX || rot == RotateFlipType.Rotate180FlipY || rot == RotateFlipType.Rotate180FlipXY)
            {
                for (int index1 = 0; index1 < bm.Bytes.Length; index1 += bm.PixelFormatSize)
                {
                    int num = bm.Bytes.Length - bm.PixelFormatSize - index1;
                    for (int index2 = 0; index2 < bm.PixelFormatSize; ++index2)
                        numArray1[index1 + index2] = bm.Bytes[num + index2];
                }
                return numArray1;
            }
            else if (rot == RotateFlipType.Rotate90FlipNone || rot == RotateFlipType.Rotate90FlipX || rot == RotateFlipType.Rotate90FlipY || rot == RotateFlipType.Rotate90FlipXY)
            {
                for (int index3 = 0; index3 < bm.Height; ++index3)
                {
                    for (int index4 = 0; index4 < bm.Width; ++index4)
                    {
                        int sourceIndex = (index3 * bm.Width + index4) * length;
                        int destinationIndex = ((bm.Width - index4 - 1) * bm.Height + index3) * length;
                        Array.Copy((Array)bm.Bytes, sourceIndex, (Array)numArray1, destinationIndex, length);
                    }
                }
                return numArray1;
            }
            else if (rot == RotateFlipType.Rotate270FlipNone || rot == RotateFlipType.Rotate270FlipX || rot == RotateFlipType.Rotate270FlipY || rot == RotateFlipType.Rotate270FlipXY)
            {
                for (int index5 = 0; index5 < bm.Height; ++index5)
                {
                    for (int index6 = 0; index6 < bm.Width; ++index6)
                    {
                        int sourceIndex = (index5 * bm.Width + index6) * length;
                        int destinationIndex = (index6 * bm.Height + bm.Height - index5 - 1) * length;
                        Array.Copy((Array)bm.Bytes, sourceIndex, (Array)numArray1, destinationIndex, length);
                    }
                }
                return numArray1;
            }
            if (rot == RotateFlipType.RotateNoneFlipY || rot == RotateFlipType.Rotate90FlipY || rot == RotateFlipType.Rotate180FlipY || rot == RotateFlipType.Rotate270FlipY)
            {
                for (int index7 = 0; index7 < bm.Height; ++index7)
                {
                    for (int index8 = 0; index8 < bm.Width; ++index8)
                    {
                        int sourceIndex = (index7 * bm.Width + index8) * length;
                        int destinationIndex = (index7 * bm.Width + bm.Width - index8 - 1) * length;
                        Array.Copy((Array)bm.Bytes, sourceIndex, (Array)numArray1, destinationIndex, length);
                    }
                }
                return numArray1;
            }
            else
            {
                if (rot != RotateFlipType.RotateNoneFlipX && rot != RotateFlipType.Rotate90FlipX && rot != RotateFlipType.Rotate180FlipX && rot != RotateFlipType.Rotate270FlipX)
                    return bm.bytes;
                for (int index9 = 0; index9 < bm.Height; ++index9)
                {
                    for (int index10 = 0; index10 < bm.Width; ++index10)
                    {
                        int sourceIndex = (index9 * bm.Width + index10) * length;
                        int destinationIndex = ((bm.Height - index9 - 1) * bm.Width + index10) * length;
                        Array.Copy((Array)bm.Bytes, sourceIndex, (Array)numArray1, destinationIndex, length);
                    }
                }
                return numArray1;
            }
        }

        public static unsafe Bitmap GetBitmapRGBA(int w, int h, PixelFormat px, byte[] bts, bool normalized = false)
        {
            switch (px)
            {
                case PixelFormat.Format8bppIndexed:
                    Bitmap bitmap1 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r1 = new Rectangle(0, 0, w, h);
                    BitmapData d1 = bitmap1.LockBits(r1, ImageLockMode.ReadWrite, bitmap1.PixelFormat);
                    for (int index1 = 0; index1 < h; ++index1)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d1.Scan0 + index1 * d1.Stride);
                        int num1 = index1 * w;
                        for (int index2 = 0; index2 < w; ++index2)
                        {
                            int num2 = index2;
                            int index3 = index2 * 4;
                            byte bt = bts[num1 + num2];
                            numPtr[index3 + 3] = byte.MaxValue;
                            numPtr[index3 + 2] = bt;
                            numPtr[index3 + 1] = bt;
                            numPtr[index3] = bt;
                        }
                    }
                    bitmap1.UnlockBits(d1);
                    return bitmap1;
                case PixelFormat.Format16bppGrayScale:
                    Bitmap bitmap2 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r2 = new Rectangle(0, 0, w, h);
                    BitmapData d2 = bitmap2.LockBits(r2, ImageLockMode.ReadWrite, bitmap2.PixelFormat);
                    for (int index4 = 0; index4 < h; ++index4)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d2.Scan0 + index4 * d2.Stride);
                        int num3 = index4 * w * 2;
                        for (int index5 = 0; index5 < w; ++index5)
                        {
                            int num4 = index5 * 2;
                            int index6 = index5 * 4;
                            int num5 = (int)((double)BitConverter.ToUInt16(bts, num3 + num4) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            numPtr[index6 + 3] = byte.MaxValue;
                            numPtr[index6 + 2] = (byte)num5;
                            numPtr[index6 + 1] = (byte)num5;
                            numPtr[index6] = (byte)num5;
                        }
                    }
                    bitmap2.UnlockBits(d2);
                    return bitmap2;
                case PixelFormat.Format24bppRgb:
                    Bitmap bitmap3 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r3 = new Rectangle(0, 0, w, h);
                    BitmapData d3 = bitmap3.LockBits(r3, ImageLockMode.ReadWrite, bitmap3.PixelFormat);
                    for (int index7 = 0; index7 < h; ++index7)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d3.Scan0 + index7 * d3.Stride);
                        int num6 = index7 * w * 3;
                        for (int index8 = 0; index8 < w; ++index8)
                        {
                            int num7 = index8 * 3;
                            int index9 = index8 * 4;
                            numPtr[index9 + 3] = byte.MaxValue;
                            numPtr[index9 + 2] = bts[num6 + num7 + 2];
                            numPtr[index9 + 1] = bts[num6 + num7 + 1];
                            numPtr[index9] = bts[num6 + num7];
                        }
                    }
                    bitmap3.UnlockBits(d3);
                    return bitmap3;
                case PixelFormat.Format32bppArgb:
                    Bitmap bitmap4 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r4 = new Rectangle(0, 0, w, h);
                    BitmapData d4 = bitmap4.LockBits(r4, ImageLockMode.ReadWrite, bitmap4.PixelFormat);
                    for (int index10 = 0; index10 < h; ++index10)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d4.Scan0 + index10 * d4.Stride);
                        int num8 = index10 * w * 4;
                        for (int index11 = 0; index11 < w; ++index11)
                        {
                            int num9 = index11 * 4;
                            int index12 = index11 * 4;
                            numPtr[index12 + 3] = bts[num8 + num9 + 3];
                            numPtr[index12 + 2] = bts[num8 + num9 + 2];
                            numPtr[index12 + 1] = bts[num8 + num9 + 1];
                            numPtr[index12] = bts[num8 + num9];
                        }
                    }
                    bitmap4.UnlockBits(d4);
                    return bitmap4;
                case PixelFormat.Format48bppRgb:
                    Bitmap bitmap5 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r5 = new Rectangle(0, 0, w, h);
                    BitmapData d5 = bitmap5.LockBits(r5, ImageLockMode.ReadWrite, bitmap5.PixelFormat);
                    for (int index13 = 0; index13 < h; ++index13)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d5.Scan0 + index13 * d5.Stride);
                        int num10 = index13 * w * 6;
                        for (int index14 = 0; index14 < w; ++index14)
                        {
                            int num11 = index14 * 6;
                            int index15 = index14 * 4;
                            int num12 = (int)((double)BitConverter.ToUInt16(bts, num10 + num11) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            int num13 = (int)((double)BitConverter.ToUInt16(bts, num10 + num11 + 2) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            int num14 = (int)((double)BitConverter.ToUInt16(bts, num10 + num11 + 4) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            numPtr[index15 + 3] = byte.MaxValue;
                            numPtr[index15 + 2] = (byte)num12;
                            numPtr[index15 + 1] = (byte)num13;
                            numPtr[index15] = (byte)num14;
                        }
                    }
                    bitmap5.UnlockBits(d5);
                    return bitmap5;
                case PixelFormat.Float:
                    Bitmap bf = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle rf = new Rectangle(0, 0, w, h);
                    BitmapData df = bf.LockBits(rf, ImageLockMode.ReadWrite, bf.PixelFormat);
                    for (int y = 0; y < h; ++y)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)df.Scan0 + y * df.Stride);
                        int num = y * w * 4;
                        for (int x = 0; x < w; ++x)
                        {
                            int index = x * 4;
                            if (normalized)
                            {
                                float f = BitConverter.ToSingle(bts, index + num) * 255;
                                numPtr[index + 0] = (byte)(f);
                                numPtr[index + 1] = (byte)(f);
                                numPtr[index + 2] = (byte)(f);
                                numPtr[index + 3] = byte.MaxValue;
                            }
                            else
                            {
                                float f = BitConverter.ToSingle(bts, index + num);
                                numPtr[index + 0] = (byte)(f);
                                numPtr[index + 1] = (byte)(f);
                                numPtr[index + 2] = (byte)(f);
                                numPtr[index + 3] = byte.MaxValue;
                            }
                        }
                    }
                    bf.UnlockBits(df);
                    return bf;
                case PixelFormat.Short:
                    Bitmap bm = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle rec = new Rectangle(0, 0, w, h);
                    BitmapData bd = bm.LockBits(rec, ImageLockMode.ReadWrite, bm.PixelFormat);
                    for (int y = 0; y < h; ++y)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)bd.Scan0 + y * bd.Stride);
                        int num3 = y * w * 2;
                        for (int x = 0; x < w; ++x)
                        {
                            int num4 = x * 2;
                            int index = x * 4;
                            byte[] bt = new byte[2];
                            bt[0] = bts[num3 + num4 + 1];
                            bt[1] = bts[num3 + num4];
                            float num5 = ((float)BitConverter.ToInt16(bt, 0) / (float)short.MaxValue) * byte.MaxValue;
                            numPtr[index] = (byte)num5;
                            numPtr[index + 1] = (byte)num5;
                            numPtr[index + 2] = (byte)num5;
                            numPtr[index + 3] = byte.MaxValue;
                        }
                    }
                    bm.UnlockBits(bd);
                    return bm;
                default:
                    throw new NotSupportedException("Pixelformat " + px.ToString() + " is not supported.");
            }
        }

        public static unsafe Bitmap GetBitmapRGB(int w, int h, PixelFormat px, byte[] bts, bool normalized = false)
        {
            switch (px)
            {
                case PixelFormat.Format8bppIndexed:
                    Bitmap bitmap1 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r1 = new Rectangle(0, 0, w, h);
                    BitmapData d1 = bitmap1.LockBits(r1, ImageLockMode.ReadWrite, bitmap1.PixelFormat);
                    for (int index1 = 0; index1 < h; ++index1)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d1.Scan0 + index1 * d1.Stride);
                        int num1 = index1 * w;
                        for (int index2 = 0; index2 < w; ++index2)
                        {
                            int num2 = index2;
                            int index3 = index2 * 3;
                            byte bt = bts[num1 + num2];
                            numPtr[index3 + 2] = bt;
                            numPtr[index3 + 1] = bt;
                            numPtr[index3] = bt;
                        }
                    }
                    bitmap1.UnlockBits(d1);
                    return bitmap1;
                case PixelFormat.Format16bppGrayScale:
                    Bitmap bitmap2 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r2 = new Rectangle(0, 0, w, h);
                    BitmapData d2 = bitmap2.LockBits(r2, ImageLockMode.ReadWrite, bitmap2.PixelFormat);
                    for (int index4 = 0; index4 < h; ++index4)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d2.Scan0 + index4 * d2.Stride);
                        int num3 = index4 * w * 2;
                        for (int index5 = 0; index5 < w; ++index5)
                        {
                            int num4 = index5 * 2;
                            int index6 = index5 * 3;
                            int num5 = (int)((double)BitConverter.ToUInt16(bts, num3 + num4) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            numPtr[index6 + 2] = (byte)num5;
                            numPtr[index6 + 1] = (byte)num5;
                            numPtr[index6] = (byte)num5;
                        }
                    }
                    bitmap2.UnlockBits(d2);
                    return bitmap2;
                case PixelFormat.Format24bppRgb:
                    Bitmap bitmap3 = new Bitmap(w, h, PixelFormat.Format24bppRgb, bts, new ZCT(), "");
                    return bitmap3;
                case PixelFormat.Format32bppArgb:
                    Bitmap bitmap4 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r4 = new Rectangle(0, 0, w, h);
                    BitmapData d4 = bitmap4.LockBits(r4, ImageLockMode.ReadWrite, bitmap4.PixelFormat);
                    for (int index10 = 0; index10 < h; ++index10)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d4.Scan0 + index10 * d4.Stride);
                        int num8 = index10 * w * 4;
                        for (int index11 = 0; index11 < w; ++index11)
                        {
                            int num9 = index11 * 4;
                            int index12 = index11 * 3;
                            numPtr[index12 + 2] = bts[num8 + num9 + 2];
                            numPtr[index12 + 1] = bts[num8 + num9 + 1];
                            numPtr[index12] = bts[num8 + num9];
                        }
                    }
                    bitmap4.UnlockBits(d4);
                    return bitmap4;
                case PixelFormat.Format48bppRgb:
                    Bitmap bitmap5 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r5 = new Rectangle(0, 0, w, h);
                    BitmapData d5 = bitmap5.LockBits(r5, ImageLockMode.ReadWrite, bitmap5.PixelFormat);
                    for (int index13 = 0; index13 < h; ++index13)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d5.Scan0 + index13 * d5.Stride);
                        int num10 = index13 * w * 6;
                        for (int index14 = 0; index14 < w; ++index14)
                        {
                            int num11 = index14 * 6;
                            int index15 = index14 * 3;
                            int num12 = (int)((double)BitConverter.ToUInt16(bts, num10 + num11) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            int num13 = (int)((double)BitConverter.ToUInt16(bts, num10 + num11 + 2) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            int num14 = (int)((double)BitConverter.ToUInt16(bts, num10 + num11 + 4) / (double)ushort.MaxValue * (double)byte.MaxValue);
                            numPtr[index15 + 2] = (byte)num12;
                            numPtr[index15 + 1] = (byte)num13;
                            numPtr[index15] = (byte)num14;
                        }
                    }
                    bitmap5.UnlockBits(d5);
                    return bitmap5;
                case PixelFormat.Float:
                    Bitmap bf = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle rf = new Rectangle(0, 0, w, h);
                    BitmapData df = bf.LockBits(rf, ImageLockMode.ReadWrite, bf.PixelFormat);
                    for (int y = 0; y < h; ++y)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)df.Scan0 + y * df.Stride);
                        int num = y * w * 4;
                        for (int x = 0; x < w; ++x)
                        {
                            int index = x * 4;
                            if (normalized)
                            {
                                float f = BitConverter.ToSingle(bts, index + num) * 255;
                                numPtr[index + 0] = (byte)(f);
                                numPtr[index + 1] = (byte)(f);
                                numPtr[index + 2] = (byte)(f);
                            }
                            else
                            {
                                float f = BitConverter.ToSingle(bts, index + num);
                                numPtr[index + 0] = (byte)(f);
                                numPtr[index + 1] = (byte)(f);
                                numPtr[index + 2] = (byte)(f);
                            }
                        }
                    }
                    bf.UnlockBits(df);
                    return bf;
                case PixelFormat.Short:
                    Bitmap bm = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle rec = new Rectangle(0, 0, w, h);
                    BitmapData bd = bm.LockBits(rec, ImageLockMode.ReadWrite, bm.PixelFormat);
                    for (int y = 0; y < h; ++y)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)bd.Scan0 + y * bd.Stride);
                        int num3 = y * w * 2;
                        for (int x = 0; x < w; ++x)
                        {
                            int num4 = x * 2;
                            int index = x * 4;
                            byte[] bt = new byte[2];
                            bt[0] = bts[num3 + num4 + 1];
                            bt[1] = bts[num3 + num4];
                            float num5 = ((float)BitConverter.ToInt16(bt, 0) / (float)short.MaxValue) * byte.MaxValue;
                            numPtr[index] = (byte)num5;
                            numPtr[index + 1] = (byte)num5;
                            numPtr[index + 2] = (byte)num5;
                        }
                    }
                    bm.UnlockBits(bd);
                    return bm;
                default:
                    throw new NotSupportedException("Pixelformat " + px.ToString() + " is not supported.");
            }
        }

        public static unsafe IntPtr GetRGB32Data(int w, int h, PixelFormat px, byte[] bts)
        {
            switch (px)
            {
                case PixelFormat.Format8bppIndexed:
                    Bitmap bitmap1 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r1 = new Rectangle(0, 0, w, h);
                    BitmapData d1 = bitmap1.LockBits(r1, ImageLockMode.ReadWrite, bitmap1.PixelFormat);
                    for (int index1 = 0; index1 < h; ++index1)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d1.Scan0 + index1 * d1.Stride);
                        int num1 = index1 * w;
                        for (int index2 = 0; index2 < w; ++index2)
                        {
                            int num2 = index2;
                            int index3 = index2 * 4;
                            byte bt = bts[num1 + num2];
                            numPtr[index3 + 3] = byte.MaxValue;
                            numPtr[index3 + 2] = bt;
                            numPtr[index3 + 1] = bt;
                            numPtr[index3] = bt;
                        }
                    }
                    bitmap1.UnlockBits(d1);
                    return d1.Scan0;
                case PixelFormat.Format16bppGrayScale:
                    Bitmap bitmap2 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r2 = new Rectangle(0, 0, w, h);
                    BitmapData d2 = bitmap2.LockBits(r2, ImageLockMode.ReadWrite, bitmap2.PixelFormat);
                    for (int index4 = 0; index4 < h; ++index4)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d2.Scan0 + index4 * d2.Stride);
                        int num3 = index4 * w * 2;
                        for (int index5 = 0; index5 < w; ++index5)
                        {
                            int num4 = index5 * 2;
                            int index6 = index5 * 4;
                            ushort num5 = (ushort)((double)BitConverter.ToUInt16(bts, num3 + num4) / (double)byte.MaxValue);
                            numPtr[index6 + 3] = byte.MaxValue;
                            numPtr[index6 + 2] = (byte)num5;
                            numPtr[index6 + 1] = (byte)num5;
                            numPtr[index6] = (byte)num5;
                        }
                    }
                    bitmap2.UnlockBits(d2);
                    return d2.Scan0;
                case PixelFormat.Format24bppRgb:
                    Bitmap bitmap3 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r3 = new Rectangle(0, 0, w, h);
                    BitmapData d3 = bitmap3.LockBits(r3, ImageLockMode.ReadWrite, bitmap3.PixelFormat);
                    for (int index7 = 0; index7 < h; ++index7)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d3.Scan0 + index7 * d3.Stride);
                        int num6 = index7 * w * 3;
                        for (int index8 = 0; index8 < w; ++index8)
                        {
                            int num7 = index8 * 3;
                            int index9 = index8 * 4;
                            numPtr[index9 + 3] = byte.MaxValue;
                            numPtr[index9 + 2] = bts[num6 + num7 + 2];
                            numPtr[index9 + 1] = bts[num6 + num7 + 1];
                            numPtr[index9] = bts[num6 + num7];
                        }
                    }
                    bitmap3.UnlockBits(d3);
                    return d3.Scan0;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                    fixed (byte* numPtr = bts)
                        return (IntPtr)(void*)numPtr;
                case PixelFormat.Format48bppRgb:
                    Bitmap bitmap4 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r4 = new Rectangle(0, 0, w, h);
                    BitmapData d4 = bitmap4.LockBits(r4, ImageLockMode.ReadWrite, bitmap4.PixelFormat);
                    for (int index10 = 0; index10 < h; ++index10)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d4.Scan0 + index10 * d4.Stride);
                        int num8 = index10 * w * 6;
                        for (int index11 = 0; index11 < w; ++index11)
                        {
                            int num9 = index11 * 6;
                            int index12 = index11 * 4;
                            int num10 = (int)((double)BitConverter.ToUInt16(bts, num8 + num9) / (double)byte.MaxValue);
                            int num11 = (int)((double)BitConverter.ToUInt16(bts, num8 + num9 + 2) / (double)byte.MaxValue);
                            int num12 = (int)((double)BitConverter.ToUInt16(bts, num8 + num9 + 4) / (double)byte.MaxValue);
                            numPtr[index12 + 3] = byte.MaxValue;
                            numPtr[index12 + 2] = (byte)num10;
                            numPtr[index12 + 1] = (byte)num11;
                            numPtr[index12] = (byte)num12;
                        }
                    }
                    bitmap4.UnlockBits(d4);
                    return d4.Scan0;
                case PixelFormat.Float:
                    Bitmap bitmap6 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r6 = new Rectangle(0, 0, w, h);
                    BitmapData d = bitmap6.LockBits(r6, ImageLockMode.ReadWrite, bitmap6.PixelFormat);
                    Statistics[] sts = Statistics.FromBytes(bitmap6);
                    for (int y = 0; y < h; y++)
                    {
                        int num1 = y * d.Stride;
                        for (int x = 0; x < w; x++)
                        {
                            float f = BitConverter.ToSingle(bts, num1 + x);
                            if ((double)f < sts[0].Min)
                                f = 0.0f;
                            int num4 = (int)((double)(f / sts[0].Max) * (double)byte.MaxValue);
                            bitmap6.SetPixel(x, y, num4);
                        }
                    }
                    return bitmap6.Data;
                default:
                    throw new NotSupportedException("Pixelformat " + px.ToString() + " is not supported.");
            }
        }

        public static unsafe Bitmap GetRGB24Data(int w, int h, PixelFormat px, byte[] bts)
        {
            switch (px)
            {
                case PixelFormat.Format8bppIndexed:
                    Bitmap bitmap1 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r1 = new Rectangle(0, 0, w, h);
                    BitmapData d1 = bitmap1.LockBits(r1, ImageLockMode.ReadWrite, bitmap1.PixelFormat);
                    for (int index1 = 0; index1 < h; ++index1)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d1.Scan0 + index1 * d1.Stride);
                        int num1 = index1 * w;
                        for (int index2 = 0; index2 < w; ++index2)
                        {
                            int num2 = index2;
                            int index3 = index2 * 3;
                            byte bt = bts[num1 + num2];
                            numPtr[index3] = bt;
                            numPtr[index3 + 1] = bt;
                            numPtr[index3 + 2] = bt;
                        }
                    }
                    bitmap1.UnlockBits(d1);
                    return bitmap1;
                case PixelFormat.Format16bppGrayScale:
                    Bitmap bitmap2 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r2 = new Rectangle(0, 0, w, h);
                    BitmapData d2 = bitmap2.LockBits(r2, ImageLockMode.ReadWrite, bitmap2.PixelFormat);
                    for (int index4 = 0; index4 < h; ++index4)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d2.Scan0 + index4 * d2.Stride);
                        int num3 = index4 * w * 2;
                        for (int index5 = 0; index5 < w; ++index5)
                        {
                            int num4 = index5 * 2;
                            int index6 = index5 * 3;
                            ushort num5 = (ushort)((double)BitConverter.ToUInt16(bts, num3 + num4) / (double)byte.MaxValue);
                            numPtr[index6 + 2] = (byte)num5;
                            numPtr[index6 + 1] = (byte)num5;
                            numPtr[index6] = (byte)num5;
                        }
                    }
                    bitmap2.UnlockBits(d2);
                    return bitmap2;
                case PixelFormat.Format24bppRgb:
                    return new Bitmap(w, h, px, bts, new ZCT(), "");
                case PixelFormat.Format32bppArgb:
                    Bitmap bitmap3 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r3 = new Rectangle(0, 0, w, h);
                    BitmapData d3 = bitmap3.LockBits(r3, ImageLockMode.ReadWrite, bitmap3.PixelFormat);
                    for (int index7 = 0; index7 < h; ++index7)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d3.Scan0 + index7 * d3.Stride);
                        int num6 = index7 * w * 4;
                        for (int index8 = 0; index8 < w; ++index8)
                        {
                            int num7 = index8 * 4;
                            int index9 = index8 * 3;
                            numPtr[index9 + 2] = bts[num6 + num7];
                            numPtr[index9 + 1] = bts[num6 + num7 + 1];
                            numPtr[index9] = bts[num6 + num7 + 2];
                        }
                    }
                    bitmap3.UnlockBits(d3);
                    return bitmap3;
                case PixelFormat.Format48bppRgb:
                    Bitmap bitmap4 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r4 = new Rectangle(0, 0, w, h);
                    BitmapData d4 = bitmap4.LockBits(r4, ImageLockMode.ReadWrite, bitmap4.PixelFormat);
                    for (int index10 = 0; index10 < h; ++index10)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d4.Scan0 + index10 * d4.Stride);
                        int num8 = index10 * w * 6;
                        for (int index11 = 0; index11 < w; ++index11)
                        {
                            int num9 = index11 * 6;
                            int index12 = index11 * 3;
                            int num10 = (int)((double)BitConverter.ToUInt16(bts, num8 + num9) / (double)byte.MaxValue);
                            int num11 = (int)((double)BitConverter.ToUInt16(bts, num8 + num9 + 2) / (double)byte.MaxValue);
                            int num12 = (int)((double)BitConverter.ToUInt16(bts, num8 + num9 + 4) / (double)byte.MaxValue);
                            numPtr[index12] = (byte)num10;
                            numPtr[index12 + 1] = (byte)num11;
                            numPtr[index12 + 2] = (byte)num12;
                        }
                    }
                    bitmap4.UnlockBits(d4);
                    return bitmap4;
                default:
                    throw new NotSupportedException("Pixelformat " + px.ToString() + " is not supported.");
            }
        }

        public static byte[] Convert16BitGrayscaleTo24BitRGB(byte[] input16BitGrayscale)
        {
            // Each 16-bit grayscale pixel takes 2 bytes, and each 8-bit RGB pixel will take 3 bytes
            int numPixels = input16BitGrayscale.Length / 2;
            byte[] output8BitRGB = new byte[numPixels * 3];

            for (int i = 0, j = 0; i < input16BitGrayscale.Length; i += 2, j += 3)
            {
                // Extract the 16-bit grayscale value from the input array
                ushort gray16 = (ushort)(input16BitGrayscale[i] | (input16BitGrayscale[i + 1] << 8));

                // Downsample to 8-bit by dividing by 256
                byte gray8 = (byte)(gray16 >> 8);

                // Set the RGB values to the downsampled grayscale value
                output8BitRGB[j] = gray8;       // Red channel
                output8BitRGB[j + 1] = gray8;   // Green channel
                output8BitRGB[j + 2] = gray8;   // Blue channel
            }

            return output8BitRGB;
        }
        public static byte[] Convert32BitARGBTo24BitRGB(byte[] input32BitARGB)
        {
            // Each 32-bit ARGB pixel takes 4 bytes, and each 24-bit RGB pixel will take 3 bytes
            int numPixels = input32BitARGB.Length / 4;
            byte[] output24BitRGB = new byte[numPixels * 3];

            for (int i = 0, j = 0; i < input32BitARGB.Length; i += 4, j += 3)
            {
                // Skip the alpha channel and copy RGB values
                output24BitRGB[j] = input32BitARGB[i + 1];     // Red channel
                output24BitRGB[j + 1] = input32BitARGB[i + 2]; // Green channel
                output24BitRGB[j + 2] = input32BitARGB[i + 3]; // Blue channel
            }

            return output24BitRGB;
        }
        public static byte[] Convert48BitTo24BitRGB(byte[] input48BitRGB)
        {
            // Each 48-bit RGB pixel takes 6 bytes (2 bytes per channel), and each 24-bit RGB pixel will take 3 bytes
            int numPixels = input48BitRGB.Length / 6;
            byte[] output24BitRGB = new byte[numPixels * 3];

            for (int i = 0, j = 0; i < input48BitRGB.Length; i += 6, j += 3)
            {
                // Extract each 16-bit channel value from the input array
                ushort r16 = (ushort)(input48BitRGB[i] | (input48BitRGB[i + 1] << 8));
                ushort g16 = (ushort)(input48BitRGB[i + 2] | (input48BitRGB[i + 3] << 8));
                ushort b16 = (ushort)(input48BitRGB[i + 4] | (input48BitRGB[i + 5] << 8));

                // Downsample each channel to 8-bit by dividing by 256 (right-shifting by 8 bits)
                byte r8 = (byte)(r16 >> 8);
                byte g8 = (byte)(g16 >> 8);
                byte b8 = (byte)(b16 >> 8);

                // Store the 8-bit values in the output array
                output24BitRGB[j] = r8;
                output24BitRGB[j + 1] = g8;
                output24BitRGB[j + 2] = b8;
            }

            return output24BitRGB;
        }

        public static unsafe Bitmap GetFiltered(
            int w,
            int h,
            int stride,
            PixelFormat px,
            byte[] bts,
            IntRange rr,
            IntRange rg,
            IntRange rb)
        {
            switch (px)
            {
                case PixelFormat.Format8bppIndexed:
                    Bitmap bitmap1 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r1 = new Rectangle(0, 0, w, h);
                    BitmapData d1 = bitmap1.LockBits(r1, ImageLockMode.ReadWrite, bitmap1.PixelFormat);
                    for (int index1 = 0; index1 < h; ++index1)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d1.Scan0 + index1 * d1.Stride);
                        int num1 = index1 * stride;
                        for (int index2 = 0; index2 < w; ++index2)
                        {
                            int num2 = index2;
                            int index3 = index2 * 4;
                            float num3 = (float)bts[num1 + num2] - (float)rr.Min;
                            if ((double)num3 < 0.0)
                                num3 = 0.0f;
                            int num4 = (int)((double)(num3 / (float)rr.Max) * (double)byte.MaxValue);
                            numPtr[index3 + 3] = byte.MaxValue;
                            numPtr[index3 + 2] = (byte)num4;
                            numPtr[index3 + 1] = (byte)num4;
                            numPtr[index3] = (byte)num4;
                        }
                    }
                    bitmap1.UnlockBits(d1);
                    return bitmap1;
                case PixelFormat.Format16bppGrayScale:
                    Bitmap bitmap2 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r2 = new Rectangle(0, 0, w, h);
                    BitmapData d2 = bitmap2.LockBits(r2, ImageLockMode.ReadWrite, bitmap2.PixelFormat);
                    for (int index4 = 0; index4 < h; ++index4)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d2.Scan0 + index4 * d2.Stride);
                        int num5 = index4 * stride;
                        for (int index5 = 0; index5 < w; ++index5)
                        {
                            int num6 = index5 * 2;
                            int index6 = index5 * 4;
                            float num7 = (float)BitConverter.ToUInt16(bts, num5 + num6) - (float)rr.Min;
                            if ((double)num7 < 0.0)
                                num7 = 0.0f;
                            int num8 = (int)((double)(num7 / (float)rr.Max) * (double)byte.MaxValue);
                            numPtr[index6 + 3] = byte.MaxValue;
                            numPtr[index6 + 2] = (byte)num8;
                            numPtr[index6 + 1] = (byte)num8;
                            numPtr[index6] = (byte)num8;
                        }
                    }
                    bitmap2.UnlockBits(d2);
                    return bitmap2;
                case PixelFormat.Format24bppRgb:
                    Bitmap bitmap3 = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    Rectangle r3 = new Rectangle(0, 0, w, h);
                    BitmapData d3 = bitmap3.LockBits(r3, ImageLockMode.ReadWrite, bitmap3.PixelFormat);
                    for (int index7 = 0; index7 < h; ++index7)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d3.Scan0 + index7 * d3.Stride);
                        int num9 = index7 * stride;
                        for (int index8 = 0; index8 < w; ++index8)
                        {
                            int num10 = index8 * 3;
                            int index9 = index8 * 3;
                            numPtr[index9 + 2] = bts[num9 + num10 + 2];
                            numPtr[index9 + 1] = bts[num9 + num10 + 1];
                            numPtr[index9] = bts[num9 + num10];
                            float num11 = (float)bts[num9 + num10] - (float)rr.Min;
                            if ((double)num11 < 0.0)
                                num11 = 0.0f;
                            float num12 = num11 / (float)rr.Max;
                            float num13 = (float)bts[num9 + num10 + 1] - (float)rg.Min;
                            if ((double)num13 < 0.0)
                                num13 = 0.0f;
                            float num14 = num13 / (float)rg.Max;
                            float num15 = (float)bts[num9 + num10 + 2] - (float)rb.Min;
                            if ((double)num15 < 0.0)
                                num15 = 0.0f;
                            float num16 = num15 / (float)rb.Max;
                            int num17 = (int)((double)num12 * (double)byte.MaxValue);
                            int num18 = (int)((double)num14 * (double)byte.MaxValue);
                            int num19 = (int)((double)num16 * (double)byte.MaxValue);
                            numPtr[index9 + 2] = (byte)num17;
                            numPtr[index9 + 1] = (byte)num18;
                            numPtr[index9] = (byte)num19;
                        }
                    }
                    bitmap3.UnlockBits(d3);
                    return bitmap3;
                case PixelFormat.Format32bppArgb:
                    Bitmap bitmap4 = new Bitmap(w, h, PixelFormat.Format32bppRgb);
                    Rectangle r4 = new Rectangle(0, 0, w, h);
                    BitmapData d4 = bitmap4.LockBits(r4, ImageLockMode.ReadWrite, bitmap4.PixelFormat);
                    for (int index10 = 0; index10 < h; ++index10)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d4.Scan0 + index10 * d4.Stride);
                        int num20 = index10 * stride;
                        for (int index11 = 0; index11 < w; ++index11)
                        {
                            int num21 = index11 * 4;
                            int index12 = index11 * 4;
                            numPtr[index12 + 2] = bts[num20 + num21 + 2];
                            numPtr[index12 + 1] = bts[num20 + num21 + 1];
                            numPtr[index12] = bts[num20 + num21];
                            float num22 = (float)bts[num20 + num21] - (float)rr.Min;
                            if ((double)num22 < 0.0)
                                num22 = 0.0f;
                            float num23 = num22 / (float)rr.Max;
                            float num24 = (float)bts[num20 + num21 + 1] - (float)rg.Min;
                            if ((double)num24 < 0.0)
                                num24 = 0.0f;
                            float num25 = num24 / (float)rg.Max;
                            float num26 = (float)bts[num20 + num21 + 2] - (float)rb.Min;
                            if ((double)num26 < 0.0)
                                num26 = 0.0f;
                            float num27 = num26 / (float)rb.Max;
                            int num28 = (int)((double)num23 * (double)byte.MaxValue);
                            int num29 = (int)((double)num25 * (double)byte.MaxValue);
                            int num30 = (int)((double)num27 * (double)byte.MaxValue);
                            numPtr[index12 + 2] = (byte)num28;
                            numPtr[index12 + 1] = (byte)num29;
                            numPtr[index12] = (byte)num30;
                        }
                    }
                    bitmap4.UnlockBits(d4);
                    return bitmap4;
                case PixelFormat.Format48bppRgb:
                    Bitmap bitmap5 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r5 = new Rectangle(0, 0, w, h);
                    BitmapData d5 = bitmap5.LockBits(r5, ImageLockMode.ReadWrite, bitmap5.PixelFormat);
                    for (int index13 = 0; index13 < h; ++index13)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)d5.Scan0 + index13 * d5.Stride);
                        int num31 = index13 * stride;
                        for (int index14 = 0; index14 < w; ++index14)
                        {
                            int num32 = index14 * 6;
                            int index15 = index14 * 4;
                            float num33 = (float)BitConverter.ToUInt16(bts, num31 + num32) - (float)rr.Min;
                            if ((double)num33 < 0.0)
                                num33 = 0.0f;
                            float num34 = num33 / (float)rr.Max;
                            float num35 = (float)BitConverter.ToUInt16(bts, num31 + num32 + 2) - (float)rg.Min;
                            if ((double)num35 < 0.0)
                                num35 = 0.0f;
                            float num36 = num35 / (float)rg.Max;
                            float num37 = (float)BitConverter.ToUInt16(bts, num31 + num32 + 4) - (float)rb.Min;
                            if ((double)num37 < 0.0)
                                num37 = 0.0f;
                            float num38 = num37 / (float)rb.Max;
                            int num39 = (int)((double)num34 * (double)byte.MaxValue);
                            int num40 = (int)((double)num36 * (double)byte.MaxValue);
                            int num41 = (int)((double)num38 * (double)byte.MaxValue);
                            numPtr[index15 + 3] = byte.MaxValue;
                            numPtr[index15 + 2] = (byte)num39;
                            numPtr[index15 + 1] = (byte)num40;
                            numPtr[index15] = (byte)num41;
                        }
                    }
                    bitmap5.UnlockBits(d5);
                    return bitmap5;
                case PixelFormat.Float:
                    Bitmap bitmap6 = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle r6 = new Rectangle(0, 0, w, h);
                    for (int y = 0; y < h; y++)
                    {
                        int num = y * stride;
                        for (int x = 0; x < w; x++)
                        {
                            float f = BitConverter.ToSingle(bts, (num) + (x*4));
                            bitmap6.SetValue(x, y, 3, byte.MaxValue);
                            bitmap6.SetValue(x, y, 2, (byte)f);
                            bitmap6.SetValue(x, y, 1, (byte)f);
                            bitmap6.SetValue(x, y, 0, (byte)f);
                         }
                    }
                    return bitmap6;
                case PixelFormat.Short:
                    Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    Rectangle re = new Rectangle(0, 0, w, h);
                    BitmapData da = bmp.LockBits(re, ImageLockMode.ReadWrite, bmp.PixelFormat);
                    for (int index4 = 0; index4 < h; ++index4)
                    {
                        byte* numPtr = (byte*)((IntPtr)(void*)da.Scan0 + index4 * da.Stride);
                        int num5 = index4 * stride;
                        for (int index5 = 0; index5 < w; ++index5)
                        {
                            int num6 = index5 * 2;
                            int index6 = index5 * 4;
                            float num7 = (float)BitConverter.ToInt16(bts, num5 + num6) - (float)rr.Min;
                            if ((double)num7 < 0.0)
                                num7 = 0.0f;
                            int num8 = (int)((double)(num7 / (float)rr.Max) * (double)byte.MaxValue);
                            numPtr[index6 + 3] = byte.MaxValue;
                            numPtr[index6 + 2] = (byte)num8;
                            numPtr[index6 + 1] = (byte)num8;
                            numPtr[index6] = (byte)num8;
                        }
                    }
                    bmp.UnlockBits(da);
                    return bmp;
                default:
                    throw new InvalidDataException("Bio supports only 8, 16 24, 32, 48 bit images.");
            }
        }

        public Bitmap GetFiltered(IntRange rr, IntRange rg, IntRange rb) => Bitmap.GetFiltered(this.SizeX, this.SizeY, this.Stride, this.PixelFormat, this.Bytes, rr, rg, rb);

        public void Crop(Rectangle r)
        {
            int bytesPerChannel = this.BitsPerPixel / 8;
            int channels = this.RGBChannelsCount;

            // Determine the number of bytes per pixel based on bits per pixel and channel count
            int bytesPerPixel = bytesPerChannel * channels;
            int croppedStride = r.Width * bytesPerPixel;
            int stride = this.Stride;
            byte[] croppedBytes = new byte[croppedStride * r.Height];

            for (int y = 0; y < r.Height; y++)
            {
                for (int x = 0; x < r.Width; x++)
                {
                    int destIndex = y * croppedStride + x * bytesPerPixel;
                    int srcIndex = (y + r.Y) * stride + (x + r.X) * bytesPerPixel;

                    // Copy the correct number of bytes per channel
                    Array.Copy(this.bytes, srcIndex, croppedBytes, destIndex, bytesPerPixel);
                }
            }

            // Update the image bytes and width/height based on the crop region
            this.bytes = croppedBytes;
            this.SizeX = r.Width;
            this.SizeY = r.Height;
        }
        public static byte[] ConvertToInterleaved(byte[] nonInterleaved, PixelFormat px)
        {
            int numChannels;
            int bytesPerChannel;

            switch (px)
            {
                case PixelFormat.Format24bppRgb:
                    numChannels = 3;      // R, G, B
                    bytesPerChannel = 1;
                    break;
                case PixelFormat.Format32bppArgb:
                    numChannels = 4;      // A, R, G, B
                    bytesPerChannel = 1;
                    break;
                case PixelFormat.Format48bppRgb:
                    numChannels = 3;      // R, G, B, each 16-bit
                    bytesPerChannel = 2;
                    break;
                case PixelFormat.Format16bppGrayScale:
                    numChannels = 1;      // Grayscale, 16-bit
                    bytesPerChannel = 2;
                    break;
                case PixelFormat.Format8bppIndexed:
                    numChannels = 1;      // Grayscale, 8-bit
                    bytesPerChannel = 1;
                    break;
                default:
                    throw new NotSupportedException("PixelFormat " + px + " is not supported.");
            }

            int numPixels = nonInterleaved.Length / (numChannels * bytesPerChannel);
            byte[] interleaved = new byte[nonInterleaved.Length];

            // Interleaving logic
            for (int i = 0; i < numPixels; i++)
            {
                for (int c = 0; c < numChannels; c++)
                {
                    int sourceIndex = i * bytesPerChannel + c * numPixels * bytesPerChannel;
                    int targetIndex = (i * numChannels + c) * bytesPerChannel;

                    // Copy each channel's bytes (1 byte for 8-bit channels, 2 bytes for 16-bit channels)
                    for (int b = 0; b < bytesPerChannel; b++)
                    {
                        interleaved[targetIndex + b] = nonInterleaved[sourceIndex + b];
                    }
                }
            }

            return interleaved;
        }

        private void Initialize(
        string file,
        int w,
        int h,
        PixelFormat px,
        byte[] bytes,
        ZCT coord,
        int index,
        Plane plane,
        bool littleEndian = true,
        bool interleaved = true)
        {
            // Generate a unique ID for this bitmap based on file name and index
            this.ID = Bitmap.CreateID(file, index);

            // Set size and pixel format properties
            this.SizeX = w;
            this.SizeY = h;
            this.pixelFormat = px;
            this.Coordinate = coord;
            this.Plane = plane;

            // Validate byte array input
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentException("Byte array cannot be null or empty.", nameof(bytes));
            }
            
            // Handle endianness and interleaving
            if (interleaved && littleEndian)
            {
                this.Bytes = bytes;
                SwitchRedBlue();
            }
            else if (!interleaved && littleEndian)
            {
                //DM-004
                this.Bytes = ConvertToInterleaved(bytes, pixelFormat);
                SwitchRedBlue();
            }
            else if (interleaved && !littleEndian)
            {
                this.Bytes = bytes;
                ReverseByteOrderByPixelFormat();
                SwitchRedBlue();
            }
            else // !interleaved && !littleEndian
            {
                //OK CMU-2.svs
                this.Bytes = ConvertToInterleaved(bytes, pixelFormat);
                ReverseByteOrderByPixelFormat();
                SwitchRedBlue();
            }
            // Special case for 32-bit ARGB format: Check and correct transparency
            if (px == PixelFormat.Format32bppArgb)
            {
                CorrectTransparency();
            }
        }

        private void CorrectTransparency()
        {
            int stride = this.SizeX * 4; // 4 bytes per pixel for 32bppArgb
            for (int y = 0; y < this.SizeY; y++)
            {
                for (int x = 0; x < this.SizeX; x++)
                {
                    int index = y * stride + x * 4 + 3; // Alpha channel is at index 3
                    if (this.Bytes[index] == 0)
                    {
                        this.Bytes[index] = 255; // Set fully opaque
                    }
                }
            }
        }

        // Helper method to reverse byte order based on pixel format
        private void ReverseByteOrderByPixelFormat()
        {
            int bytesPerPixel = GetPixelFormatSize(this.pixelFormat) / 8;

            // Reverse each pixel's byte order
            for (int i = 0; i < this.Bytes.Length; i += bytesPerPixel)
            {
                Array.Reverse(this.Bytes, i, bytesPerPixel);
            }
        }

        public Bitmap(string file, int w, int h, PixelFormat px, byte[] bts, ZCT coord, int index) => this.Initialize(file, w, h, px, bts, coord, index, (Plane)null);

        public Bitmap(int w, int h, PixelFormat px)
        {
            PixelFormat = px;
            SizeX = w;
            SizeY = h;
            Bytes = new byte[Stride * h];
            Initialize("", w, h, px, this.Bytes, new ZCT(), 0, null);
        }

        public Bitmap(
            string file,
            int w,
            int h,
            PixelFormat px,
            byte[] byts,
            ZCT coord,
            int index,
            Plane plane,
            bool littleEndian = true,
            bool interleaved = true)
        {
            this.Initialize(file, w, h, px, byts, coord, index, plane, littleEndian, interleaved);
        }

        public Bitmap(string file, UnmanagedImage im, ZCT coord, int index)
        {
            this.Image = im;
            Initialize("", im.Width, im.Height, im.PixelFormat, this.Bytes, new ZCT(), 0, null);
        }

        public Bitmap(string file, UnmanagedImage im, ZCT coord, int index, Plane pl)
        {
            this.Image = im;
            Initialize("", im.Width, im.Height, im.PixelFormat, this.Bytes, new ZCT(), 0, null);
        }

        public Bitmap(int width, int height, int stride, PixelFormat pixelFormat, IntPtr imageData)
        {
            this.SizeX = width;
            this.SizeY = height;
            this.pixelFormat = pixelFormat;
            this.bytes = new byte[stride * height];
            Marshal.Copy(imageData, this.bytes, 0, stride * height);
            Initialize("", width, height, pixelFormat, this.Bytes, new ZCT(), 0, null);
        }
        public Bitmap(UnmanagedImage im)
        {
            byte[] bts = new byte[im.Stride * im.Height];
            Marshal.Copy(im.ImageData, bts, 0, im.Stride * im.Height);
            Initialize("", im.Width, im.Height, im.PixelFormat, bts, new ZCT(), 0, null);
        }
        /// <summary>
        /// Saves a Bitmap to a file. Only supports 8 bit depth. Supported formats are PNG, XPM, JPEG, TIFF, PNM, RAS, BMP, and GIF.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="format"></param>
        public void Save(string file, string format)
        {
            GetPixbuf().Save(file, format);
        }

        public Bitmap(int w, int h, PixelFormat px, byte[] bts, ZCT coord, string id) => this.Initialize(id, w, h, px, bts, coord, 0, null);

        public static UnmanagedImage SwitchRedBlue(UnmanagedImage image) => Bitmap.SwitchRedBlue(new Bitmap(image)).Image;

        public static Bitmap SwitchRedBlue(Bitmap image)
        {
            ExtractChannel extractChannel1 = new ExtractChannel((short)2);
            ExtractChannel extractChannel2 = new ExtractChannel((short)0);
            UnmanagedImage image1 = image.Image;
            UnmanagedImage channelImage1 = extractChannel1.Apply(image1);
            UnmanagedImage channelImage2 = extractChannel2.Apply(image.Image);
            new ReplaceChannel((short)2, channelImage2).ApplyInPlace(image.Image);
            new ReplaceChannel((short)0, channelImage1).ApplyInPlace(image.Image);
            channelImage1.Dispose();
            channelImage2.Dispose();
            return image;
        }

        public void SwitchRedBlue()
        {
            if (RGBChannelsCount == 1)
                return;

            // Handle 24bpp RGB format (3 bytes per pixel: R, G, B)
            if (this.PixelFormat == PixelFormat.Format24bppRgb)
            {
                for (int index1 = 0; index1 < this.SizeY; ++index1)
                {
                    // Iterate through the row using Stride (not just SizeX)
                    for (int index2 = 0; index2 < this.Stride; index2 += 3)
                    {
                        // Swapping Red (index2) and Blue (index2 + 2)
                        int index3 = index1 * this.Stride + index2;
                        byte temp = this.bytes[index3 + 2];  // Blue
                        this.bytes[index3 + 2] = this.bytes[index3];  // Red
                        this.bytes[index3] = temp;  // Assign Blue to Red
                    }
                }
            }
            // Handle 32bpp ARGB/RGB format (4 bytes per pixel: A, R, G, B)
            else if (this.PixelFormat == PixelFormat.Format32bppArgb || this.PixelFormat == PixelFormat.Format32bppRgb)
            {
                for (int index4 = 0; index4 < this.SizeY; ++index4)
                {
                    // Iterate through the row using Stride (not just SizeX)
                    for (int index5 = 0; index5 < this.Stride; index5 += 4)
                    {
                        // Swapping Red (index5 + 1) and Blue (index5 + 3)
                        int index6 = index4 * this.Stride + index5;
                        byte temp = this.bytes[index6 + 2];  // Blue
                        this.bytes[index6 + 2] = this.bytes[index6];  // Red
                        this.bytes[index6] = temp;  // Assign Blue to Red
                    }
                }
            }
            // Handle 48bpp RGB format (6 bytes per pixel: R, G, B for each pixel, 2 bytes per channel)
            else if (this.PixelFormat == PixelFormat.Format48bppRgb)
            {
                for (int index7 = 0; index7 < this.SizeY; ++index7)
                {
                    int num1 = index7 * this.Stride;  // Row offset
                    for (int index8 = 0; index8 < this.Stride; index8 += 6)
                    {
                        // Swapping Red and Blue channels (each channel is 2 bytes)
                        byte temp1 = this.bytes[num1 + index8];  // Red (2 bytes)
                        byte temp2 = this.bytes[num1 + index8 + 1];  // Green (2 bytes)
                        this.bytes[num1 + index8] = this.bytes[num1 + index8 + 4];  // Blue (2 bytes)
                        this.bytes[num1 + index8 + 1] = this.bytes[num1 + index8 + 5];  // Blue (2 bytes)
                        this.bytes[num1 + index8 + 4] = temp1;  // Red (2 bytes)
                        this.bytes[num1 + index8 + 5] = temp2;  // Green (2 bytes)
                    }
                }
            }
        }

        public byte[] GetSaveBytes(bool littleEndian)
        {
            Bitmap bitmap = this.Copy();
            bitmap.SwitchRedBlue();
            if (!littleEndian)
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            if (!littleEndian)
                Array.Reverse<byte>(bitmap.Bytes);
            return bitmap.Bytes;
        }

        public static byte[] GetBuffer(Bitmap bmp, int stride)
        {
            BitmapData d = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            IntPtr scan0 = d.Scan0;
            int length1 = d.Stride * bmp.Height;
            byte[] array = new byte[length1];
            byte[] destination = array;
            int length2 = length1;
            Marshal.Copy(scan0, destination, 0, length2);
            Array.Reverse<byte>(array);
            bmp.UnlockBits(d);
            return array;
        }

        public static Bitmap To24Bit(Bitmap b) => Bitmap.GetRGB24Data(b.Width, b.Height, b.PixelFormat, b.Bytes);

        public static Bitmap To32Bit(Bitmap b) => b.GetImageRGBA();

        public static Bitmap SwitchChannels(Bitmap image, int c1, int c2)
        {
            ExtractChannel extractChannel1 = new ExtractChannel((short)c1);
            ExtractChannel extractChannel2 = new ExtractChannel((short)c2);
            UnmanagedImage image1 = image.Image;
            UnmanagedImage channelImage1 = extractChannel1.Apply(image1);
            UnmanagedImage channelImage2 = extractChannel2.Apply(image.Image);
            new ReplaceChannel((short)c1, channelImage2).ApplyInPlace(image.Image);
            new ReplaceChannel((short)c2, channelImage1).ApplyInPlace(image.Image);
            channelImage1.Dispose();
            channelImage2.Dispose();
            return image;
        }

        public Bitmap Copy()
        {
            byte[] bts = new byte[this.Bytes.Length];
            for (int index = 0; index < bts.Length; ++index)
                bts[index] = this.bytes[index];
            return new Bitmap(this.SizeX, this.SizeY, this.PixelFormat, bts, this.Coordinate, this.ID)
            {
                Plane = this.Plane
            };
        }

        public Bitmap CopyInfo()
        {
            Bitmap bitmap = new Bitmap(this.SizeX, this.SizeY, this.PixelFormat, new byte[this.Stride * this.SizeY], this.Coordinate, this.ID);
            bitmap.bytes = new byte[bitmap.Stride * bitmap.SizeY];
            bitmap.Plane = this.Plane;
            return bitmap;
        }
        /*
        public Bitmap Get16Depth()
        {
            Bitmap bm = new Bitmap(SizeX, SizeY, PixelFormat.Format48bppRgb);
            for (int y = 0; y < SizeY; y++)
            {
                for (int x = 0; x < SizeX; x++)
                {
                    for (int r = 0; r < RGBChannelsCount; r++)
                    {
                        float f = GetValueRGB(x, y, r);
                        bm.SetValue(x, y, f);
                    }
                }
            }
            return bm;
        }
        */
        public void To8Bit(bool normalized = false)
        {
            if (PixelFormat != PixelFormat.Float)
            {
                Bitmap bitmap = AForge.Imaging.Image.Convert16bppTo8bpp(this);
                this.Image = (UnmanagedImage)bitmap;
            }
            else
            {
                Bitmap bm = new Bitmap(SizeX, SizeY, PixelFormat.Format8bppIndexed);
                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < SizeX; x++)
                    {
                        if (!normalized)
                        {
                            float f = (float)GetValue(x, y);
                            bm.SetValue(x, y, (byte)f);
                        }
                        else
                        {
                            float f = (float)GetValue(x, y);
                            float s = (float)(f * byte.MaxValue);
                            bm.SetValue(x, y, (byte)s);
                        }
                    }
                }
                Bytes = bm.Bytes;
                PixelFormat = PixelFormat.Format8bppIndexed;
            }
        }

        public void To16Bit(bool normalized = false)
        {
            if(PixelFormat != PixelFormat.Float)
                this.Image = (UnmanagedImage)AForge.Imaging.Image.Convert8bppTo16bpp(this);
            else
            {
                Bitmap bm = new Bitmap(SizeX, SizeY, PixelFormat.Format16bppGrayScale);
                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < SizeX; x++)
                    {
                        if (!normalized)
                        {
                            ushort f = (ushort)GetValue(x, y);
                            bm.SetValue(x, y, f);
                        }
                        else
                        {
                            float f = (float)GetValue(x, y);
                            ushort s = (ushort)(f * ushort.MaxValue);
                            bm.SetValue(x, y, s);
                        }
                    }
                }
                Bytes = bm.Bytes;
                PixelFormat = PixelFormat.Format16bppGrayScale;
            }
        }
        public void ToFloat()
        {
            Bitmap bm = new Bitmap(SizeX, SizeY, PixelFormat.Float);
            bm.Stats = Statistics.FromBytes(this);
            for (int y = 0; y < SizeY; y++)
            {
                for (int x = 0; x < SizeX; x++)
                {
                    float f = GetValue(x, y) / bm.Stats[0].Max;
                    bm.SetValue(x, y, f);
                }
            }
            PixelFormat = PixelFormat.Float;
            Bytes = bm.Bytes;
        }

        public byte[] SwitchEndianness(byte[] bts)
        {
            int length = bts.Length;
            for (int i = 0; i < length / 2; i++)
            {
                byte temp = bts[i];
                bts[i] = bts[length - 1 - i];
                bts[length - 1 - i] = temp;
            }
            return bts;
        }

        public void SwitchEndianness()
        {
            int length = Bytes.Length;
            for (int i = 0; i < length / 2; i++)
            {
                byte temp = Bytes[i];
                Bytes[i] = Bytes[length - 1 - i];
                Bytes[length - 1 - i] = temp;
            }
        }

        public void ToShort()
        {
            Bitmap bm = new Bitmap(SizeX, SizeY, PixelFormat.Short);
            for (int y = 0; y < SizeY; y++)
            {
                for (int x = 0; x < SizeX; x++)
                {
                    short f = (short)GetValue(x, y);
                    bm.SetValue(x, y, f);
                }
            }
            Bytes = SwitchEndianness(bm.Bytes);
            PixelFormat = PixelFormat.Short;
            RotateFlip(RotateFlipType.Rotate180FlipNone);
        }

        public void ToRGB()
        {
            if (this.BitsPerPixel > 8)
            {
                int sizeX1 = this.SizeX;
            }
            else
            {
                int sizeX2 = this.SizeX;
            }
            int sizeX3 = this.SizeX;
            int sizeY = this.SizeY;
            if (this.PixelFormat == PixelFormat.Format48bppRgb)
                return;
            if (this.PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                byte[] numArray = new byte[sizeY * this.SizeX * 3 * 2];
                for (int index1 = 0; index1 < sizeY; ++index1)
                {
                    int num1 = index1 * (sizeX3 * 2 * 3);
                    int num2 = index1 * (sizeX3 * 2);
                    for (int index2 = 0; index2 < sizeX3; ++index2)
                    {
                        int num3 = index2 * 6;
                        int num4 = index2 * 2;
                        numArray[num1 + num3] = this.Bytes[num2 + num4];
                        numArray[num1 + num3 + 1] = this.Bytes[num2 + num4 + 1];
                        numArray[num1 + num3 + 2] = this.Bytes[num2 + num4];
                        numArray[num1 + num3 + 3] = this.Bytes[num2 + num4 + 1];
                        numArray[num1 + num3 + 4] = this.Bytes[num2 + num4];
                        numArray[num1 + num3 + 5] = this.Bytes[num2 + num4 + 1];
                    }
                }
                this.Bytes = numArray;
                this.PixelFormat = PixelFormat.Format48bppRgb;
            }
            else
            {
                if (this.PixelFormat == PixelFormat.Format24bppRgb || this.PixelFormat != PixelFormat.Format8bppIndexed)
                    return;
                byte[] numArray = new byte[sizeY * this.SizeX * 3];
                for (int index3 = 0; index3 < this.SizeY; ++index3)
                {
                    int num5 = index3 * (this.SizeX * 3);
                    int num6 = index3 * this.SizeX;
                    for (int index4 = 0; index4 < this.SizeX; ++index4)
                    {
                        int num7 = index4 * 3;
                        int num8 = index4;
                        numArray[num5 + num7] = this.Bytes[num6 + num8];
                        numArray[num5 + num7 + 1] = this.Bytes[num6 + num8];
                        numArray[num5 + num7 + 2] = this.Bytes[num6 + num8];
                    }
                }
                this.Bytes = numArray;
                this.PixelFormat = PixelFormat.Format24bppRgb;
            }
        }

        public bool isRGB
        {
            get
            {
                if (RGBChannelsCount > 1)
                    return true;
                else
                    return false;
            }
        }

        public override string ToString() => System.IO.Path.GetFileName(File) + ", " + Width + ", " + Height;

        public void Dispose()
        {
            this.bytes = (byte[])null;
            if (this.stats != null)
            {
                for (int index = 0; index < this.stats.Length; ++index)
                {
                    if (this.stats[index] != null)
                        this.stats[index].Dispose();
                }
            }
            this.ID = (string)null;
            this.file = (string)null;
        }

        public static Bitmap operator /(Bitmap a, Bitmap b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) / b.GetPixel(ix, iy));
            }
            return bitmap;
        }

        public static Bitmap operator *(Bitmap a, Bitmap b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) * b.GetPixel(ix, iy));
            }
            return bitmap;
        }

        public static Bitmap operator +(Bitmap a, Bitmap b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) + b.GetPixel(ix, iy));
            }
            return bitmap;
        }

        public static Bitmap operator -(Bitmap a, Bitmap b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) - b.GetPixel(ix, iy));
            }
            return bitmap;
        }

        public static Bitmap operator /(Bitmap a, float b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) / b);
            }
            return bitmap;
        }

        public static Bitmap operator *(Bitmap a, float b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) * b);
            }
            return bitmap;
        }

        public static Bitmap operator +(Bitmap a, float b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) + b);
            }
            return bitmap;
        }

        public static Bitmap operator -(Bitmap a, float b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) - b);
            }
            return bitmap;
        }

        public static Bitmap operator /(Bitmap a, ColorS b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) / b);
            }
            return bitmap;
        }

        public static Bitmap operator *(Bitmap a, ColorS b)
        {
            Bitmap bitmap = a.Copy();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) * b);
            }
            return bitmap;
        }

        public static Bitmap operator +(Bitmap a, ColorS b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) + b);
            }
            return bitmap;
        }

        public static Bitmap operator -(Bitmap a, ColorS b)
        {
            Bitmap bitmap = a.CopyInfo();
            for (int iy = 0; iy < a.SizeY; ++iy)
            {
                for (int ix = 0; ix < a.SizeX; ++ix)
                    bitmap.SetPixel(ix, iy, a.GetPixel(ix, iy) - b);
            }
            return bitmap;
        }

        public class ColorPalette
        {
            internal Color[] entries;

            public Color[] Entries
            {
                get => this.entries;
                set => this.entries = value;
            }

            public ColorPalette() => this.entries = new Color[256];
        }
    }
}
