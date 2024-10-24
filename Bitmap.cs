using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private float min = ushort.MaxValue;
        private float max = ushort.MinValue;
        private float stackMin = ushort.MaxValue;
        private float stackMax = ushort.MinValue;
        private float stackMean = 0;
        private float stackMedian = 0;
        private float mean = 0;
        private float median = 0;
        private float meansum = 0;
        private float[] stackValues = new float[ushort.MaxValue];
        private int count = 0;
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
        public static Statistics[] FromBytes(byte[] bts, int w, int h, int rGBChannels, int BitsPerPixel, int stride, PixelFormat px)
        {
            Statistics[] sts = new Statistics[rGBChannels];
            for (int i = 0; i < rGBChannels; i++)
            {
                sts[i] = new Statistics(BitsPerPixel);
                sts[i].max = ushort.MinValue;
                sts[i].min = ushort.MaxValue;
                sts[i].bitsPerPixel = BitsPerPixel;
            }
            
            float sumr = 0;
            float sumg = 0;
            float sumb = 0;
            float suma = 0;
            if (px == PixelFormat.Format16bppGrayScale || px == PixelFormat.Format48bppRgb)
            {
                for (int y = 0; y < h; y++)
                {
                    int BytesPerPixel = BitsPerPixel / 8;
                    for (int x = 0; x < stride; x += BytesPerPixel * rGBChannels)
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
            else if (px == PixelFormat.Format8bppIndexed || px == PixelFormat.Format24bppRgb || px == PixelFormat.Format32bppArgb)
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
            else if (px == PixelFormat.Short)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        short s = BitConverter.ToInt16(bts, y * stride + x);
                        if (sts[0].max < s)
                            sts[0].max = s;
                        if (sts[0].min > s)
                            sts[0].min = s;
                        sumr += s;
                    }
                }
            }
            else if (px == PixelFormat.Float)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float s = BitConverter.ToSingle(bts, y * stride + x);
                        if (sts[0].max < s)
                            sts[0].max = s;
                        if (sts[0].min > s)
                            sts[0].min = s;
                        sumr += s;
                    }
                }
            }
            else
                throw new NotSupportedException(px + " is not supported.");

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
            return FromBytes(bf.Bytes, bf.SizeX, bf.SizeY, bf.RGBChannelsCount, bf.BitsPerPixel, bf.Stride, bf.PixelFormat);
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
        private Plane plane;

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

        public void SetValue(int x, int y, ushort value)
        {
            SetValue(x, y, 0, value);
        }
        public void SetValue(int x, int y, float value)
        {
            SetValue(x, y, 0, value);
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
                // Ensure the channel is within the valid range (0 to PixelFormatSize - 1)
                if (channel < 0 || channel >= PixelFormatSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(channel), "Invalid channel index.");
                }

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

        public int HashID => this.ID.GetHashCode();

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

        public Gdk.Pixbuf Pixbuf 
        { 
            get { return new Gdk.Pixbuf(Bytes, RGBChannelsCount > 3, BitsPerPixel, SizeX, SizeY, Stride); }
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

        public byte[] PaddedBytes => Bitmap.GetPaddedBuffer(this.bytes, this.SizeX, this.SizeY, this.Stride, this.PixelFormat);

        public unsafe UnmanagedImage Image
        {
            get
            {
                fixed (byte* numPtr = this.PaddedBytes)
                    return new UnmanagedImage((IntPtr)(void*)numPtr, this.SizeX, this.SizeY, this.PaddedStride, this.PixelFormat);
            }
            set
            {
                this.bytes = new byte[value.Stride * value.Height];
                Marshal.Copy(value.ImageData, this.Bytes, 0, value.Stride * value.Height);
                this.PixelFormat = value.PixelFormat;
                this.SizeX = value.Width;
                this.SizeY = value.Height;
            }
        }

        public byte[] RGBBytes => Bitmap.GetBitmapRGB(this.SizeX, this.SizeY, this.PixelFormat, this.Bytes).Bytes;

        public Bitmap ImageRGB
        {
            get => Bitmap.GetBitmapRGB(this.SizeX, this.SizeY, this.PixelFormat, this.Bytes);
            set
            {
                Marshal.Copy(value.Bytes, 0, Marshal.UnsafeAddrOfPinnedArrayElement<byte>(this.Bytes, 0), value.Bytes.Length);
                this.PixelFormat = value.PixelFormat;
                this.SizeX = value.Width;
                this.SizeY = value.Height;
            }
        }

        public unsafe IntPtr Data
        {
            get
            {
                fixed (byte* numPtr = this.Bytes)
                    return (IntPtr)(void*)numPtr;
            }
        }

        public IntPtr RGBData => Bitmap.GetRGB32Data(this.SizeX, this.SizeY, this.PixelFormat, this.bytes);

        public Plane Plane
        {
            get => this.plane;
            set => this.plane = value;
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
            if (px == PixelFormat.Format24bppRgb || px == PixelFormat.Format32bppArgb || px == PixelFormat.Format32bppRgb)
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
                bitmapArray[0].plane = plane;
            bitmapArray[0].ID = Bitmap.CreateID(file, 0);
            bitmapArray[1].file = file;
            if (plane != null)
                bitmapArray[1].plane = plane;
            bitmapArray[1].ID = Bitmap.CreateID(file, 0);
            bitmapArray[2].file = file;
            if (plane != null)
                bitmapArray[2].plane = plane;
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
                        byts[num1 + num3] = (byte)0;
                        byts[num1 + num3 + 1] = (byte)0;
                        byts[num1 + num3 + 2] = bfs[1].Bytes[num2 + num4];
                        byts[num1 + num3 + 3] = bfs[1].Bytes[num2 + num4 + 1];
                        byts[num1 + num3 + 4] = bfs[0].Bytes[num2 + num4];
                        byts[num1 + num3 + 5] = bfs[0].Bytes[num2 + num4 + 1];
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
                    byts1[num5 + num7] = bfs[2].Bytes[num6 + num8];
                    byts1[num5 + num7 + 1] = bfs[2].Bytes[num6 + num8 + 1];
                    byts1[num5 + num7 + 2] = bfs[1].Bytes[num6 + num8];
                    byts1[num5 + num7 + 3] = bfs[1].Bytes[num6 + num8 + 1];
                    byts1[num5 + num7 + 4] = bfs[0].Bytes[num6 + num8];
                    byts1[num5 + num7 + 5] = bfs[0].Bytes[num6 + num8 + 1];
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
                        bts[num1 + num3] = (byte)0;
                        bts[num1 + num3 + 1] = bfs[1].Bytes[num2 + num4];
                        bts[num1 + num3 + 2] = bfs[0].Bytes[num2 + num4];
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
                    bts1[num5 + num7] = bfs[2].Bytes[num6 + num8];
                    bts1[num5 + num7 + 1] = bfs[1].Bytes[num6 + num8];
                    bts1[num5 + num7 + 2] = bfs[0].Bytes[num6 + num8];
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
                    bts[num1 + num3] = bfs.Bytes[num2 + num4];
                    bts[num1 + num3 + 1] = bfs.Bytes[num2 + num4];
                    bts[num1 + num3 + 2] = bfs.Bytes[num2 + num4];
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


        public static unsafe Bitmap GetBitmapRGB(int w, int h, PixelFormat px, byte[] bts)
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
                        int num8 = y * w * 4;
                        for (int x = 0; x < w; ++x)
                        {
                            int num9 = x * 4;
                            int index = x * 4;
                            byte[] bt = new byte[4];
                            bt[0] = bts[num8 + num9 + 3];
                            bt[1] = bts[num8 + num9 + 2];
                            bt[2] = bts[num8 + num9 + 1];
                            bt[3] = bts[num8 + num9];
                            float f = BitConverter.ToSingle(bt, 0) / ushort.MaxValue;
                            numPtr[index + 0] = (byte)(f * byte.MaxValue);
                            numPtr[index + 1] = (byte)(f * byte.MaxValue);
                            numPtr[index + 2] = (byte)(f * byte.MaxValue);
                            numPtr[index + 3] = byte.MaxValue;
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
                        int num1 = y * stride;
                        for (int x = 0; x < w; x++)
                        {
                            float f = BitConverter.ToSingle(bts, (y*stride)+(x*4));
                            float num3 = f - rr.Min;
                            if ((double)num3 < 0.0)
                                num3 = 0.0f;
                            int num4 = (int)((double)(num3 / rr.Max) * (double)byte.MaxValue);
                            bitmap6.SetValue(x, y, 3, (ushort)byte.MaxValue);
                            bitmap6.SetValue(x, y, 2, (ushort)num4);
                            bitmap6.SetValue(x, y, 1, (ushort)num4);
                            bitmap6.SetValue(x, y, 0, (ushort)num4);
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
            if (this.BitsPerPixel > 8)
            {
                if (this.RGBChannelsCount == 1)
                {
                    int num1 = 2;
                    int num2 = r.Width * num1;
                    int stride = this.Stride;
                    byte[] numArray = new byte[num2 * r.Height];
                    for (int index1 = 0; index1 < r.Height; ++index1)
                    {
                        for (int index2 = 0; index2 < num2; index2 += num1)
                        {
                            int index3 = index1 * num2 + index2;
                            int index4 = (index1 + r.Y) * stride + (index2 + r.X * num1);
                            numArray[index3] = this.bytes[index4];
                            numArray[index3 + 1] = this.bytes[index4 + 1];
                        }
                    }
                    this.bytes = numArray;
                }
                else
                {
                    int num3 = 6;
                    int num4 = r.Width * num3;
                    int stride = this.Stride;
                    byte[] numArray = new byte[num4 * r.Height];
                    for (int index5 = 0; index5 < r.Height; ++index5)
                    {
                        for (int index6 = 0; index6 < num4; index6 += num3)
                        {
                            int index7 = index5 * num4 + index6;
                            int index8 = (index5 + r.Y) * stride + (index6 + r.X * num3);
                            numArray[index7] = this.bytes[index8];
                            numArray[index7 + 1] = this.bytes[index8 + 1];
                            numArray[index7 + 2] = this.bytes[index8 + 2];
                            numArray[index7 + 3] = this.bytes[index8 + 3];
                            numArray[index7 + 4] = this.bytes[index8 + 4];
                            numArray[index7 + 5] = this.bytes[index8 + 5];
                        }
                    }
                    this.bytes = numArray;
                }
            }
            else
                this.Image = new AForge.Imaging.Filters.Crop(r).Apply(this.Image);
            this.SizeX = r.Width;
            this.SizeY = r.Height;
        }

        public UnmanagedImage GetCropBitmap(Rectangle r)
        {
            if (this.BitsPerPixel <= 8)
                return new AForge.Imaging.Filters.Crop(r).Apply(this.Image);
            if (this.RGBChannelsCount == 1)
            {
                int num = 2;
                int stride1 = r.Width * num;
                int stride2 = this.Stride;
                byte[] arr = new byte[stride1 * r.Height];
                for (int index1 = 0; index1 < r.Height; ++index1)
                {
                    for (int index2 = 0; index2 < stride1; index2 += num)
                    {
                        int index3 = (index1 * stride1 + index2) * this.RGBChannelsCount;
                        int index4 = ((index1 + r.Y) * stride2 + (index2 + r.X * num)) * this.RGBChannelsCount;
                        arr[index3] = this.bytes[index4];
                        arr[index3 + 1] = this.bytes[index4 + 1];
                    }
                }
                return new UnmanagedImage(Marshal.UnsafeAddrOfPinnedArrayElement<byte>(arr, 0), r.Width, r.Height, stride1, PixelFormat.Format16bppGrayScale);
            }
            int num1 = 6;
            int stride3 = r.Width * num1;
            int stride4 = this.Stride;
            byte[] arr1 = new byte[stride3 * r.Height];
            for (int index5 = 0; index5 < r.Height; ++index5)
            {
                for (int index6 = 0; index6 < stride3; index6 += num1)
                {
                    int index7 = index5 * stride3 + index6;
                    int index8 = (index5 + r.Y) * stride4 + (index6 + r.X * num1);
                    arr1[index7] = this.bytes[index8];
                    arr1[index7 + 1] = this.bytes[index8 + 1];
                    arr1[index7 + 2] = this.bytes[index8 + 2];
                    arr1[index7 + 3] = this.bytes[index8 + 3];
                    arr1[index7 + 4] = this.bytes[index8 + 4];
                    arr1[index7 + 5] = this.bytes[index8 + 5];
                }
            }
            return new UnmanagedImage(Marshal.UnsafeAddrOfPinnedArrayElement<byte>(arr1, 0), r.Width, r.Height, stride3, PixelFormat.Format48bppRgb);
        }

        public Bitmap GetCropBuffer(Rectangle r)
        {
            if (this.BitsPerPixel <= 8)
                return new Bitmap(new AForge.Imaging.Filters.Crop(r).Apply(this.Image));
            if (this.RGBChannelsCount == 1)
            {
                int num1 = 2;
                int num2 = r.Width * num1;
                int stride = this.Stride;
                byte[] bts = new byte[num2 * r.Height];
                for (int index1 = 0; index1 < r.Height; ++index1)
                {
                    for (int index2 = 0; index2 < num2; index2 += num1)
                    {
                        int index3 = (index1 * num2 + index2) * this.RGBChannelsCount;
                        int index4 = ((index1 + r.Y) * stride + (index2 + r.X * num1)) * this.RGBChannelsCount;
                        bts[index3] = this.bytes[index4];
                        bts[index3 + 1] = this.bytes[index4 + 1];
                    }
                }
                return new Bitmap(r.Width, r.Height, PixelFormat.Format16bppGrayScale, bts, this.Coordinate, this.ID);
            }
            int num3 = 6;
            int num4 = r.Width * num3;
            int stride1 = this.Stride;
            byte[] bts1 = new byte[num4 * r.Height];
            for (int index5 = 0; index5 < r.Height; ++index5)
            {
                for (int index6 = 0; index6 < num4; index6 += num3)
                {
                    int index7 = index5 * num4 + index6;
                    int index8 = (index5 + r.Y) * stride1 + (index6 + r.X * num3);
                    bts1[index7] = this.bytes[index8];
                    bts1[index7 + 1] = this.bytes[index8 + 1];
                    bts1[index7 + 2] = this.bytes[index8 + 2];
                    bts1[index7 + 3] = this.bytes[index8 + 3];
                    bts1[index7 + 4] = this.bytes[index8 + 4];
                    bts1[index7 + 5] = this.bytes[index8 + 5];
                }
            }
            return new Bitmap(r.Width, r.Height, PixelFormat.Format48bppRgb, bts1, this.Coordinate, this.ID);
        }

        public static byte[] ConvertToInterleaved(byte[] nonInterleaved, PixelFormat px)
        {
            if (px == PixelFormat.Format24bppRgb)
            {
                int numPixels = nonInterleaved.Length / 3;
                byte[] interleaved = new byte[nonInterleaved.Length];

                for (int i = 0; i < numPixels; i++)
                {
                    interleaved[3 * i] = nonInterleaved[i];                // Red
                    interleaved[3 * i + 1] = nonInterleaved[i + numPixels]; // Green
                    interleaved[3 * i + 2] = nonInterleaved[i + 2 * numPixels]; // Blue
                }
                return interleaved;
            }
            else if (px == PixelFormat.Format32bppArgb)
            {
                int numPixels = nonInterleaved.Length / 4;
                byte[] interleaved = new byte[nonInterleaved.Length];

                for (int i = 0; i < numPixels; i++)
                {
                    interleaved[4 * i] = nonInterleaved[i];                      // Red
                    interleaved[4 * i + 1] = nonInterleaved[i + numPixels];      // Green
                    interleaved[4 * i + 2] = nonInterleaved[i + 2 * numPixels];  // Blue
                    interleaved[4 * i + 3] = nonInterleaved[i + 3 * numPixels];  // Alpha
                }

                return interleaved;
            }
            else if (px == PixelFormat.Format48bppRgb)
            {
                int numPixels = nonInterleaved.Length / 6;
                byte[] interleaved = new byte[nonInterleaved.Length];

                for (int i = 0; i < numPixels; i++)
                {
                    interleaved[6 * i] = nonInterleaved[i * 2];                     // Red high byte
                    interleaved[6 * i + 1] = nonInterleaved[i * 2 + 1];             // Red low byte
                    interleaved[6 * i + 2] = nonInterleaved[numPixels * 2 + i * 2];     // Green high byte
                    interleaved[6 * i + 3] = nonInterleaved[numPixels * 2 + i * 2 + 1]; // Green low byte
                    interleaved[6 * i + 4] = nonInterleaved[numPixels * 4 + i * 2];     // Blue high byte
                    interleaved[6 * i + 5] = nonInterleaved[numPixels * 4 + i * 2 + 1]; // Blue low byte
                }
                return interleaved;
            }
            else if (px == PixelFormat.Float || px == PixelFormat.Short || px == PixelFormat.Format8bppIndexed || px == PixelFormat.Format16bppGrayScale)
                return nonInterleaved;
            else
                throw new NotSupportedException("PixelFormat " + px + " is not supported.");
        }

        private void Initialize(
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
            // Generate a unique ID for this bitmap based on file name and index
            this.ID = Bitmap.CreateID(file, index);

            // Set size and pixel format properties
            this.SizeX = w;
            this.SizeY = h;
            this.pixelFormat = px;
            this.Coordinate = coord;
            this.Plane = plane;

            // Handle interleaving if needed
            if (!interleaved)
            {
                // Ensure ConvertToInterleaved is correctly implemented to handle the pixel format
                this.Bytes = ConvertToInterleaved(byts, px);
            }
            else
            {
                this.Bytes = byts;
                // Directly assign the input bytes if already interleaved
                SwitchRedBlueIfNecessary();
            }

            // Handle byte order (endianess)
            if (!littleEndian)
            {
                // Reverse the byte order in chunks corresponding to the pixel format
                ReverseByteOrderByPixelFormat();

                // Optionally rotate the image if needed for endianness (though this is unusual)
                this.RotateFlip(RotateFlipType.Rotate180FlipNone);

                // Switch red and blue channels if necessary (only for formats like RGB/BGR)
                SwitchRedBlueIfNecessary();
            }
            if(PixelFormat == PixelFormat.Format32bppArgb)
            {
                //Let's check to see if channel 4 is transparent.
                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < SizeX; x++)
                    {
                        int v = (int)GetValue(x, y, 3);
                        if (v == 0)
                            SetValue(x, y, 3, (byte)255);
                    }
                }
            }
            // Calculate and assign statistics from the byte data (presumably for image analysis)
            this.stats = Statistics.FromBytes(this);
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

        // Switch red and blue channels if required (e.g., for RGB <-> BGR conversions)
        private void SwitchRedBlueIfNecessary()
        {
            if (this.pixelFormat == PixelFormat.Format24bppRgb ||
                this.pixelFormat == PixelFormat.Format32bppArgb ||
                this.pixelFormat == PixelFormat.Format32bppRgb)
            {
                int bytesPerPixel = GetPixelFormatSize(this.pixelFormat) / 8;

                for (int i = 0; i < this.Bytes.Length; i += bytesPerPixel)
                {
                    // Swap red (R) and blue (B) channels
                    byte temp = this.Bytes[i];
                    this.Bytes[i] = this.Bytes[i + 2];
                    this.Bytes[i + 2] = temp;
                }
            }
        }


        public Bitmap(string file, int w, int h, PixelFormat px, byte[] bts, ZCT coord, int index) => this.Initialize(file, w, h, px, bts, coord, index, (Plane)null);

        public Bitmap(int w, int h, PixelFormat px)
        {
            this.SizeX = w;
            this.SizeY = h;
            this.pixelFormat = px;
            this.Coordinate = new ZCT();
            this.Bytes = new byte[h * this.Stride];
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
            this.ID = Bitmap.CreateID(file, index);
            this.SizeX = im.Width;
            this.SizeY = im.Height;
            this.pixelFormat = im.PixelFormat;
            this.Coordinate = coord;
            this.Image = im;
            this.stats = Statistics.FromBytes(this);
        }

        public Bitmap(string file, UnmanagedImage im, ZCT coord, int index, Plane pl)
        {
            this.ID = Bitmap.CreateID(file, index);
            this.SizeX = im.Width;
            this.SizeY = im.Height;
            this.pixelFormat = im.PixelFormat;
            this.Coordinate = coord;
            this.Image = im;
            this.Plane = pl;
            this.stats = Statistics.FromBytes(this);
        }

        public Bitmap(int width, int height, int stride, PixelFormat pixelFormat, IntPtr imageData)
        {
            this.SizeX = width;
            this.SizeY = height;
            this.pixelFormat = pixelFormat;
            this.bytes = new byte[stride * height];
            Marshal.Copy(imageData, this.bytes, 0, stride * height);
            this.stats = Statistics.FromBytes(this);
        }

        public Bitmap(int width, int height, int stride, PixelFormat pixelFormat, short[] imageData)
        {
            this.SizeX = width;
            this.SizeY = height;
            this.pixelFormat = pixelFormat;
            this.stats = Statistics.FromBytes(this);
        }

        public Bitmap(UnmanagedImage im)
        {
            this.SizeX = im.Width;
            this.SizeY = im.Height;
            this.pixelFormat = im.PixelFormat;
            this.Coordinate = new ZCT();
            this.Image = im;
            this.stats = Statistics.FromBytes(this);
        }

        public Bitmap(int w, int h, PixelFormat px, byte[] bts, ZCT coord, string id) => this.Initialize(id, w, h, px, bts, coord, 0, (Plane)null);

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
            if (this.PixelFormat == PixelFormat.Format24bppRgb)
            {
                for (int index1 = 0; index1 < this.SizeY; ++index1)
                {
                    for (int index2 = 0; index2 < this.Stride; index2 += 3)
                    {
                        int index3 = index1 * this.Stride + index2;
                        byte num = this.bytes[index3 + 2];
                        this.bytes[index3 + 2] = this.bytes[index3];
                        this.bytes[index3] = num;
                    }
                }
            }
            else if (this.PixelFormat == PixelFormat.Format32bppArgb || this.PixelFormat == PixelFormat.Format32bppRgb)
            {
                for (int index4 = 0; index4 < this.SizeY; ++index4)
                {
                    for (int index5 = 0; index5 < this.Stride; index5 += 4)
                    {
                        int index6 = index4 * this.Stride + index5;
                        byte num = this.bytes[index6 + 2];
                        this.bytes[index6 + 2] = this.bytes[index6];
                        this.bytes[index6] = num;
                    }
                }
            }
            else
            {
                if (this.PixelFormat != PixelFormat.Format48bppRgb)
                    return;
                for (int index7 = 0; index7 < this.SizeY; ++index7)
                {
                    int num1 = index7 * this.Stride;
                    for (int index8 = 0; index8 < this.Stride; index8 += 6)
                    {
                        int num2 = index8;
                        byte num3 = this.bytes[num1 + num2];
                        byte num4 = this.bytes[num1 + num2 + 1];
                        this.bytes[num1 + num2] = this.bytes[num1 + num2 + 4];
                        this.bytes[num1 + num2 + 1] = this.bytes[num1 + num2 + 5];
                        this.bytes[num1 + num2 + 4] = num3;
                        this.bytes[num1 + num2 + 5] = num4;
                    }
                }
            }
        }

        public byte[] GetSaveBytes(bool littleEndian)
        {
            Bitmap bitmap = this.Copy();
            if (isRGB)
                bitmap.SwitchRedBlue();
            if (!littleEndian)
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            int length = this.bytes.Length;
            byte[] numArray = new byte[length];
            Marshal.Copy(bitmap.Data, numArray, 0, length);
            if (!littleEndian)
                Array.Reverse<byte>(numArray);
            return numArray;
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

        public static Bitmap To32Bit(Bitmap b) => b.ImageRGB;

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
                plane = this.Plane
            };
        }

        public Bitmap CopyInfo()
        {
            Bitmap bitmap = new Bitmap(this.SizeX, this.SizeY, this.PixelFormat, new byte[this.Stride * this.SizeY], this.Coordinate, this.ID);
            bitmap.bytes = new byte[bitmap.Stride * bitmap.SizeY];
            bitmap.plane = this.Plane;
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
        public void To8Bit()
        {
            Bitmap bitmap = AForge.Imaging.Image.Convert16bppTo8bpp(this);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            this.Image = (UnmanagedImage)bitmap;
        }

        public void To16Bit() => this.Image = (UnmanagedImage)AForge.Imaging.Image.Convert8bppTo16bpp(this);

        public void ToFloat()
        {
            Bitmap bm = new Bitmap(SizeX, SizeY, PixelFormat.Float);
            for (int y = 0; y < SizeY; y++)
            {
                for (int x = 0; x < SizeX; x++)
                {
                    float f = (float)GetValue(x, y);
                    bm.SetValue(x, y, f);
                }
            }
            PixelFormat = PixelFormat.Float;
            Bytes = SwitchEndianness(bm.Bytes);
            RotateFlip(RotateFlipType.Rotate180FlipNone);
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

        public bool isRGB => this.pixelFormat != PixelFormat.Format8bppIndexed && this.pixelFormat != PixelFormat.Format16bppGrayScale && this.pixelFormat != PixelFormat.Float && this.pixelFormat != PixelFormat.Short;

        public override string ToString() => this.ID;

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
            GC.Collect();
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
