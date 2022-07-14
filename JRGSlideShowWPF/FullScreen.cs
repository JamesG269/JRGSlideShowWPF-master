using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {        
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            WindowStateCode();            
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {            
            base.OnRenderSizeChanged(sizeInfo);
            WindowStateCode();            
        }        
        private void WindowStateCode()
        {                        
            switch (WindowState)
            {                                
                case WindowState.Maximized:                    
                    GoFullScreen();                                 
                    return;
                case WindowState.Normal:                    
                    LeaveFullScreen(true);                    
                    return;
            }                    
        }
        int oldHeight = 0;
        int oldWidth = 0;
        int oldTop = 0;
        int oldLeft = 0;
        int inScreenChange = 0;

        Boolean isMaximized = false;
        private void GoFullScreen()
        {
            Screen screen = Screen.FromHandle(thisHandle);
            GoFullScreenWorker(screen);
        }
        private void GoFullScreenWorker(Screen screen)
        {
            if (0 != Interlocked.Exchange(ref inScreenChange, 1))
            {
                return;
            }
            if (isMaximized == false)
            {
                oldHeight = (int)Height;
                oldWidth = (int)Width;
                oldTop = (int)Top;
                oldLeft = (int)Left;
            }
            WindowState = WindowState.Normal;

            Top = screen.Bounds.Top;
            Left = screen.Bounds.Left;
            Matrix matrix;
            if (PSource != null)
            {
                matrix = PSource.CompositionTarget.TransformToDevice;                       // Have to check if this works when moving to a second monitor.
                Height = (int)(screen.Bounds.Height / matrix.M11);
                Width = (int)(screen.Bounds.Width / matrix.M22);
            }

            isMaximized = true;
            Activate();
            if (dispatcherPlaying.IsEnabled)
            {
                SetCursorTime();
            }
            SetDisplayMode();
            Interlocked.Exchange(ref inScreenChange, 0);
        }

        private void LeaveFullScreen(Boolean move)
        {
            if (0 != Interlocked.Exchange(ref inScreenChange, 1))
            {
                return;
            }
            if (isMaximized == true)
            {                
                Height = oldHeight;
                Width = oldWidth;                
                if (move == true)
                {
                    Top = oldTop;
                    Left = oldLeft;
                }
                isMaximized = false;
                Activate();                
                SetDisplayMode();
            }
            Interlocked.Exchange(ref inScreenChange, 0);
        }
        private void ToggleMaximize()
        {
            if (isMinimized)
            {
                Show();
                isMinimized = false;
            }
            if (isMaximized)
            {
                LeaveFullScreen(true);
            }
            else
            {
                GoFullScreen();
            }
        }
        private (int, int) GetMaxSize()
        {
            var bounds = Screen.FromHandle(thisHandle).Bounds;
            ScreenMaxHeight = bounds.Height;
            ScreenMaxWidth = bounds.Width;
            return (bounds.Top, bounds.Left);
        }
    }
}