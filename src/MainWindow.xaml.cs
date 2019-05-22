using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace GameOfLifeCSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //static int height = 200;
        //static int width = 320;
        static readonly int height = 1000;
        static readonly int width = 1000;
        byte[] frontBuffer = new byte[height * width];
        byte[] backBuffer = new byte[height * width];
        bool running = false;
        DispatcherTimer timer = new DispatcherTimer();
        BitmapSource bmp;
        Random random = new Random();
        byte[] neighbourFrontBuffer = new byte[height * width];
        byte[] neighbourBackBuffer = new byte[height * width];
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            frontBuffer[0] = 200;
            frontBuffer[1] = 20;
            frontBuffer[2] = 20;
            frontBuffer[3] = 20;
            frontBuffer[4] = 20;
            frontBuffer[5] = 20;
            frontBuffer[6] = 20;
            frontBuffer[7] = 20;
            frontBuffer[8] = 20;
            frontBuffer[9] = 20;
            InitialzeNeighbours();
            timer.Interval = new TimeSpan(1);
            timer.Tick += new EventHandler(Timer_Tick);
            DrawBmp();
        }

        private void DrawBmp()
        {
            bmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Halftone256, frontBuffer, width);
            ImageDrawing image = new ImageDrawing(bmp, new Rect(0, 0, bmp.Height, bmp.Width));
            //this.Background = new DrawingBrush(image);
            //this.InvalidateVisual();
            rect.Fill = new DrawingBrush(image);
        }

        /// <summary>
        /// basic game of life implementation that simply apples the rules to generate the next generation to the backbuffer and swaps it with the frontbuffer
        /// </summary>
        private void GetNextGeneration()
        {
            int neighbours;
            backBuffer = new byte[height * width];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    neighbours = NeighbourCheck(column, row);
                    if (frontBuffer[row * width + column] > 0)
                    {
                        if (!((neighbours == 3) || (neighbours == 4)))
                            backBuffer[row * width + column] = 0;
                        else
                            backBuffer[row * width + column] = 55;
                    }
                    else if (neighbours == 3)
                        backBuffer[row * width + column] = 55;
                }
            }

            
            SwapBuffers();
            for (int i = 0; i < width * height; i++)
                neighbourFrontBuffer[i] = neighbourBackBuffer[i];
        }

        /// <summary>
        /// Optimized game of life implementation that caches the sums of neighbours for every cell and updates this information when cells are updated.
        /// Works better than the simple implementation when there are not many changes in a large field.
        /// </summary>
        private void GetNextGenerationFast()
        {
            backBuffer = new byte[height * width];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int position = row * width + column;
                    if (frontBuffer[position] > 0)
                    {
                        if (!((neighbourFrontBuffer[position] == 3) || (neighbourFrontBuffer[position] == 4)))
                        {
                            backBuffer[position] = 0;
                            //substracts 1 from the neighbourcount of all surrounding cells
                            for (int changeRow = row - 1; changeRow < row + 2; changeRow++)
                                for (int changeColumn = column - 1; changeColumn < column + 2; changeColumn++)
                                    neighbourBackBuffer[((changeRow + height) % height) * width + (changeColumn + width) % width] -= 1;
                        }
                        else
                            backBuffer[position] = 55;
                    }
                    else if (neighbourFrontBuffer[position] == 3)
                    {
                        backBuffer[position] = 55;
                        //adds 1 to the neighbourcount of all surrounding cells
                        for (int changeRow = row - 1; changeRow < row + 2; changeRow++)
                            for (int changeColumn = column - 1; changeColumn < column + 2; changeColumn++)
                                neighbourBackBuffer[((changeRow + height) % height) * width + (changeColumn + width) % width] += 1;
                    }
                }
            }
            SwapBuffers();
        }

        private void InitialzeNeighbours()
        {
            neighbourFrontBuffer = new byte[height * width];
            neighbourBackBuffer = new byte[height * width];
            int test2 = NeighbourCheck(0, 0);
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int position = row * width + column;
                    int test = NeighbourCheck(column, row);
                    neighbourFrontBuffer[position] = Convert.ToByte(NeighbourCheck(column, row));
                    neighbourBackBuffer[position] = neighbourFrontBuffer[position];
                }
            }
        }

        private void SwapBuffers()
        {
            byte[] temp;
            temp = frontBuffer;
            frontBuffer = backBuffer;
            backBuffer = temp;

            for (int i = 0; i < width * height; i++)
                neighbourFrontBuffer[i] = neighbourBackBuffer[i];
        }

        private int NeighbourCheck(int column, int row)
        {
            int neighbours = 0;
            for (int cellCheckColumn = column - 1; cellCheckColumn < column + 2; cellCheckColumn++)
                for (int cellCheckRow = row - 1; cellCheckRow < row + 2; cellCheckRow++)
                {
                    if (frontBuffer[((cellCheckRow + height) % height) * width + (cellCheckColumn + width) % width] > 0)
                        neighbours++;
                }
            return neighbours;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!running)
            {
                timer.Start();
                this.btnStart.Content = "Stop";
            }
            else
            {
                timer.Stop();
                this.btnStart.Content = "Start";
            }
            running = !running;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((bool)cbFast.IsChecked)
                GetNextGenerationFast();
            else
                GetNextGeneration();
            DrawBmp();
            lblCount.Content = Convert.ToString((Convert.ToInt32(lblCount.Content) + 1));
        }

        private void btnSv_Click(object sender, RoutedEventArgs e)
        {
            SavePicture(bmp);
        }

        private void SavePicture(BitmapSource bmp)
        {
            FileStream filestream = new FileStream(@"C:\Users\schoenfeld\Pictures\picture.png", FileMode.Create);
            PngBitmapEncoder bmpEncoder = new PngBitmapEncoder();
            bmpEncoder.Frames.Add(BitmapFrame.Create(bmp));
            bmpEncoder.Save(filestream);
            filestream.Close();
        }

        private void btnRdm_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < height; i++)
                for (int k = 0; k < width; k++)
                    if (random.Next(25) == 1)
                        frontBuffer[i * width + k] = 55;
            InitialzeNeighbours();
            DrawBmp();
            lblCount.Content = "0";
            //frontBuffer = new byte[width * height];
            //for (int count = 0; count < random.Next(width * height / 2, width * height / 4 * 3); count++)
            //    frontBuffer[random.Next(width * height)] = 55;
            //InitialzeNeighbours();
            //DrawBmp();
        }

        private void btnFwd_Click(object sender, RoutedEventArgs e)
        {
            GetNextGenerationFast();
            DrawBmp();
            lblCount.Content = Convert.ToString((Convert.ToInt32(lblCount.Content) + 1));
        }

        private void cbFast_Checked(object sender, RoutedEventArgs e)
        {
            InitialzeNeighbours();
        }
    }

    
}
