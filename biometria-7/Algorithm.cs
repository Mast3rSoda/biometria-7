using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace biometria_7
{
    public static class Algorithm
    {
        public static Bitmap KMM(Bitmap bmp)
        {
            int[] val = new int[] { 3, 5, 7, 12, 13, 14, 15, 20,
                                21, 22, 23, 28, 29, 30, 31, 48,
                                52, 53, 54, 55, 56, 60, 61, 62,
                                63, 65, 67, 69, 71, 77, 79, 80,
                                81, 83, 84, 85, 86, 87, 88, 89,
                                91, 92, 93, 94, 95, 97, 99, 101,
                                103, 109, 111, 112, 113, 115, 116, 117,
                                118, 119, 120, 121, 123, 124, 125, 126,
                                127, 131, 133, 135, 141, 143, 149, 151,
                                157, 159, 181, 183, 189, 191, 192, 193,
                                195, 197, 199, 205, 207, 208, 209, 211,
                                212, 213, 214, 215, 216, 217, 219, 220,
                                221, 222, 223, 224, 225, 227, 229, 231,
                                237, 239, 240, 241, 243, 244, 245, 246,
                                247, 248, 249, 251, 252, 253, 254, 255 };

            byte[,] grayS = ImageTo2DByteArray(bmp);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] vs = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, vs, 0, vs.Length);


            for (int i = 0; i < data.Height; i++)
                for (int y = 0; y < data.Width; y++)
                    grayS[i, y] = (byte)(grayS[i, y] < 25 ? 1 : 0);
            bool c = false;
            do
            {
                c = false;
                for (int i = 0; i < data.Height; i++)
                    for (int y = 0; y < data.Width; y++)
                    {
                        if (grayS[i, y] == 1)
                        {
                            if (i != 0 && grayS[i - 1, y] == 0)
                                grayS[i, y] = 2;

                            else if (i != data.Height - 1 && grayS[i + 1, y] == 0)
                                grayS[i, y] = 2;

                            else if (y != 0 && grayS[i, y - 1] == 0)
                                grayS[i, y] = 2;

                            else if (y != data.Width - 1 && grayS[i, y + 1] == 0)
                                grayS[i, y] = 2;

                            else if (i != 0 && y != 0 && grayS[i - 1, y - 1] == 0)
                                grayS[i, y] = 3;

                            else if (i != 0 && y != data.Width - 1 && grayS[i - 1, y + 1] == 0)
                                grayS[i, y] = 3;

                            else if (i != data.Height - 1 && y != 0 && grayS[i + 1, y - 1] == 0)
                                grayS[i, y] = 3;

                            else if (i != data.Height - 1 && y != data.Width - 1 && grayS[i + 1, y + 1] == 0)
                                grayS[i, y] = 3;
                        }
                        else
                            vs[(i * data.Stride) + y * 3] = 255;
                    }
                for (int n = 2; n < 4; n++)
                    for (int i = 0; i < data.Height; i++)
                        for (int y = 0; y < data.Width; y++)
                        {
                            if (grayS[i, y] == n)
                                if (val.Contains(CalculateWeight(i, y, grayS, data.Width, data.Height)))
                                {
                                    grayS[i, y] = 0;
                                    vs[(i * data.Stride) + (y * 3)] =
                                    vs[(i * data.Stride) + (y * 3) + 1] =
                                    vs[(i * data.Stride) + (y * 3) + 2] = byte.MaxValue;
                                    c = true;
                                }
                                else
                                    grayS[i, y] = 1;
                        }
            } while (c);
            Marshal.Copy(vs, 0, data.Scan0, vs.Length);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static int CalculateWeight(int i, int j, byte[,] grayS, int w, int h)
        {
            int[] N = new int[] { 128, 1, 2, 64, 0, 4, 32, 16, 8 };
            int weight = 0;
            if (i - 1 > 0 && j - 1 > 0 && grayS[i - 1, j - 1] != 0)
                weight += N[0];
            if (j - 1 > 0 && grayS[i, j - 1] != 0)
                weight += N[1];
            if (i + 1 < h && j - 1 > 0 && grayS[i + 1, j - 1] != 0)
                weight += N[2];
            if (i - 1 > 0 && grayS[i - 1, j] != 0)
                weight += N[3];
            if (i + 1 < h && grayS[i + 1, j] != 0)
                weight += N[5];
            if (i - 1 > 0 && j + 1 < w && grayS[i - 1, j + 1] != 0)
                weight += N[6];
            if (j + 1 < w && grayS[i, j + 1] != 0)
                weight += N[7];
            if (i + 1 < h && j + 1 < w && grayS[i + 1, j + 1] != 0)
                weight += N[8];
            return weight;
        }

        public static byte[,] ImageTo2DByteArray(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            byte[] bytes = new byte[height * data.Stride];
            try
            {
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            byte[,] result = new byte[height, width];
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    int offset = y * data.Stride + x * 3;
                    result[y, x] = (byte)((bytes[offset + 0] + bytes[offset + 1] + bytes[offset + 2]) / 3);
                }
            return result;
        }

    }
}
