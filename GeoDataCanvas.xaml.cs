using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeoDataCanvasControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GeoDataCanvas : UserControl
    {
        public int GridStepInMeters { get; private set; }

        public GeoDataCanvas()
        {
            InitializeComponent();
        }


        public async Task PlotPointsWhereMeanIsAtCenterAndAxisAsync(IEnumerable<GeoCoordinate> coordinates, AxisOriginPoint axisOriginPoint)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var tempCoordinates = new List<GeoCoordinate>();
            tempCoordinates.AddRange(coordinates);

            for (int howManyPointsToDraw = tempCoordinates.Count; howManyPointsToDraw <= tempCoordinates.Count; howManyPointsToDraw++)
            {
                Canvas_Target.Children.Clear();


                #region draw background
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.White);
                rect.Fill = new SolidColorBrush(Colors.White);
                rect.Width = Canvas_Target.Width + 25;
                rect.Height = Canvas_Target.Height + 20;
                Canvas.SetLeft(rect, 0);
                Canvas.SetTop(rect, 0);
                Canvas_Target.Children.Add(rect);
                #endregion


                coordinates = tempCoordinates.Take(howManyPointsToDraw);

                double averageLatitude = coordinates.Average(c => c.Latitude);
                double averageLongtitude = coordinates.Average(c => c.Longitude);

                GeoCoordinate averageCoordinate = new GeoCoordinate(averageLatitude, averageLongtitude);
                double distanceStandartDeviationInMeters = Statistics.StandardDeviation(coordinates.Select(c => c.GetDistanceTo(averageCoordinate)));

                #region Finding Min\Max Lon\Lat     and scales
                double minLongtitude = double.MaxValue;
                double maxLongtitude = double.MinValue;
                double minLatitude = double.MaxValue;
                double maxLatitude = double.MinValue;

                foreach (var coord in coordinates)
                {
                    if (coord.Longitude < minLongtitude)
                        minLongtitude = coord.Longitude;

                    if (coord.Longitude > maxLongtitude)
                        maxLongtitude = coord.Longitude;

                    if (coord.Latitude < minLatitude)
                        minLatitude = coord.Latitude;

                    if (coord.Latitude > maxLatitude)
                        maxLatitude = coord.Latitude;
                }

                var scaleLon = (Canvas_Target.ActualWidth) / (maxLongtitude - minLongtitude);
                var scaleLat = (Canvas_Target.ActualHeight) / (maxLatitude - minLatitude);
                #endregion






                #region correcting Min\Max Lon\Lat     and scales      (making same distances to all sides relative to average point)
                #region finding biggest distances
                double latitudeDistanceBetweenLeftmostAndAverage = Math.Abs(averageLatitude - minLatitude);
                double latitudeDistanceBetweenRightmostAndAverage = Math.Abs(averageLatitude - maxLatitude);
                double biggestLatitudeDifference = Math.Max(latitudeDistanceBetweenLeftmostAndAverage, latitudeDistanceBetweenRightmostAndAverage);

                double longtitudeDistanceBetweenTopmostAndAverage = Math.Abs(averageLongtitude - minLongtitude);
                double longtitudeDistanceBetweenBottommostAndAverage = Math.Abs(averageLongtitude - maxLongtitude);
                double biggestLongtitudeDifference = Math.Max(longtitudeDistanceBetweenTopmostAndAverage, longtitudeDistanceBetweenBottommostAndAverage);
                #endregion

                #region correcting min max lon lat      and scales
                minLongtitude = averageLongtitude - biggestLongtitudeDifference;
                maxLongtitude = averageLongtitude + biggestLongtitudeDifference;

                minLatitude = averageLatitude - biggestLatitudeDifference;
                maxLatitude = averageLatitude + biggestLatitudeDifference;


                scaleLon = (Canvas_Target.ActualWidth) / (maxLongtitude - minLongtitude);
                scaleLat = (Canvas_Target.ActualHeight) / (maxLatitude - minLatitude);
                #endregion
                #endregion








                #region Plot center
                //double centerLonDiff = averageCoordinate.Longitude - minLongtitude;
                //double centerLatDiff = averageCoordinate.Latitude - minLatitude;
                double centerLonDiff = coordinates.Average(c => c.Longitude) - minLongtitude;
                double centerLatDiff = coordinates.Average(c => c.Latitude) - minLatitude;

                double Xc = (centerLonDiff * scaleLon);
                double Yc = (centerLatDiff * scaleLat);

                double centerDiameter = 6;

                Ellipse ellC = new Ellipse() { Width = centerDiameter, Height = centerDiameter, Fill = System.Windows.Media.Brushes.Red };

                Canvas.SetLeft(ellC, Xc - centerDiameter / 2);
                Canvas.SetTop(ellC, Yc - centerDiameter / 2);

                Canvas_Target.Children.Add(ellC);
                #endregion

                #region Plotting points
                foreach (var coord in coordinates)
                {
                    double lonDiff = coord.Longitude - minLongtitude;
                    double latDiff = coord.Latitude - minLatitude;

                    double X = (lonDiff * scaleLon);
                    double Y = (latDiff * scaleLat);

                    double pointDiameter = 3;

                    Ellipse ell = new Ellipse() { Width = pointDiameter, Height = pointDiameter, Fill = System.Windows.Media.Brushes.Black };

                    Canvas.SetLeft(ell, X - pointDiameter / 2);
                    Canvas.SetTop(ell, Y - pointDiameter / 2);

                    //await Task.Delay(20);

                    Canvas_Target.Children.Add(ell);
                }
                #endregion

                #region Drawing standart deviation circles
                #region Finding canvas coordinate of farthest point from center
                var farthestFromAverage = coordinates
                                    .Select(c => new
                                    {
                                        GeoCoordinate = c,
                                        DistanceFromAverage = averageCoordinate.GetDistanceTo(c)
                                    })
                                    .OrderByDescending(t => t.DistanceFromAverage)
                                    .First();

                double lonDiffOfFarthest = farthestFromAverage.GeoCoordinate.Longitude - minLongtitude;
                double latDiffOfFarthest = farthestFromAverage.GeoCoordinate.Latitude - minLatitude;

                double xOfFarthest = (lonDiffOfFarthest * scaleLon);
                double yOfFarthest = (latDiffOfFarthest * scaleLat);


                //Ellipse ell2 = new Ellipse() { Width = 2, Height = 2, Fill = System.Windows.Media.Brushes.Red };

                //Canvas.SetLeft(ell2, xOfFarthest);
                //Canvas.SetTop(ell2, yOfFarthest);

                //Canvas_Target.Children.Add(ell2); 
                #endregion

                double distanceBetwenAverageAndFarthestInPixels = GetDistanceBetweenPoints(
                    new PointF((float)Xc, (float)Yc),
                    new PointF((float)xOfFarthest, (float)yOfFarthest));

                double metersPerPixel = farthestFromAverage.DistanceFromAverage / distanceBetwenAverageAndFarthestInPixels;

                double pixelsStandardDeviation = distanceStandartDeviationInMeters / metersPerPixel;

                //#region SD as diameter
                //#region One SD
                //Ellipse standardDeviationEllipse1 = new Ellipse() { Width = pixelsStandardDeviation, Height = pixelsStandardDeviation, Stroke = System.Windows.Media.Brushes.Green, StrokeThickness = 1 };
                //Canvas.SetLeft(standardDeviationEllipse1, Xc - pixelsStandardDeviation / 2);
                //Canvas.SetTop(standardDeviationEllipse1, Yc - pixelsStandardDeviation / 2);
                //Canvas_Target.Children.Add(standardDeviationEllipse1);
                //#endregion

                //#region Two SD
                //Ellipse standardDeviationEllipse2 = new Ellipse() { Width = pixelsStandardDeviation * 2, Height = pixelsStandardDeviation * 2, Stroke = System.Windows.Media.Brushes.Green, StrokeThickness = 1 };
                //Canvas.SetLeft(standardDeviationEllipse2, Xc - pixelsStandardDeviation);
                //Canvas.SetTop(standardDeviationEllipse2, Yc - pixelsStandardDeviation);
                //Canvas_Target.Children.Add(standardDeviationEllipse2);
                //#endregion 
                //#endregion




                #region SD as radius
                #region One SD
                Ellipse standardDeviationEllipse1 = new Ellipse() { Width = pixelsStandardDeviation * 2, Height = pixelsStandardDeviation * 2, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 1.3 };
                Canvas.SetLeft(standardDeviationEllipse1, Xc - pixelsStandardDeviation);
                Canvas.SetTop(standardDeviationEllipse1, Yc - pixelsStandardDeviation);
                Canvas_Target.Children.Add(standardDeviationEllipse1);
                #endregion

                #region Two SD
                Ellipse standardDeviationEllipse2 = new Ellipse() { Width = pixelsStandardDeviation * 4, Height = pixelsStandardDeviation * 4, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 1.3 };
                Canvas.SetLeft(standardDeviationEllipse2, Xc - pixelsStandardDeviation * 2);
                Canvas.SetTop(standardDeviationEllipse2, Yc - pixelsStandardDeviation * 2);
                Canvas_Target.Children.Add(standardDeviationEllipse2);
                #endregion
                #endregion
                #endregion





                switch (axisOriginPoint)
                {
                    case AxisOriginPoint.Center: PlotGridAxisInCenter(metersPerPixel); break;
                    case AxisOriginPoint.BottomLeft: PlotGridAxisOnLeftAndBottom(metersPerPixel); break;
                }


                await Task.Delay(20);
            }

            

            watch.Stop();
            Debug.WriteLine($"Drawing points took {watch.ElapsedMilliseconds} ms");
        }

        public void Clear()
        {
            Canvas_Target.Children.Clear();
        }




        private void PlotGridAxisInCenter(double metersPerPixel)
        {
            double pixelsPerMeter = 1 / metersPerPixel;

            //var test = CaltulateGridStepInMeters(metersPerPixel);

            #region Vertical lines and labels
            int howManyMetersToLeftAndRightFromCenter = (int)Math.Floor(Canvas_Target.ActualWidth / 2 / pixelsPerMeter);

            int gridStepInMeters = CaltulateGridStepInMeters(metersPerPixel, ref howManyMetersToLeftAndRightFromCenter);

            //int currentMeter = -howManyMetersToLeftAndRightFromCenter;

            int howManyMeters = -howManyMetersToLeftAndRightFromCenter;
            for (double shift = -howManyMetersToLeftAndRightFromCenter * pixelsPerMeter;
                shift <= howManyMetersToLeftAndRightFromCenter * pixelsPerMeter;
                shift += gridStepInMeters * pixelsPerMeter, howManyMeters += gridStepInMeters)
            {
                #region Line
                Line verticalLine = new Line();
                verticalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                verticalLine.X1 = Canvas_Target.ActualWidth / 2 + shift;
                verticalLine.X2 = Canvas_Target.ActualWidth / 2 + shift;
                verticalLine.Y1 = 0;
                verticalLine.Y2 = Canvas_Target.ActualHeight;

                verticalLine.StrokeThickness = 1;
                Canvas_Target.Children.Add(verticalLine);
                #endregion



                #region Text
                //int howManyMeters = (int)Math.Ceiling(shift / pixelsPerMeter);

                TextBlock textBlock = new TextBlock();

                textBlock.Text = howManyMeters.ToString();

                textBlock.Foreground = System.Windows.Media.Brushes.Red;

                Canvas.SetLeft(textBlock, Canvas_Target.ActualWidth / 2 + shift);

                Canvas.SetTop(textBlock, Canvas_Target.ActualHeight / 2);

                Canvas_Target.Children.Add(textBlock);
                #endregion
            }
            #endregion

            #region Horizontal lines and labels
            int howManyMetersToTopAndBottomFromCenter = (int)Math.Floor(Canvas_Target.ActualHeight / 2 / pixelsPerMeter);
            howManyMeters = -howManyMetersToTopAndBottomFromCenter;
            for (double shift = -howManyMetersToTopAndBottomFromCenter * pixelsPerMeter; shift <= howManyMetersToTopAndBottomFromCenter * pixelsPerMeter; shift += gridStepInMeters * pixelsPerMeter, howManyMeters += gridStepInMeters)
            {
                #region Line
                Line horizontalLine = new Line();
                horizontalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                horizontalLine.X1 = 0;
                horizontalLine.X2 = Canvas_Target.ActualWidth;
                horizontalLine.Y1 = Canvas_Target.ActualHeight / 2 + shift;
                horizontalLine.Y2 = Canvas_Target.ActualHeight / 2 + shift;

                horizontalLine.StrokeThickness = 1;
                Canvas_Target.Children.Add(horizontalLine);
                #endregion




                #region Text
                //int howManyMeters = -(int)Math.Ceiling(shift / pixelsPerMeter);

                if (howManyMeters == 0) continue;

                TextBlock textBlock = new TextBlock();

                textBlock.Text = howManyMeters.ToString();

                textBlock.Foreground = System.Windows.Media.Brushes.Red;

                Canvas.SetLeft(textBlock, Canvas_Target.ActualWidth / 2);

                Canvas.SetTop(textBlock, Canvas_Target.ActualHeight / 2 + shift);

                Canvas_Target.Children.Add(textBlock);
                #endregion
            }
            #endregion






            //Line centralVerticalLine = new Line();
            //centralVerticalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

            //centralVerticalLine.X1 = Canvas_Target.ActualWidth / 2;
            //centralVerticalLine.X2 = Canvas_Target.ActualWidth / 2;
            //centralVerticalLine.Y1 = 0;
            //centralVerticalLine.Y2 = Canvas_Target.ActualHeight;

            //centralVerticalLine.StrokeThickness = 1;
            //Canvas_Target.Children.Add(centralVerticalLine);
        }


        private void PlotGridAxisOnLeftAndBottom(double metersPerPixel)
        {
            double pixelsPerMeter = 1 / metersPerPixel;

            #region Vertical lines and labels
            //int howManyMetersToLeftAndRightFromCenter = (int)Math.Floor(Canvas_Target.ActualWidth / 2 / pixelsPerMeter);

            //int currentMeter = -howManyMetersToLeftAndRightFromCenter;

            int maxMeters = (int)Math.Floor(Canvas_Target.ActualWidth / pixelsPerMeter);
            int gridStepInMeters = CaltulateGridStepInMeters(metersPerPixel, ref maxMeters);

            int howManyMeters = 0;
            for (double shiftFromLeft = 0;
                shiftFromLeft <= Canvas_Target.ActualWidth;
                shiftFromLeft += pixelsPerMeter * gridStepInMeters, howManyMeters += gridStepInMeters)
            {
                #region Line
                Line verticalLine = new Line();
                verticalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                verticalLine.X1 = shiftFromLeft;
                verticalLine.X2 = shiftFromLeft;
                verticalLine.Y1 = 0;
                verticalLine.Y2 = Canvas_Target.ActualHeight;

                verticalLine.StrokeThickness = 1;
                Canvas_Target.Children.Add(verticalLine);
                #endregion



                #region Text
                //int howManyMeters = (int)Math.Ceiling(shift / pixelsPerMeter);

                TextBlock textBlock = new TextBlock();

                textBlock.Text = howManyMeters.ToString();

                textBlock.Foreground = System.Windows.Media.Brushes.Red;

                Canvas.SetLeft(textBlock, shiftFromLeft);

                Canvas.SetTop(textBlock, Canvas_Target.ActualHeight);

                Canvas_Target.Children.Add(textBlock);
                #endregion
            }
            #endregion

            #region Horizontal lines and labels
            //int howManyMetersToTopAndBottomFromCenter = (int)Math.Floor(Canvas_Target.ActualHeight / pixelsPerMeter);
            howManyMeters = 0;
            for (double shiftFromBottom = 0;
                shiftFromBottom <= Canvas_Target.ActualHeight;
                shiftFromBottom += pixelsPerMeter * gridStepInMeters, howManyMeters += gridStepInMeters)
            {
                #region Line
                Line horizontalLine = new Line();
                horizontalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                horizontalLine.X1 = 0;
                horizontalLine.X2 = Canvas_Target.ActualWidth;
                horizontalLine.Y1 = Canvas_Target.ActualHeight - shiftFromBottom;
                horizontalLine.Y2 = Canvas_Target.ActualHeight - shiftFromBottom;

                horizontalLine.StrokeThickness = 1;
                Canvas_Target.Children.Add(horizontalLine);
                #endregion




                #region Text
                //int howManyMeters = -(int)Math.Ceiling(shift / pixelsPerMeter);

                if (howManyMeters == 0) continue;

                TextBlock textBlock = new TextBlock();

                textBlock.Text = howManyMeters.ToString();

                textBlock.Foreground = System.Windows.Media.Brushes.Red;

                Canvas.SetLeft(textBlock, 0);

                Canvas.SetTop(textBlock, Canvas_Target.ActualHeight - shiftFromBottom);

                Canvas_Target.Children.Add(textBlock);
                #endregion
            }
            #endregion






            //Line centralVerticalLine = new Line();
            //centralVerticalLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

            //centralVerticalLine.X1 = Canvas_Target.ActualWidth / 2;
            //centralVerticalLine.X2 = Canvas_Target.ActualWidth / 2;
            //centralVerticalLine.Y1 = 0;
            //centralVerticalLine.Y2 = Canvas_Target.ActualHeight;

            //centralVerticalLine.StrokeThickness = 1;
            //Canvas_Target.Children.Add(centralVerticalLine);
        }

        private int CaltulateGridStepInMeters(double metersPerPixel, ref int maxMetersLabel)
        {
            //return 6;
            double pixelsPerMeter = 1 / metersPerPixel;

            double defaultFontSize = new TextBlock().FontSize;

            if(pixelsPerMeter < defaultFontSize * 1.2)
            {
                double howManyLabelsPerDimension = Canvas_Target.ActualWidth / (defaultFontSize * 1.2);

                int startingGridStepInMeters = (int)Math.Ceiling((defaultFontSize * 1.5) / pixelsPerMeter);
                int finalGridStepInMeters = -1;

                //int labelsPerSide = (int)Math.Ceiling(howManyLabelsPerDimension / 2);

                while (true)
                {
                    finalGridStepInMeters = startingGridStepInMeters;

                    while (maxMetersLabel % finalGridStepInMeters != 0)
                        finalGridStepInMeters++;

                    if (finalGridStepInMeters == maxMetersLabel)
                        maxMetersLabel--;
                    else
                        break;
                }
                

                GridStepInMeters = finalGridStepInMeters;

                return finalGridStepInMeters;
            }

            GridStepInMeters = 1;

            return 1;
        }

        private static double GetDistanceBetweenPoints(PointF point1, PointF point2)
        {
            //pythagorean theorem c^2 = a^2 + b^2
            //thus c = square root(a^2 + b^2)
            double a = (double)(point2.X - point1.X);
            double b = (double)(point2.Y - point1.Y);

            return Math.Sqrt(a * a + b * b);
        }


        public enum AxisOriginPoint
        {
            Center,
            BottomLeft
        }
    }
}
