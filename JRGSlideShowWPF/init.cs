
using System;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Interop;
using System.Threading;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;


namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        public void InitVars()
        {
            IsUserjgentile = String.Compare(Environment.UserName, "jgentile", true) == 0;
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            PSource = PresentationSource.FromVisual(this);
            thisHandle = new WindowInteropHelper(this).Handle;            
            progressBar.Visibility = Visibility.Hidden;
            InfoBlockControl.Visibility = Visibility.Hidden;
            MotdBlockControl.Visibility = Visibility.Hidden;

        }
        public async Task InitSlideShow()
        {
            if (string.IsNullOrEmpty(SlideShowDirectory) || !Directory.Exists(SlideShowDirectory))
            {
                await OpenDirCheckCancel();
            }
            else
            {
                await Task.Run(() => StartGetFiles());
                await DisplayGetNextImage(1);
            }            
        }
        public void InitMotd()
        {
            MotdClass = new TextBoxMessage()
            {
                textBlock = MotdBlockControl,
                dispatchTimer = new System.Windows.Threading.DispatcherTimer()
            };
            InfoTextBoxClass = new TextBoxMessage()
            {
                textBlock = InfoBlockControl,
                dispatchTimer = new System.Windows.Threading.DispatcherTimer()
            };
        }

        private bool ClosePrevious()
        {
            InitRNGKeys();
            Process[] processlist = Process.GetProcessesByName("JRGSlideShowWPF");
            var currentProcess = Process.GetCurrentProcess();
            int c = 0;
            foreach (Process theprocess in processlist)
            {
                if (theprocess.Id == currentProcess.Id)
                {
                    continue;
                }
                //theprocess.Close();
                c++;
            }
            if (c > 0)
            {
                if (Environment.CommandLine.Contains("/close"))
                {
                    return false;
                }
            }
            return true;
        }
    }
}