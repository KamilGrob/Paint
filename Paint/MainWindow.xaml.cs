using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Emgu.CV.Reg;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private bool isDrawing = false;
        private Polygon pol = new Polygon();
        private SolidColorBrush drawingBrush = new SolidColorBrush(Colors.Black);

        public MainWindow()
        {
            InitializeComponent();
            Resources.Add("UpdatedColor", new SolidColorBrush(Colors.Black));
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            isDrawing = true;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentMouseFunction == MouseFunction.DrawLine)
            {
                if (isDrawing)
                {
                    Line line = new Line
                    {
                        Stroke = drawingBrush,
                        X1 = startPoint.X,
                        Y1 = startPoint.Y,
                        X2 = e.GetPosition(canvas).X,
                        Y2 = e.GetPosition(canvas).Y
                    };

                    startPoint = e.GetPosition(canvas);
                    canvas.Children.Add(line);
                }
            }
            else if(currentMouseFunction == MouseFunction.Erase)
            {
                if (isDrawing)
                {
                    HitTestResult result = VisualTreeHelper.HitTest(canvas, e.GetPosition(canvas));
                    if (result != null && result.VisualHit is Shape)
                    {
                        canvas.Children.Remove((Shape)result.VisualHit);
                    }
                }
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            if(currentMouseFunction == MouseFunction.DrawPoint)
            {
                Line line = new Line
                {
                    Stroke = drawingBrush,
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = e.GetPosition(canvas).X,
                    Y2 = e.GetPosition(canvas).Y,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round
                };
                canvas.Children.Add(line);
            }
            else if (currentMouseFunction == MouseFunction.DrawRectangle)
            {

                Rectangle rect = new Rectangle
                {
                    Stroke = drawingBrush,
                    Width = Math.Abs(e.GetPosition(canvas).X - startPoint.X),
                    Height = Math.Abs(e.GetPosition(canvas).Y - startPoint.Y)
                };
                Canvas.SetLeft(rect, Math.Min(startPoint.X, e.GetPosition(canvas).X));
                Canvas.SetTop(rect, Math.Min(startPoint.Y, e.GetPosition(canvas).Y));
                canvas.Children.Add(rect);
            }
            else if (currentMouseFunction == MouseFunction.DrawPolygon)
            {
                if (pol.Points.Count <= 4)
                {
                    pol.Points.Add(startPoint);
                }
                if (pol.Points.Count > 4)
                {
                    pol.Points.Add(pol.Points[0]);
                    pol.Stroke = drawingBrush;
                    canvas.Children.Add(pol);
                    pol = new Polygon();
                }
            }
            else if (currentMouseFunction == MouseFunction.DrawElipse)
            {
                System.Windows.Shapes.Ellipse ell = new System.Windows.Shapes.Ellipse
                {
                    Stroke = drawingBrush,
                    Width = Math.Abs(e.GetPosition(canvas).X - startPoint.X),
                    Height = Math.Abs(e.GetPosition(canvas).Y - startPoint.Y)
                };
                Canvas.SetLeft(ell, Math.Min(startPoint.X, e.GetPosition(canvas).X));
                Canvas.SetTop(ell, Math.Min(startPoint.Y, e.GetPosition(canvas).Y));
                canvas.Children.Add(ell);
            }
        }

        private void Button_line(object sender, RoutedEventArgs e)
        {
            currentMouseFunction = MouseFunction.DrawLine;
        }

        private void Button_odcinek(object sender, RoutedEventArgs e)
        {
            currentMouseFunction = MouseFunction.DrawPoint;
  
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            currentMouseFunction = MouseFunction.Erase;
        }

        private MouseFunction currentMouseFunction = MouseFunction.DrawLine;

        private enum MouseFunction
        {
            DrawLine,
            DrawPoint,
            DrawSegment,
            DrawRectangle,
            DrawElipse,
            DrawPolygon,
            EditLine,
            AddImg,
            Erase
        }

        private void Button_rectangle(object sender, RoutedEventArgs e)
        {
            currentMouseFunction = MouseFunction.DrawRectangle;
        }

        private void Button_elipse(object sender, RoutedEventArgs e)
        {
            currentMouseFunction = MouseFunction.DrawElipse;
        }

        private void Button_polygon(object sender, RoutedEventArgs e)
        {
            currentMouseFunction = MouseFunction.DrawPolygon;
        }

        private void Button_color(object sender, RoutedEventArgs e)
        {
            // Tworzenie nowego okna edycji koloru
            ColorEditWindow colorEditWindow = new ColorEditWindow();

            // Przekazanie aktualnego koloru do okna edycji koloru
            colorEditWindow.SelectedColor = (canvas.Background as SolidColorBrush)?.Color ?? Colors.Black;

            // Pokaż nowe okno
            colorEditWindow.ShowDialog();

            // Pobierz aktualizowany kolor po zamknięciu okna
            Color updatedColor = colorEditWindow.SelectedColor;

            // Zaktualizuj zasób dla koloru
            Resources["UpdatedColor"] = new SolidColorBrush(updatedColor);

            // Ustaw nowy kolor w aplikacji
            SolidColorBrush brush = new SolidColorBrush(updatedColor);

            // Ustaw kolor brush dla rysowania
            drawingBrush = brush;
        }

        private void Button_Addimg(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(openFileDialog.FileName))
                };
                Point position = Mouse.GetPosition(canvas);
                
                canvas.Children.Add(image);

            }
        }
        private void ApplyFilter()
        {
            if (canvas.Children.Count > 0 && canvas.Children[0] is Image image)
            {
            
                WriteableBitmap bitmap = new WriteableBitmap(image.Source as BitmapSource);

              
                int[,] filterMatrix = new int[,] {
                    { -1, -1, -1 },
                    { -1,  9, -1 },
                    { -1, -1, -1 }
                };

                int filterSize = 3; 

              
                int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
                int stride = bytesPerPixel * bitmap.PixelWidth;
                byte[] pixelData = new byte[stride * bitmap.PixelHeight];

                bitmap.CopyPixels(pixelData, stride, 0);

                for (int y = 1; y < bitmap.PixelHeight - 1; y++)
                {
                    for (int x = 1; x < bitmap.PixelWidth - 1; x++)
                    {
                        ApplyFilterToPoint(pixelData, stride, x, y, filterMatrix, filterSize);
                    }
                }

                bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixelData, stride, 0);

         
                image.Source = bitmap;
            }
        }

        private void ApplyFilterToPoint(byte[] pixelData, int stride, int x, int y, int[,] filterMatrix, int filterSize)
        {
            int bytesPerPixel = stride / pixelData.Length;

            int red = 0, green = 0, blue = 0;

            for (int i = 0; i < filterSize; i++)
            {
                for (int j = 0; j < filterSize; j++)
                {
                    int pixelIndex = (y + j - 1) * stride + (x + i - 1) * bytesPerPixel;

                    blue += pixelData[pixelIndex];
                    green += pixelData[pixelIndex + 1];
                    red += pixelData[pixelIndex + 2];
                }
            }

            red = Math.Min(Math.Max(red, 0), 255);
            green = Math.Min(Math.Max(green, 0), 255);
            blue = Math.Min(Math.Max(blue, 0), 255);

            int currentIndex = y * stride + x * bytesPerPixel;

            pixelData[currentIndex] = (byte)blue;
            pixelData[currentIndex + 1] = (byte)green;
            pixelData[currentIndex + 2] = (byte)red;
        }


        private void Button_ApplyFilter(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(canvas);

                BitmapEncoder encoder = (filePath.EndsWith(".png")) ? new PngBitmapEncoder() : (BitmapEncoder)new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                using (FileStream file = File.Create(filePath))
                {
                    encoder.Save(file);
                }
            }
        }
    }
}