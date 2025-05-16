using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Web.UI;
using System;
using System.Linq;

namespace Algorithms.Sections
{
    public class MorphologicalOperations
    {
        public static Image<Bgr, byte> compConexe(Image<Gray, byte> inputImage)
        {
            Image<Bgr, byte> resultImage = new Image<Bgr, byte>(inputImage.Size);

            int compNo = 1;
            for(int i = 0; i < inputImage.Height; ++i)
            {
                for(int j = 0; j < inputImage.Width; ++j)
                {
                    if (inputImage.Data[i, j, 0] == 255 && resultImage.Data[i, j, 0] == 0)
                    {
                        bfs(i, j, compNo, ref inputImage, ref resultImage);
                    }            
                }
            }

            return resultImage;
        }

        private static void bfs(int x, int y, int compNo, ref Image<Gray, byte> inputImage, ref Image<Bgr, byte> resultImage)
        {
            Queue<Tuple<int, int>> pxQueue = new Queue<Tuple<int, int>>();
            Random rnd = new Random();  
            Tuple<byte, byte, byte> color = Tuple.Create(
                (byte)rnd.Next(1, 256),
                (byte)rnd.Next(1, 256),
                (byte)rnd.Next(1, 256)
            );

            pxQueue.Enqueue(new Tuple<int, int>(x, y));
            while(pxQueue.Count > 0)
            {
                x = pxQueue.First().Item1;
                y = pxQueue.First().Item2;
                for(int i = -1; i <= 1; ++i)
                {    
                    for (int j = -1; j <= 1; ++j)
                    {
                        if (x + i >= 0 && x + i < inputImage.Height &&
                            y + j >= 0 && y + j < inputImage.Width &&
                            inputImage.Data[x + i, y + j, 0] == 255 &&
                            resultImage.Data[x + i, y + j, 0] == 0)
                        {
                            pxQueue.Enqueue(new Tuple<int, int>(x + i, y + j));
                            resultImage.Data[x + i, y + j, 0] = color.Item1;
                            resultImage.Data[x + i, y + j, 1] = color.Item2;
                            resultImage.Data[x + i, y + j, 2] = color.Item3;
                        }
                    }
                }

                pxQueue.Dequeue();
            }
        }
    }
}