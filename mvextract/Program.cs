using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace mvextract
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length <= 0)
            {
                Console.WriteLine("Usage: mvextract.exe <file.mv>");
                return;
            }

            string file = args[0];

            if (!File.Exists(file))
            {
                Console.WriteLine("File not found: {0}", file);
                return;
            }

            Console.WriteLine("File = {0}", file);

            FileInfo fi = new FileInfo(file);
            string fileLocation = Path.GetDirectoryName(file);
            string fileWithoutExt = Path.GetFileNameWithoutExtension(file);
            string outputDir = Path.Combine(fileLocation, fileWithoutExt);

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            Console.WriteLine("Output Directory = {0}", outputDir);

            byte[] bytes = File.ReadAllBytes(file);
            byte[] buffer;

            buffer = new byte[4];
            Array.Copy(bytes, 0, buffer, 0, 4);
            Array.Reverse(buffer);
            int width = (int)BitConverter.ToUInt32(buffer, 0);

            buffer = new byte[4];
            Array.Copy(bytes, 4, buffer, 0, 4);
            Array.Reverse(buffer);
            int height = (int)BitConverter.ToUInt32(buffer, 0);

            buffer = new byte[8];
            Array.Copy(bytes, 8, buffer, 0, 8);
            Array.Reverse(buffer);
            int fileBytes = (int)BitConverter.ToUInt64(buffer, 0);

            buffer = new byte[4];
            Array.Copy(bytes, 16, buffer, 0, 4);
            Array.Reverse(buffer);
            int numFrames = (int)BitConverter.ToUInt32(buffer, 0);

            // Calculate bytes per frame from width and height
            int bytesPerFrame = width * height * 4;

            Console.WriteLine("Width = {0}", width);
            Console.WriteLine("Height = {0}", height);
            Console.WriteLine("Total Bytes = {0}", fileBytes);
            Console.WriteLine("Real Filesize = {0}", fi.Length);
            Console.WriteLine("Frames = {0}", numFrames);
            Console.WriteLine("Bytes Per Frame = {0}", bytesPerFrame);

            for (int frameNum = 1; frameNum <= numFrames; frameNum++)
            {
                int start = (frameNum * bytesPerFrame) - bytesPerFrame;
                if (start < 0) start = 0;
                int end = start + bytesPerFrame;

                // The image is formatted as RGBA but it needs to be in ARGB
                for (int i = start + 32; i < end - 32; i += 4)
                {
                    byte R = bytes[i];
                    byte G = bytes[i + 1];
                    byte B = bytes[i + 2];
                    byte A = bytes[i + 3];

                    bytes[i] = B;
                    bytes[i + 1] = G;
                    bytes[i + 2] = R;
                    bytes[i + 3] = A;
                }

                Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
                var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(bytes, start + 32, bitmapData.Scan0, bytesPerFrame - 32);
                bmp.UnlockBits(bitmapData);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bmp.Save(Path.Combine(outputDir, String.Format("frame{0}.png", frameNum)), ImageFormat.Png);
            }

            Console.WriteLine("Exported {0} frames", numFrames);
        }


    }
}
