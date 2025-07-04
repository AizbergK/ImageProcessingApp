﻿using Emgu.CV;
using Emgu.CV.Structure;

using System.Windows;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

using Framework.View;
using static Framework.Utilities.DataProvider;
using static Framework.Utilities.DrawingHelper;
using static Framework.Utilities.FileHelper;
using static Framework.Converters.ImageConverter;

using Algorithms.Sections;
using Algorithms.Tools;
using Algorithms.Utilities;
using System.Windows.Controls.Primitives;
using System;
using PointCollection = System.Windows.Media.PointCollection;
using System.Linq;
using System.Drawing;


namespace Framework.ViewModel
{
    public class MenuCommands : BaseVM
    {
        private readonly MainVM _mainVM;

        public MenuCommands(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

        private ImageSource InitialImage
        {
            get => _mainVM.InitialImage;
            set => _mainVM.InitialImage = value;
        }

        private ImageSource ProcessedImage
        {
            get => _mainVM.ProcessedImage;
            set => _mainVM.ProcessedImage = value;
        }

        private double ScaleValue
        {
            get => _mainVM.ScaleValue;
            set => _mainVM.ScaleValue = value;
        }

        #region File

        #region Load grayscale image
        private RelayCommand _loadGrayscaleImageCommand;
        public RelayCommand LoadGrayscaleImageCommand
        {
            get
            {
                if (_loadGrayscaleImageCommand == null)
                    _loadGrayscaleImageCommand = new RelayCommand(LoadGrayscaleImage);
                return _loadGrayscaleImageCommand;
            }
        }

        private void LoadGrayscaleImage(object parameter)
        {
            Clear(parameter);

            string fileName = LoadFileDialog("Select a grayscale picture");
            if (fileName != null)
            {
                GrayInitialImage = new Image<Gray, byte>(fileName);
                InitialImage = Convert(GrayInitialImage);
            }
        }
        #endregion

        #region Load color image
        private ICommand _loadColorImageCommand;
        public ICommand LoadColorImageCommand
        {
            get
            {
                if (_loadColorImageCommand == null)
                    _loadColorImageCommand = new RelayCommand(LoadColorImage);
                return _loadColorImageCommand;
            }
        }

        private void LoadColorImage(object parameter)
        {
            Clear(parameter);

            string fileName = LoadFileDialog("Select a color picture");
            if (fileName != null)
            {
                ColorInitialImage = new Image<Bgr, byte>(fileName);
                InitialImage = Convert(ColorInitialImage);
            }
        }
        #endregion

        #region Save processed image
        private ICommand _saveProcessedImageCommand;
        public ICommand SaveProcessedImageCommand
        {
            get
            {
                if (_saveProcessedImageCommand == null)
                    _saveProcessedImageCommand = new RelayCommand(SaveProcessedImage);
                return _saveProcessedImageCommand;
            }
        }

        private void SaveProcessedImage(object parameter)
        {
            if (GrayProcessedImage == null && ColorProcessedImage == null)
            {
                MessageBox.Show("If you want to save your processed image, " +
                    "please load and process an image first!");
                return;
            }

            string imagePath = SaveFileDialog("image.jpg");
            if (imagePath != null)
            {
                GrayProcessedImage?.Bitmap.Save(imagePath, GetJpegCodec("image/jpeg"), GetEncoderParameter(Encoder.Quality, 100));
                ColorProcessedImage?.Bitmap.Save(imagePath, GetJpegCodec("image/jpeg"), GetEncoderParameter(Encoder.Quality, 100));
                Process.Start(imagePath);
            }
        }
        #endregion

        #region Exit
        private ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new RelayCommand(Exit);
                return _exitCommand;
            }
        }

        private void Exit(object parameter)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #endregion

        #region Edit

        #region Remove drawn shapes from initial canvas
        private ICommand _removeInitialDrawnShapesCommand;
        public ICommand RemoveInitialDrawnShapesCommand
        {
            get
            {
                if (_removeInitialDrawnShapesCommand == null)
                    _removeInitialDrawnShapesCommand = new RelayCommand(RemoveInitialDrawnShapes);
                return _removeInitialDrawnShapesCommand;
            }
        }

