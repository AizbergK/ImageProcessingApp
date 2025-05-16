using Emgu.CV.Structure;
using Emgu.CV;

using OpenCvSharp;
using System.Security.Cryptography;
using System;

namespace Algorithms.Sections
{
    public class FastMedian
    {
        public static Image<Gray, byte> fastMedianGray(Image<Gray, byte> inputImage, int radius)
        {
            int[,] rowHist = new int[inputImage.Width, 256];
            int[] kernelHist = new int[256];
            Image<Gray, byte> newImage = new Image<Gray, byte>(inputImage.Size);

            for (int i = 0; i < radius; ++i)
            {
                for (int j = 0; j < inputImage.Width; ++j) //initializam ca si cand am fi pe linia -1
                {
                    ++rowHist[j, inputImage.Data[i, j, 0]];
                }
            }


            for (int y = 0; y < inputImage.Height; ++y)
            {
                //init kernel at start of new row
                kernelHist = new int[256];
                for (int k = 0; k < radius; ++k)
                {
                    if (y - radius - 1 >= 0)
                    {
                        --rowHist[k, inputImage.Data[y - radius - 1, k, 0]];
                    }
                    if (y + radius < inputImage.Height)
                    {
                        ++rowHist[k, inputImage.Data[y + radius, k, 0]];
                    }
                    for (int l = 0; l < 256; ++l)
                    {
                        kernelHist[l] += rowHist[k, l];
                    }
                }

                for (int x = 0; x < inputImage.Width; ++x)
                {
                    if (x + radius < inputImage.Width)
                    {
                        if (y - radius - 1 >= 0)
                        {
                            --rowHist[x + radius, inputImage.Data[y - radius - 1, x + radius, 0]];
                        }
                        if (y + radius < inputImage.Height)
                        {
                            ++rowHist[x + radius, inputImage.Data[y + radius, x + radius, 0]];
                        }
                        for (int k = 0; k < 256; ++k)
                        {
                            kernelHist[k] += rowHist[x + radius, k];
                        }
                    }

                    if (x - radius - 1 >= 0)
                    {
                        for (int k = 0; k < 256; ++k)
                        {
                            kernelHist[k] -= rowHist[x - radius - 1, k];
                        }
                    }

                    newImage.Data[y, x, 0] = median(ref kernelHist);
                }
            }

            return newImage;
        }
        public static Image<Bgr, byte> fastMedianColor(Image<Bgr, byte> inputImage, int radius)
        {

            Image<Bgr, byte> newImage = new Image<Bgr, byte>(inputImage.Size);

            for (int channel = 0; channel < 3; ++channel)
            {
                int[,] rowHist = new int[inputImage.Width, 256];
                int[] kernelHist = new int[256];
                for (int i = 0; i < radius; ++i)
                {
                    for (int j = 0; j < inputImage.Width; ++j) //initializam ca si cand am fi pe linia -1
                    {
                        ++rowHist[j, inputImage.Data[i, j, channel]];
                    }
                }

                for (int i = 0; i < inputImage.Height; ++i)
                {
                    //init kernel at start of new row
                    kernelHist = new int[256];
                    for (int k = 0; k < radius; ++k)
                    {
                        if (i - radius - 1 >= 0)
                        {
                            --rowHist[k, inputImage.Data[i - radius - 1, k, channel]];
                        }
                        if (i + radius < inputImage.Height)
                        {
                            ++rowHist[k, inputImage.Data[i + radius, k, channel]];
                        }
                        for (int l = 0; l < 256; ++l)
                        {
                            kernelHist[l] += rowHist[k, l];
                        }
                    }

                    for (int j = 0; j < inputImage.Width; ++j)
                    {
                        if (j + radius < inputImage.Width)
                        {
                            if (i - radius - 1 >= 0)
                            {
                                --rowHist[j + radius, inputImage.Data[i - radius - 1, j + radius, channel]];
                            }
                            if (i + radius < inputImage.Height)
                            {
                                ++rowHist[j + radius, inputImage.Data[i + radius, j + radius, channel]];
                            }
                            for (int k = 0; k < 256; ++k)
                            {
                                kernelHist[k] += rowHist[j + radius, k];
                            }
                        }

                        if (j - radius - 1 >= 0)
                        {
                            for (int k = 0; k < 256; ++k)
                            {
                                kernelHist[k] -= rowHist[j - radius - 1, k];
                            }
                        }

                        newImage.Data[i, j, channel] = median(ref kernelHist);
                    }
                }
            }
            return newImage;
        }

        private static byte median(ref int[] kernelHist)
        {
            int pixelCounted = 0, totalPixels = 0;
            byte medianVal;

            for (int i = 0; i < 256; ++i)
            {
                totalPixels += kernelHist[i];
            }

            for (medianVal = 0; pixelCounted < (totalPixels + 1) / 2; ++medianVal)
            {
                pixelCounted += kernelHist[medianVal];
            }
            --medianVal;
            return medianVal;
        }
    }


}