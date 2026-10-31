using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Generates assets\icon.ico (multi-resolution) for Lucent Display Control.
// Usage: IconGen.exe <output.ico> [preview256.png]
public static class IconGen {
    static Bitmap Render(int s) {
        Bitmap bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp)) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            float pad = Math.Max(0.5f, s * 0.04f);
            float size = s - pad * 2;
            float r = size * 0.24f;
            using (GraphicsPath bg = RoundRect(pad, pad, size, size, r)) {
                using (LinearGradientBrush lgb = new LinearGradientBrush(new PointF(pad, pad), new PointF(pad + size, pad + size),
                    Color.FromArgb(56, 189, 248), Color.FromArgb(99, 102, 241))) {
                    g.FillPath(lgb, bg);
                }
                // soft top highlight
                using (LinearGradientBrush hl = new LinearGradientBrush(new RectangleF(pad, pad, size, size * 0.55f),
                    Color.FromArgb(70, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f)) {
                    g.SetClip(bg);
                    g.FillRectangle(hl, pad, pad, size, size * 0.55f);
                    g.ResetClip();
                }
            }

            // white diamond (the "lens")
            float c = s / 2f;
            float d = s * 0.30f;
            PointF[] diamond = new PointF[] { new PointF(c, c - d), new PointF(c + d, c), new PointF(c, c + d), new PointF(c - d, c) };
            using (SolidBrush white = new SolidBrush(Color.White)) {
                g.FillPolygon(white, diamond);
            }

            // inner pupil with the brand gradient (skipped on tiny sizes)
            if (s >= 32) {
                float pr = s * 0.115f;
                using (LinearGradientBrush pg = new LinearGradientBrush(new PointF(c - pr, c - pr), new PointF(c + pr, c + pr),
                    Color.FromArgb(56, 189, 248), Color.FromArgb(99, 102, 241))) {
                    g.FillEllipse(pg, c - pr, c - pr, pr * 2, pr * 2);
                }
            }
        }
        return bmp;
    }

    static GraphicsPath RoundRect(float x, float y, float w, float h, float r) {
        GraphicsPath p = new GraphicsPath();
        float d = r * 2;
        p.AddArc(x, y, d, d, 180, 90);
        p.AddArc(x + w - d, y, d, d, 270, 90);
        p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
        p.AddArc(x, y + h - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    static byte[] ToPng(Bitmap b) {
        using (MemoryStream ms = new MemoryStream()) {
            b.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }

    static byte[] ToDib(Bitmap b) {
        int s = b.Width;
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter w = new BinaryWriter(ms)) {
            int maskStride = ((s + 31) / 32) * 4;
            w.Write(40); w.Write(s); w.Write(s * 2); w.Write((short)1); w.Write((short)32);
            w.Write(0); w.Write(s * s * 4 + maskStride * s); w.Write(0); w.Write(0); w.Write(0); w.Write(0);
            BitmapData bd = b.LockBits(new Rectangle(0, 0, s, s), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] row = new byte[s * 4];
            for (int y = s - 1; y >= 0; y--) {
                System.Runtime.InteropServices.Marshal.Copy(new IntPtr(bd.Scan0.ToInt64() + (long)y * bd.Stride), row, 0, row.Length);
                w.Write(row);
            }
            b.UnlockBits(bd);
            w.Write(new byte[maskStride * s]);
            return ms.ToArray();
        }
    }

    public static int Main(string[] args) {
        int[] sizes = new int[] { 16, 20, 24, 32, 40, 48, 64, 256 };
        List<byte[]> images = new List<byte[]>();
        Bitmap preview = null;
        foreach (int s in sizes) {
            using (Bitmap b = Render(s)) {
                images.Add(s >= 64 ? ToPng(b) : ToDib(b));
                if (s == 256) preview = (Bitmap)b.Clone();
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0])));
        using (FileStream fs = File.Create(args[0]))
        using (BinaryWriter w = new BinaryWriter(fs)) {
            w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
            int offset = 6 + 16 * sizes.Length;
            for (int i = 0; i < sizes.Length; i++) {
                int s = sizes[i];
                w.Write((byte)(s >= 256 ? 0 : s)); w.Write((byte)(s >= 256 ? 0 : s));
                w.Write((byte)0); w.Write((byte)0);
                w.Write((short)1); w.Write((short)32);
                w.Write(images[i].Length); w.Write(offset);
                offset += images[i].Length;
            }
            foreach (byte[] img in images) w.Write(img);
        }
        if (args.Length > 1 && preview != null) preview.Save(args[1], ImageFormat.Png);
        Console.WriteLine("Icon written: " + args[0]);
        return 0;
    }
}