        private void RemoveInitialDrawnShapes(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
        }
        #endregion

        #region Remove drawn shapes from processed canvas
        private ICommand _removeProcessedDrawnShapesCommand;
        public ICommand RemoveProcessedDrawnShapesCommand
        {
            get
            {
                if (_removeProcessedDrawnShapesCommand == null)
                    _removeProcessedDrawnShapesCommand = new RelayCommand(RemoveProcessedDrawnShapes);
                return _removeProcessedDrawnShapesCommand;
            }
        }

        private void RemoveProcessedDrawnShapes(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
        }
        #endregion

        #region Remove drawn shapes from both canvases
        private ICommand _removeDrawnShapesCommand;
        public ICommand RemoveDrawnShapesCommand
        {
            get
            {
                if (_removeDrawnShapesCommand == null)
                    _removeDrawnShapesCommand = new RelayCommand(RemoveDrawnShapes);
                return _removeDrawnShapesCommand;
            }
        }

        private void RemoveDrawnShapes(object parameter)
        {
            var canvases = (object[])parameter;
            RemoveUiElements(canvases[0] as Canvas);
            RemoveUiElements(canvases[1] as Canvas);
        }
        #endregion

        #region Clear initial canvas
        private ICommand _clearInitialCanvasCommand;
        public ICommand ClearInitialCanvasCommand
        {
            get
            {
                if (_clearInitialCanvasCommand == null)
                    _clearInitialCanvasCommand = new RelayCommand(ClearInitialCanvas);
                return _clearInitialCanvasCommand;
            }
        }

