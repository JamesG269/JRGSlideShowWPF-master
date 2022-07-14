﻿using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {        
        int MouseWheelCount = 0;
        int MouseOneIntCount = 0;

        private bool mRestoreForDragMove;

        private async void MouseWheel2(object sender, MouseWheelEventArgs e)
        {
            MouseWheelCount++;
            if (OneInt == 1)
            {
                MouseOneIntCount++;
                return;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }            
            if (e.Delta > 0)
            {                
                await DisplayGetNextImageWithoutCheck(1);                             
            }
            else if (e.Delta < 0)
            {
                await DisplayGetNextImageWithoutCheck(-1);
            }
            Interlocked.Exchange(ref OneInt, 0);
        }

        Boolean MouseLeftDown = false;
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseHidden == true)
            {
                unhideCursor();
            }
            if (e.ClickCount == 2)
            {
                if (isMaximized == true)
                {
                    var point = System.Windows.Forms.Cursor.Position;            // have to get cursor position before leavefullscreen() I think...
                    LeaveFullScreen(false);
                    WindowToCursor(point);
                }
                else
                {
                    GoFullScreen();
                }
            }
            else
            {
                mRestoreForDragMove = isMaximized;

                if (!isMaximized)
                {
                    MouseLeftDown = true;
                }
                StartMouseMove = System.Windows.Forms.Cursor.Position;
            }
        }
        System.Drawing.Point StartMouseMove;

        Point lastMovePosition;
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var currentPosition = e.GetPosition((IInputElement)sender);

            if (mRestoreForDragMove)
            {
                var point = System.Windows.Forms.Cursor.Position;
                if (DidMouseMove() == true)
                {
                    mRestoreForDragMove = false;
                    LeaveFullScreen(false);
                    WindowToCursor(point);
                    try
                    {
                        DragMove();
                    }
                    catch { }
                }
            }
            else if (MouseLeftDown == true)
            {
                var point = System.Windows.Forms.Cursor.Position;
                MouseLeftDown = false;
                LeaveFullScreen(false);
                WindowToCursor(point);
                try
                {
                    DragMove();
                }
                catch { }
            }
            if (currentPosition != lastMovePosition)
            {
                if (MouseHidden == true)
                {
                    unhideCursor();
                }
                lastMovePosition = currentPosition;
            }
            if (isMaximized && DidMouseMove())
            {
                //unhideCursor();
            }
        }
        private void unhideCursor()
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            MouseHidden = false;
            SetCursorTime();
        }
        private bool DidMouseMove()
        {
            var point = System.Windows.Forms.Cursor.Position;
            if (Math.Abs(point.X - StartMouseMove.X) >= SystemParameters.MinimumHorizontalDragDistance * 3 ||
                Math.Abs(point.Y - StartMouseMove.Y) >= SystemParameters.MinimumVerticalDragDistance * 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void WindowToCursor(System.Drawing.Point point)
        {
            Matrix matrix;
            if (PSource != null)
            {
                matrix = PSource.CompositionTarget.TransformToDevice;                       // Have to check if this works when moving to a second monitor.
                Left = (int)((point.X / matrix.M11) - (RestoreBounds.Width / 2));
                Top = (int)((point.Y / matrix.M22) - (RestoreBounds.Height / 2));
            }
        }
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
            MouseLeftDown = false;
        }
        Boolean MouseHidden = false;
        DateTime CursorNextHide = new DateTime();
        private void MouseHide(object sender, EventArgs e)
        {
            
            if (isMaximized)
            {                
                if (CursorNextHide.CompareTo(DateTime.Now) > 0)
                {                    
                    return;
                }                
                if (MouseHidden == true)
                {
                    return;
                }
                MouseHidden = true;
                this.Cursor = System.Windows.Input.Cursors.None;
            }                        
        }

        public void SetCursorTime()
        {
            CursorNextHide = DateTime.Now.AddSeconds(5);
        }
        
        public  void MouseInitTimer()
        {
            SetCursorTime();            
            dispatcherHideCursor = new System.Windows.Threading.DispatcherTimer();
            dispatcherHideCursor.Stop();
            dispatcherHideCursor.Interval = TimeSpan.FromMilliseconds(200);
            dispatcherHideCursor.Tick += MouseHide;
            dispatcherHideCursor.Start();
        }
    }
}
