using Emgu.CV.Structure;
using Emgu.CV;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Sections
{
    public class Canny
    {
        public static Image<Gray, byte> cannyGray(Image<Gray, byte> inputImage, int T1, int T2)
        {
            double[,] gradients = new double[inputImage.Height, inputImage.Width];
            double[,] angles = new double[inputImage.Height, inputImage.Width];

            Image<Gray, byte> gaussianImage = gaussianFilter(inputImage);
            Image<Gray, byte> sobelImage = sobelFilter(gaussianImage, ref gradients, ref angles);
            Image<Gray, byte> nonMaximaImage = nonMaximaFilter(sobelImage, ref gradients, ref angles);
            Image<Gray, byte> cannyImage = histTresh(nonMaximaImage, T1, T2);

            Image<Gray, byte> returnImage;
            switch (T1)
            {
                case 0:
                    return inputImage;
                case 1:
                    return gaussianImage;
                case 2:
                    return sobelImage;
                case 3:
                    return nonMaximaImage;
                case 4:
                    return sobelFilter(inputImage, ref gradients, ref angles);
            }    
            return cannyImage;
        }

        private static Image<Gray, byte> gaussianFilter(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> gaussianResult = new Image<Gray, byte>(inputImage.Size);

            int maskRadius = 2;
            double[,] gauss = { {0.018, 0.082, 0.135, 0.082, 0.018},
                                {0.082, 0.367, 0.606, 0.367, 0.082},
                                {0.135, 0.606, 1.000, 0.606, 0.135},
                                {0.082, 0.367, 0.606, 0.367, 0.082},
                                {0.018, 0.082, 0.135, 0.082, 0.018}};
            double gaussFactor = gauss.Cast<double>().Sum();

            for (int i = maskRadius; i < inputImage.Height - maskRadius; ++i)
            {   
                for (int j = maskRadius; j < inputImage.Width - maskRadius; ++j)
                {
                    double pixel = 0;
                    for (int v = -maskRadius; v <= maskRadius; ++v)
                    {    
                        for (int w = -maskRadius; w <= maskRadius; ++w)
                        {
                            pixel += gauss[v + maskRadius, w + maskRadius] * inputImage.Data[i + v, j + w, 0];
                            gaussianResult.Data[i, j, 0] = (byte)(pixel  / gaussFactor);
                        }
                    }
                }
            }

            for (int i = 0; i < inputImage.Height; ++i)
            { 
                gaussianResult.Data[i, 0, 0] = inputImage.Data[i, 0, 0];
                gaussianResult.Data[i, 1, 0] = inputImage.Data[i, 1, 0];
                gaussianResult.Data[i, inputImage.Width - 2, 0] = inputImage.Data[i, inputImage.Width - 2, 0];
                gaussianResult.Data[i, inputImage.Width - 1, 0] = inputImage.Data[i, inputImage.Width - 1, 0];
            }
            for (int i = 0; i < inputImage.Width; ++i)
            {
                gaussianResult.Data[0, i, 0] = inputImage.Data[0, i, 0];
                gaussianResult.Data[1, i, 0] = inputImage.Data[1, i, 0];
                gaussianResult.Data[inputImage.Height - 2, i, 0] = inputImage.Data[inputImage.Height - 2, i, 0];
                gaussianResult.Data[inputImage.Height - 1, i, 0] = inputImage.Data[inputImage.Height - 1, i, 0];
            }

            return gaussianResult;
        }
        private static Image<Gray, byte> sobelFilter(Image<Gray, byte> inputImage, ref double[,] gradients, ref double[,] angles)
        {
            Image<Gray, byte> sobelResult = new Image<Gray, byte>(inputImage.Size);

            int[,] gradientX = new int[inputImage.Height, inputImage.Width];
            int[,] gradientY = new int[inputImage.Height, inputImage.Width];

            int[,] maskX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] maskY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int i = 1; i < inputImage.Height - 1; ++i)
                for (int j = 1; j < inputImage.Width - 1; ++j)
                {
                    for (int v = -1; v <= 1; ++v)
                        for (int w = -1; w <= 1; ++w)
                        {
                            gradientX[i, j] += maskX[1 + v, 1 + w] * inputImage.Data[i + v, j + w, 0];
                            gradientY[i, j] += maskY[1 + v, 1 + w] * inputImage.Data[i + v, j + w, 0];
                        }
                }

            double step = Math.PI / 8;
            for (int i = 1; i < inputImage.Height - 1; ++i)
                for (int j = 1; j < inputImage.Width - 1; ++j)
                {
                    gradients[i, j] = Math.Sqrt(Math.Pow(gradientX[i, j], 2) + Math.Pow(gradientY[i, j], 2));
                    angles[i, j] = Math.Atan2(gradientY[i, j], gradientX[i, j]);
                    if (angles[i, j] <= step && angles[i, j] >= -step || angles[i, j] <= -7 * step && angles[i, j] >= 7 * step)
                        angles[i, j] = 0; //dir0 horizontal
                    else if(angles[i, j] < -step && angles[i, j] > -3 *step || angles[i, j] < 7 * step && angles[i, j] > 5 * step)
                        angles[i, j] = 1; //dir1 2nd diag
                    else if(angles[i, j] < -3 * step && angles[i, j] > -5 * step || angles[i, j] < 5 * step && angles[i, j] > 3 * step)
                        angles[i, j] = 2; //dir2 vertical
                    else
                        angles[i, j] = 3; //dir3 main diag
                }

            double max = 0;
            for (int i = 0; i < inputImage.Height; ++i)
                for (int j = 0; j < inputImage.Width; ++j)
                {
                    if (max < gradients[i, j])
                        max = gradients[i, j];
                }
            for (int i = 0; i < inputImage.Height; ++i)
                for (int j = 0; j < inputImage.Width; ++j)
                    sobelResult.Data[i, j, 0] = (byte)(gradients[i, j] / max * 255);

            return sobelResult;

        }

        private static Image<Gray, byte> nonMaximaFilter(Image<Gray, byte> inputImage, ref double[,] gradients, ref double[,] angles)
        {
            Image<Gray, byte> nonMaxResult = new Image<Gray, byte>(inputImage.Size);
            for (int i = 0; i < inputImage.Height; ++i)
                for (int j = 0; j < inputImage.Width; ++j)
                    nonMaxResult.Data[i, j, 0] = inputImage.Data[i, j, 0];

            double step = Math.PI / 8;
            for (int i = 1; i < inputImage.Height - 1; ++i)
                for (int j = 1; j < inputImage.Width - 1; ++j)
                {
                    //horizontal
                    if (angles[i, j] == 0 && (gradients[i, j] < gradients[i, j - 1] || gradients[i, j] < gradients[i, j + 1]))
                        nonMaxResult.Data[i, j, 0] = 0;
                    //other diag
                    else if (angles[i, j] == 1 && (gradients[i, j] < gradients[i - 1, j + 1] || gradients[i, j] < gradients[i + 1, j - 1]))
                        nonMaxResult.Data[i, j, 0] = 0;
                    //vertical
                    else if (angles[i, j] == 2 && (gradients[i, j] < gradients[i + 1, j] || gradients[i, j] < gradients[i - 1, j]))
                        nonMaxResult.Data[i, j, 0] = 0;
                    //main diag
                    else if (angles[i, j] == 3 && (gradients[i, j] < gradients[i + 1, j + 1] || gradients[i, j] < gradients[i - 1, j - 1]))
                        nonMaxResult.Data[i, j, 0] = 0;
                }

            return nonMaxResult;
        }

        private static Image<Gray, byte> histTresh(Image<Gray, byte> inputImage, int T1, int T2)
        {
            Image<Gray, byte> cannyResult = new Image<Gray, byte>(inputImage.Size);
            for (int i = 1; i < inputImage.Height - 1; ++i)
                for (int j = 1; j < inputImage.Width - 1; ++j)
                {
                    if (inputImage.Data[i, j, 0] <= T1)
                        cannyResult.Data[i, j, 0] = 0;
                    else if (inputImage.Data[i, j, 0] > T2)
                        cannyResult.Data[i, j, 0] = 255;
                    else
                    {
                        if (inputImage.Data[i + 1, j, 0] > T2 || inputImage.Data[i - 1, j, 0] > T2 || 
                            inputImage.Data[i, j + 1, 0] > T2 || inputImage.Data[i, j - 1, 0] > T2 ||
                            inputImage.Data[i + 1, j + 1, 0] > T2 || inputImage.Data[i + 1, j - 1, 0] > T2 || 
                            inputImage.Data[i - 1, j + 1, 0] > T2 || inputImage.Data[i - 1, j - 1, 0] > T2)
                            cannyResult.Data[i, j, 0] = 255;
                        else
                            cannyResult.Data[i, j, 0] = 0;
                    }
                }

            return cannyResult;
        }
    }
}
