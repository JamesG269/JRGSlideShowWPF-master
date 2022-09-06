﻿using System;
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
        private async void ChangeTimerCode()
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            bool ShowPictureBackup = dispatcherPlaying.IsEnabled;
            Stop();
            SlideShowTimer SlideShowTimerWindow = new SlideShowTimer
            {
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
            };

            SlideShowTimerWindow.TimerTextBox.Text = dispatcherPlaying.Interval.Seconds.ToString();
            SlideShowTimerWindow.ShowDialog();

            int i = int.Parse(SlideShowTimerWindow.TimerTextBox.Text);
            dispatcherPlaying.Interval = new TimeSpan(0, 0, 0, i, 0);

            Activate();
            OneInt = 0;
            if (ShowPictureBackup == true)
            {
                Play();
            }
        }
    }
}