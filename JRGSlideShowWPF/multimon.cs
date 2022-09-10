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
using GoogleCast.Models.Media;
using GoogleCast.Channels;
using GoogleCast;


namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        public void GetMonitors()
        {
            MonitorMenu.Items.Clear();
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem
                {
                    Header = screen.DeviceName
                };
                item.Click += new RoutedEventHandler(MonitorClick);                
                MonitorMenu.Items.Add(item);
            }
        }
        public void MonitorClick(Object sender, RoutedEventArgs e)
        {
            string DeviceName = ((System.Windows.Controls.MenuItem)sender).Header.ToString();
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.DeviceName == DeviceName)
                {
                    GoFullScreenWorker(screen);
                }    
            }
            
        }
    }
}