        private void ClearInitialCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            GrayInitialImage = null;
            ColorInitialImage = null;
            InitialImage = null;
        }
        #endregion

        #region Clear processed canvas
        private ICommand _clearProcessedCanvasCommand;
        public ICommand ClearProcessedCanvasCommand
        {
            get
            {
                if (_clearProcessedCanvasCommand == null)
                    _clearProcessedCanvasCommand = new RelayCommand(ClearProcessedCanvas);
                return _clearProcessedCanvasCommand;
            }
        }

        private void ClearProcessedCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            GrayProcessedImage = null;
            ColorProcessedImage = null;
            ProcessedImage = null;
        }
        #endregion

        #region Closing all open windows and clear both canvases
        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                    _clearCommand = new RelayCommand(Clear);
                return _clearCommand;
            }
        }

        private void Clear(object parameter)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window != Application.Current.MainWindow)
                {
                    window.Close();
                }
            }

            ScaleValue = 1;

            var canvases = (object[])parameter;
            ClearInitialCanvas(canvases[0] as Canvas);
            ClearProcessedCanvas(canvases[1] as Canvas);
        }
        #endregion

        #endregion

        #region Tools

        #region Magnifier
        private ICommand _magnifierCommand;
        public ICommand MagnifierCommand
        {
            get
            {
                if (_magnifierCommand == null)
                    _magnifierCommand = new RelayCommand(Magnifier);
                return _magnifierCommand;
            }
        }

        private void Magnifier(object parameter)
        {
            if (MagnifierOn == true) return;
            if (MouseClickCollection.Count == 0)
            {
                MessageBox.Show("Please select an area first!");
                return;
            }

            MagnifierWindow magnifierWindow = new MagnifierWindow();
            magnifierWindow.Show();
        }
        #endregion

        #region Visualize color levels

        #region Row color levels
        private ICommand _rowColorLevelsCommand;
        public ICommand RowColorLevelsCommand
        {
            get
            {
                if (_rowColorLevelsCommand == null)
                    _rowColorLevelsCommand = new RelayCommand(RowColorLevels);
                return _rowColorLevelsCommand;
            }
        }

        private void RowColorLevels(object parameter)
        {
            if (RowColorLevelsOn == true) return;
            if (MouseClickCollection.Count == 0)
            {
                MessageBox.Show("Please select an area first!");
                return;
            }

            ColorLevelsWindow window = new ColorLevelsWindow(_mainVM, CLevelsType.Row);
            window.Show();
        }
        #endregion

        #region Column color levels
        private ICommand _columnColorLevelsCommand;
        public ICommand ColumnColorLevelsCommand
        {
            get
            {
                if (_columnColorLevelsCommand == null)
                    _columnColorLevelsCommand = new RelayCommand(ColumnColorLevels);
                return _columnColorLevelsCommand;
            }
        }

        private void ColumnColorLevels(object parameter)
        {
            if (ColumnColorLevelsOn == true) return;
            if (MouseClickCollection.Count == 0)
            {
                MessageBox.Show("Please select an area first!");
                return;
            }

            ColorLevelsWindow window = new ColorLevelsWindow(_mainVM, CLevelsType.Column);
            window.Show();
        }
        #endregion

        #endregion

        #region Visualize image histogram

        #region Initial image histogram
        private ICommand _histogramInitialImageCommand;
        public ICommand HistogramInitialImageCommand
        {
            get
            {
                if (_histogramInitialImageCommand == null)
                    _histogramInitialImageCommand = new RelayCommand(HistogramInitialImage);
                return _histogramInitialImageCommand;
            }
        }

        private void HistogramInitialImage(object parameter)
        {
            if (InitialHistogramOn == true) return;
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            HistogramWindow window = null;

            if (ColorInitialImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.InitialColor);
            }
            else if (GrayInitialImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.InitialGray);
            }

            window.Show();
        }
        #endregion

        #region Processed image histogram
        private ICommand _histogramProcessedImageCommand;
        public ICommand HistogramProcessedImageCommand
        {
            get
            {
                if (_histogramProcessedImageCommand == null)
                    _histogramProcessedImageCommand = new RelayCommand(HistogramProcessedImage);
                return _histogramProcessedImageCommand;
            }
        }

        private void HistogramProcessedImage(object parameter)
        {
            if (ProcessedHistogramOn == true) return;
            if (ProcessedImage == null)
            {
                MessageBox.Show("Please process an image first!");
                return;
            }

            HistogramWindow window = null;

            if (ColorProcessedImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.ProcessedColor);
            }
            else if (GrayProcessedImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.ProcessedGray);
            }

            window.Show();
        }
        #endregion

        #endregion

        #region Copy image
        private ICommand _copyImageCommand;
        public ICommand CopyImageCommand
        {
            get
            {
                if (_copyImageCommand == null)
                    _copyImageCommand = new RelayCommand(CopyImage);
                return _copyImageCommand;
            }
        }

        private void CopyImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Copy(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
            else if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Copy(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
        }
        #endregion

        #region Invert image
        private ICommand _invertImageCommand;
        public ICommand InvertImageCommand
        {
            get
            {
                if (_invertImageCommand == null)
                    _invertImageCommand = new RelayCommand(InvertImage);
                return _invertImageCommand;
            }
        }

        private void InvertImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter as Canvas);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Invert(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Invert(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Convert color image to grayscale image
        private ICommand _convertImageToGrayscaleCommand;
        public ICommand ConvertImageToGrayscaleCommand
        {
            get
            {
                if (_convertImageToGrayscaleCommand == null)
                    _convertImageToGrayscaleCommand = new RelayCommand(ConvertImageToGrayscale);
                return _convertImageToGrayscaleCommand;
            }
        }

        private void ConvertImageToGrayscale(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                GrayProcessedImage = Tools.Convert(ColorInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only color images!");
            }
        }
        #endregion

        #endregion

        #region Pointwise operations

        #region Spline tool

        private ICommand _splineTool;
        public ICommand SplineTool
        {
            get
            {
                if (_splineTool == null)
                    _splineTool = new RelayCommand(splineTool);
                return _splineTool;
            }
        }

        private void splineTool(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            DrawingForm drawingForm = new DrawingForm();
            drawingForm.ShowDialog();
            var functionValues = drawingForm.GetPoints();

            PointCollection clickedPoints = new PointCollection();
            for (int i = 0; i < 256; i++)
                clickedPoints.Add(new System.Windows.Point(i, functionValues[i]));
            GraphWindow graphWindow = new GraphWindow(_mainVM, clickedPoints);
            graphWindow.ShowDialog();

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Algorithms.Sections.PointwiseOperations.applySpline(GrayInitialImage, functionValues);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                ColorProcessedImage = Algorithms.Sections.PointwiseOperations.applySpline(ColorInitialImage, functionValues);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }

        #endregion

        #endregion

        #region Thresholding

        #region Otsu 2 praguri

        private ICommand _thresholdingOtsu;
        public ICommand ThresholdingOtsu
        {
            get
            {
                if (_thresholdingOtsu == null)
                    _thresholdingOtsu = new RelayCommand(thresholdingOtsu);
                return _thresholdingOtsu;
            }
        }

        private void thresholdingOtsu(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Algorithms.Sections.Thresholding.OtsuTwoStep(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only grayscale images!");
            }
        }
        #endregion

        #region Step

        private ICommand _thresholdingStep;
        public ICommand ThresholdingStep
        {
            get
            {
                if (_thresholdingStep == null)
                    _thresholdingStep = new RelayCommand(thresholdingStep);
                return _thresholdingStep;
            }
        }

        private void thresholdingStep(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter);

            List<string> labels = new List<string>
                {
                    "Step: ",
                };
            DialogWindow dialog = new DialogWindow(_mainVM, labels);
            dialog.ShowDialog();
            List<double> values = dialog.GetValues();
            int step = (int)values[0];

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Algorithms.Sections.Thresholding.StepTresh(GrayInitialImage, step);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only grayscale images!");
            }
        }

        #endregion

        #endregion

        #region Filters

        #region Filtering Fast Median

        private ICommand _fastMedian;
        public ICommand FastMedian
        {
            get
            {
                if (_fastMedian == null)
                    _fastMedian = new RelayCommand(fastMedian);
                return _fastMedian;
            }
        }

        private void fastMedian(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }
            List<string> labels = new List<string>
            {
                "Raza masca pixeli: ",
            };
            DialogWindow dialog = new DialogWindow(_mainVM, labels);
            dialog.ShowDialog();
            List<double> values = dialog.GetValues();
            int radius = (int)values[0];

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Algorithms.Sections.FastMedian.fastMedianGray(GrayInitialImage, radius);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                ColorProcessedImage = Algorithms.Sections.FastMedian.fastMedianColor(ColorInitialImage, radius);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Canny

        private ICommand _canny;
        public ICommand Canny
        {
            get
            {
                if (_canny == null)
                    _canny = new RelayCommand(canny);
                return _canny;
            }
        }

        private void canny(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }


            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                List<string> labels = new List<string>
                {
                    "Prag T1: ",
                    "Prag T2: ",
                };
                DialogWindow dialog = new DialogWindow(_mainVM, labels);
                dialog.ShowDialog();
                List<double> values = dialog.GetValues();
                int T1 = (int)values[0];
                int T2 = (int)values[1];

                GrayProcessedImage = Algorithms.Sections.Canny.cannyGray(GrayInitialImage, T1, T2);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only grayscale images!");
            }
        }
        #endregion

        #endregion

        #region Morphological operations

        #region Componente conexe

        private ICommand _compConexe;
        public ICommand CompConexe
        {
            get
            {
                if (_compConexe == null)
                    _compConexe = new RelayCommand(compConexe);
                return _compConexe;
            }
        }

        private void compConexe(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                ColorProcessedImage = Algorithms.Sections.MorphologicalOperations.compConexe(GrayInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only grayscale images!");
            }
        }

        #endregion

        #endregion

        #region Geometric transformations

        #region Transformare Proiectiva

        
        private ICommand _transfPr;
        public ICommand TransfPr
        {
            get
            {
                if (_transfPr == null)
                    _transfPr = new RelayCommand(transfPr);
                return _transfPr;
            }
        }

        private void transfPr(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image!");
                return;
            }

            var canvases = (object[])parameter;
            var initialCanvas = canvases[0] as Canvas;
            var processedCanvas = canvases[1] as Canvas;

            var selectedRegion = new PointCollection();
            var resultRegion = new PointCollection();

            int noElems = MouseClickCollection.Count;
            if (noElems >= 6)
            {
                resultRegion.Add(MouseClickCollection.ElementAt(noElems - 2));
                resultRegion.Add(MouseClickCollection.ElementAt(noElems - 1));
                selectedRegion.Add(MouseClickCollection.ElementAt(noElems - 6));
                selectedRegion.Add(MouseClickCollection.ElementAt(noElems - 5));
                selectedRegion.Add(MouseClickCollection.ElementAt(noElems - 4));
                selectedRegion.Add(MouseClickCollection.ElementAt(noElems - 3));

                ClearProcessedCanvas(processedCanvas);

                DrawPolygon(initialCanvas, selectedRegion, 2, System.Windows.Media.Brushes.Red, ScaleValue);
                DrawRectangle(processedCanvas, resultRegion.ElementAt(0), resultRegion.ElementAt(1), 2, System.Windows.Media.Brushes.Red, ScaleValue);

                List<Tuple<int, int>> fromPx = new List<Tuple<int, int>>();
                fromPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 6).X, (int)MouseClickCollection.ElementAt(noElems - 6).Y));
                fromPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 5).X, (int)MouseClickCollection.ElementAt(noElems - 5).Y));
                fromPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 4).X, (int)MouseClickCollection.ElementAt(noElems - 4).Y));
                fromPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 3).X, (int)MouseClickCollection.ElementAt(noElems - 3).Y));
                List<Tuple<int, int>> toPx = new List<Tuple<int, int>>();
                toPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 2).X, (int)MouseClickCollection.ElementAt(noElems - 2).Y));
                toPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 1).X, (int)MouseClickCollection.ElementAt(noElems - 2).Y));
                toPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 1).X, (int)MouseClickCollection.ElementAt(noElems - 1).Y));
                toPx.Add(new Tuple<int, int>((int)MouseClickCollection.ElementAt(noElems - 2).X, (int)MouseClickCollection.ElementAt(noElems - 1).Y));

                if (GrayInitialImage != null)
                {
                    GrayProcessedImage = Algorithms.Sections.GeometricTransformations.transfPr(GrayInitialImage, fromPx, toPx);
                    ProcessedImage = Convert(GrayProcessedImage);
                }
                else
                {
                    ColorProcessedImage = Algorithms.Sections.GeometricTransformations.transfPr(ColorInitialImage, fromPx, toPx);
                    ProcessedImage = Convert(ColorProcessedImage);
                }
            }
            else
            {
                MessageBox.Show("Please select 4 clicks for the initial region and the 2 corners of the resulting rectangle");
            }


        }
        #endregion

        #endregion

        #region Segmentation
        #endregion

        #region Use processed image as initial image
        private ICommand _useProcessedImageAsInitialImageCommand;
        public ICommand UseProcessedImageAsInitialImageCommand
        {
            get
            {
                if (_useProcessedImageAsInitialImageCommand == null)
                    _useProcessedImageAsInitialImageCommand = new RelayCommand(UseProcessedImageAsInitialImage);
                return _useProcessedImageAsInitialImageCommand;
            }
        }

        private void UseProcessedImageAsInitialImage(object parameter)
        {
            if (ProcessedImage == null)
            {
                MessageBox.Show("Please process an image first!");
                return;
            }

            var canvases = (object[])parameter;

            ClearInitialCanvas(canvases[0] as Canvas);

            if (GrayProcessedImage != null)
            {
                GrayInitialImage = GrayProcessedImage;
                InitialImage = Convert(GrayInitialImage);
            }
            else if (ColorProcessedImage != null)
            {
                ColorInitialImage = ColorProcessedImage;
                InitialImage = Convert(ColorInitialImage);
            }

            ClearProcessedCanvas(canvases[1] as Canvas);
        }
        #endregion
    }
}

