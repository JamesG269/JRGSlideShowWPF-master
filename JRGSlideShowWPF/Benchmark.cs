using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        public int benchmarkRunning = 0;
        public bool benchmarkStop = false;
        private void BenchMarkWorker()
        {            
            Stop();
            benchmarkStop = false;
            int imagesLimit = ImageList.Length;

            Stopwatch benchmark = new Stopwatch();
            ImageIdxListPtr = 0;
            imagesDisplayed = 0;
            var backuprandomize = RandomizeImages;
            if (RandomizeImages == true)
            {
                RandomizeImages = false;
                CreateIdxListCode();
            }
            if (ImageListReady == true)
            {
                benchmark.Start();
                while (imagesDisplayed < imagesLimit && benchmarkStop == false)
                {
                    LoadNextImage(1);
                    System.Windows.Application.Current.Dispatcher.InvokeAsync((new Action(async () =>
                    {
                        await DisplayCurrentImage();
                    })), System.Windows.Threading.DispatcherPriority.Background);
                }
                benchmark.Stop();
            }
            if (backuprandomize == true)
            {
                RandomizeImages = backuprandomize;
                CreateIdxListCode();
            }
            MessageBox.Show("Benchmark - Images displayed: " + imagesDisplayed + " Milliseconds: " + benchmark.ElapsedMilliseconds + " Ticks: " + benchmark.ElapsedTicks + " Images per second: " + imagesDisplayed / (benchmark.ElapsedMilliseconds / 1000));            
        }
    }
}