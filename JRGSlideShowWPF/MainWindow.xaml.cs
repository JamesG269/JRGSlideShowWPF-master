/*
JRGSlideShowWPF is a slide show program written in C# targetting WPF by James Gentile (jamesraymondgentile@gmail.com), that runs on Windows.
I wrote this app because other slide show software I tried was lacking in several key areas,
I wanted the monitor not to sleep (this is like 1 line of code, I can't understand why 
this is not an option in other slide show apps) and I wanted the ability to delete the
currently displayed picture which I couldn't find in the others I tried. It is simple to use:
open the app, select a folder, and it starts playing. 

Functions:
F1 Key - Picture and technical info.
Del key - prompt to delete current picture.
Double click window - maximize or de-maximize.
Right click window - options such as open folder, randomize/sequential play, change timer, etc.
Mouse wheel up/down - scroll through picture list.
Screen will not sleep if the app is full screen.
Dedicated to Caroline Bejoc from Sigonella and Marcy Magee from Swansboro NC.
*/

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
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        const string version = "3.0";

        public System.Windows.Threading.DispatcherTimer dispatcherPlaying;
        public System.Windows.Threading.DispatcherTimer dispatcherHideCursor;
        public System.Windows.Threading.DispatcherTimer dispatcherTextBoxMessage;

        string SlideShowDirectory;

        Boolean RandomizeImages = true;
        Boolean ImageReadyToDisplay = false;

        int ImagesNotNull = 0;
        int OneInt = 0;
        int imagesDisplayed = 0;

        FolderBrowserDialog dialog = new FolderBrowserDialog();

        IntPtr thisHandle = IntPtr.Zero;

        PresentationSource PSource = null;
     
        TextBoxMessage MotdClass;
        TextBoxMessage InfoTextBoxClass;
        bool Starting = false;
        bool IsUserjgentile = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public async void Window_ContentRendered(object sender, EventArgs e)
        {
            Starting = true;
            bool result = await Task.Run(() => InitAndClosePrevious());
            if (result == false)
            {
                Close();
            }
            InitMotd();
            InitVars();                       
            LoadSettings();
            NotifyStart();
            await InitSlideShow();            
            Starting = false;
            await DisplayGetNextImageWithoutCheck(1);
            Play(true);
            MouseInitTimer();
            EnableMotd();
        }
        
        public bool ShowPicture = false;
        private void Stop()
        {
            dispatcherPlaying.Stop();
            SetDisplayMode();                      
        }
        
        private void Play(bool PlayNow = false)
        {
            if (ImageListReady == false)
            {
                Stop();
            }
            else
            {
                dispatcherPlaying.Start();                                               
            }
            SetDisplayMode();
        }
        
        protected override async void OnClosing(CancelEventArgs e)
        {
            _cancelTokenSource.Cancel();
            Stop();
            if (NIcon != null)
            {
                NIcon.Dispose();
                NIcon = null;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            SaveSettings();
            base.OnClosing(e);
        }

        private void EnableMotd(object sender, RoutedEventArgs e)
        {
            ShowMotd = MotdXaml.IsChecked;
            if (MotdXaml.IsChecked == true)
            {
                EnableMotd();               
            }
            else
            {
                MotdClass.messageDisplayEndUninterruptable(new Action(() => { MotdClass.textBlock.Visibility = Visibility.Hidden; }));
            }
        }
    }
}
