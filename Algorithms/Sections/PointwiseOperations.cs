using Emgu.CV.Structure;
using Emgu.CV;
using System.Collections.Generic;

namespace Algorithms.Sections
{
    public class PointwiseOperations
    {
        public static Image<Gray, byte> applySpline(Image<Gray, byte> inputImage, byte[] function)
        {
            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);

            for (int i = 0; i < inputImage.Height; ++i)
            {
                for(int j = 0;  j < inputImage.Width; ++j)
                {
                    resultImage.Data[i, j, 0] = function[inputImage.Data[i, j, 0]];
                }
            }

            return resultImage;
        }

        public static Image<Bgr, byte> applySpline(Image<Bgr, byte> inputImage, byte[] function)
        {
            Image<Bgr, byte> resultImage = new Image<Bgr, byte>(inputImage.Size);

            for (int i = 0; i < inputImage.Height; ++i)
            {
                for (int j = 0; j < inputImage.Width; ++j)
                {
                    for(int ch = 0; ch < 3; ++ch)
                    {
                        resultImage.Data[i, j, ch] = function[inputImage.Data[i, j, ch]];
                    }
                }
            }

            return resultImage;
        }
    }
}