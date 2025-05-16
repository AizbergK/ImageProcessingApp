using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace Algorithms.Sections
{
    public class GeometricTransformations
    {
        public static Image<Gray, byte> transfPr(Image<Gray, byte> inputImage, List<Tuple<int, int>> selectedRegion, List<Tuple<int, int>> resultRegion)
        {
            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);

            Matrix transform = transformMat(resultRegion, selectedRegion);
            for (int y = resultRegion[0].Item2; y < resultRegion[2].Item2; ++y)
                for(int x = resultRegion[0].Item1; x < resultRegion[2].Item1; ++x)
                {
                    Matrix px = DenseMatrix.OfArray(new double[,] { { x }, { y }, { 1 } });
                    Matrix pxFrom = transform * px;
                    double Xc = pxFrom[0, 0] / pxFrom[2, 0];
                    double Yc = pxFrom[1, 0] / pxFrom[2, 0];
                    int Xfloor = (int)Math.Floor(Xc);
                    int Yfloor = (int)Math.Floor(Yc);
                    if (Xfloor + 1 < inputImage.Width && Yfloor + 1 < inputImage.Height)
                    {
                        resultImage.Data[y, x, 0] = interpBilin(
                                inputImage.Data[Yfloor, Xfloor, 0],
                                inputImage.Data[Yfloor + 1, Xfloor, 0],
                                inputImage.Data[Yfloor, Xfloor + 1, 0],
                                inputImage.Data[Yfloor + 1, Xfloor + 1, 0],
                                Xc % 1, Yc % 1
                            );
                    }
                    else
                    { 
                        resultImage.Data[y, x, 0] = inputImage.Data[(int)Yc, (int)Xc, 0]; 
                    }
                }

            return resultImage;
        }

        public static Image<Bgr, byte> transfPr(Image<Bgr, byte> inputImage, List<Tuple<int, int>> selectedRegion, List<Tuple<int, int>> resultRegion)
        {
            Image<Bgr, byte> resultImage = new Image<Bgr, byte>(inputImage.Size);

            Matrix transform = transformMat(resultRegion, selectedRegion);
            for (int y = resultRegion[0].Item2; y < resultRegion[2].Item2; ++y)
                for (int x = resultRegion[0].Item1; x < resultRegion[2].Item1; ++x)
                {
                    Matrix px = DenseMatrix.OfArray(new double[,] { { x }, { y }, { 1 } });
                    Matrix pxFrom = transform * px;
                    double Xc = pxFrom[0, 0] / pxFrom[2, 0];
                    double Yc = pxFrom[1, 0] / pxFrom[2, 0];
                    int Xfloor = (int)Math.Floor(Xc);
                    int Yfloor = (int)Math.Floor(Yc);

                    for (int ch = 0; ch < 3; ++ch)
                    {
                        if (Xfloor + 1 < inputImage.Width && Yfloor + 1 < inputImage.Height)
                        {
                            resultImage.Data[y, x, ch] = interpBilin(
                                    inputImage.Data[Yfloor, Xfloor, ch],
                                    inputImage.Data[Yfloor + 1, Xfloor, ch],
                                    inputImage.Data[Yfloor, Xfloor + 1, ch],
                                    inputImage.Data[Yfloor + 1, Xfloor + 1, ch],
                                    Xc % 1, Yc % 1
                                );
                        }
                        else
                        {
                            resultImage.Data[y, x, ch] = inputImage.Data[(int)Yc, (int)Xc, ch];
                        }
                    }
                }

            return resultImage;
        }

        private static Matrix transformMat(List<Tuple<int, int>> notPirme, List<Tuple<int, int>> prime)
        {
            Matrix b = DenseMatrix.Create(3, 1, 0);
            Matrix bi = DenseMatrix.Create(3, 1, 0);

            Matrix P = DenseMatrix.OfArray(new double[,] { {notPirme[0].Item1, notPirme[1].Item1, notPirme[2].Item1 }, 
                                                           { notPirme[0].Item2, notPirme[1].Item2, notPirme[2].Item2 }, 
                                                           {1,1,1}});
            Matrix P4 = DenseMatrix.OfArray(new double[,] { { notPirme[3].Item1 }, { notPirme[3].Item2 }, { 1 } });

            Matrix Pi = DenseMatrix.OfArray(new double[,] { { prime[0].Item1, prime[1].Item1, prime[2].Item1 }, 
                                                            { prime[0].Item2, prime[1].Item2, prime[2].Item2 }, 
                                                            { 1, 1, 1 } });
            Matrix Pi4 = DenseMatrix.OfArray(new double[,] { { prime[3].Item1 }, { prime[3].Item2 }, { 1 } });

            b = P.Inverse() * P4;
            bi = Pi.Inverse() * Pi4;

            Matrix A = DenseMatrix.OfColumnVectors(bi[0, 0] / b[0, 0] * Pi.Column(0), 
                                                   bi[1, 0] / b[1, 0] * Pi.Column(1), 
                                                   bi[2, 0] / b[2, 0] * Pi.Column(2)) * P.Inverse();

            return A;
        }

        private static byte interpBilin(double p00, double p01, double p10, double p11, double xFrac, double yFrac)
        {
            double interp0 = xFrac * p10 + (1.0 - xFrac) * p00;
            double interp1 = xFrac * p11 + (1.0 - xFrac) * p01;
            double result = yFrac * interp1 + (1.0 - yFrac) * interp0;

            return (byte)(result + 0.5);
        }
    }
}