using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using Shell32;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private void ChangeTimerCode()
        {            
            bool ShowPictureBackup = dispatcherPlaying.IsEnabled;
            Stop();
            SlideShowTimer SlideShowTimerWindow = new SlideShowTimer
            {
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
            };

            SlideShowTimerWindow.TimerTextBox.Text = TimerSeconds.ToString();
            SlideShowTimerWindow.ShowDialog();

            TimerSeconds = int.Parse(SlideShowTimerWindow.TimerTextBox.Text);
            
            dispatcherPlaying.Interval = new TimeSpan(0, 0, 0, TimerSeconds, 0);

            Activate();            
            if (ShowPictureBackup == true)
            {
                Play();
            }
        }
    }
}