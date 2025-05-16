using Emgu.CV.Structure;
using Emgu.CV;
using System.Dynamic;
using System.Security.Cryptography;
using System;

namespace Algorithms.Sections
{
    public class Thresholding
    {
        public static Image<Gray, byte> OtsuTwoStep(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);
            int[] histograma = new int[256];
            int totalPixeli = inputImage.Height * inputImage.Width;
            for (int i = 0; i < inputImage.Size.Height; ++i)
                for (int j = 0; j < inputImage.Size.Width; ++j)
                {
                    ++histograma[inputImage.Data[i, j, 0]];
                }
            double averageValue = 0;
            for (int i = 0; i < 256; ++i)
                averageValue += histograma[i] * i;
            averageValue /= totalPixeli;

            int t1 = 0, t2 = 1;
            double sig_max = 0;

            for (int T1 = 0; T1 <= 253; ++T1)
                for (int T2 = T1 + 1; T2 <= 254; ++T2)
                {
                    double sig = 0, P1 = 0, P2 = 0, P3 = 0, u1 = 0, u2 = 0, u3 = 0;

                    for (int i = 0; i <= T1; ++i)
                    {
                        double pk = histograma[i] / (double)totalPixeli;
                        P1 += pk;
                        u1 += i * pk;
                    }
                    u1 /= P1;
                    for (int i = T1 + 1; i <= T2; ++i)
                    {
                        double pk = histograma[i] / (double)totalPixeli;
                        P2 += pk;
                        u2 += i * pk;
                    }
                    u2 /= P2;
                    for (int i = T2 + 1; i <= 255; ++i)
                    {
                        double pk = histograma[i] / (double)totalPixeli;
                        P3 += pk;
                        u3 += i * pk;
                    }
                    u3 /= P3;
                    double Ptot = P1 + P2 + P3;
                    sig = P1 * Math.Pow((u1 - averageValue), 2) +
                            P2 * Math.Pow((u2 - averageValue), 2) +
                            P3 * Math.Pow((u3 - averageValue), 2);

                    if (sig > sig_max)
                    {
                        sig_max = sig;
                        t1 = T1;
                        t2 = T2;
                    }
                }

            for (int i = 0; i < inputImage.Size.Height; ++i)
                for (int j = 0; j < inputImage.Size.Width; ++j)
                {
                    if (inputImage.Data[i, j, 0] <= t1)
                        resultImage.Data[i, j, 0] = 0;
                    else if (inputImage.Data[i, j, 0] > t2)
                        resultImage.Data[i, j, 0] = 255;
                    else
                        resultImage.Data[i, j, 0] = 128;
                }
            return resultImage;
        }

        public static Image<Gray, byte> StepTresh(Image<Gray, byte> inputImage, int step)
        {
            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);

            for (int i = 0; i < inputImage.Height; ++i)
                for (int j = 0; j < inputImage.Width; ++j)
                    if (inputImage.Data[i, j, 0] < step)
                        resultImage.Data[i, j, 0] = 0;
                    else
                        resultImage.Data[i, j, 0] = 255;

            return resultImage;
        }
    }
}