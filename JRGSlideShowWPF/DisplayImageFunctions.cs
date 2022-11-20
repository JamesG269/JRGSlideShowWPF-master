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
        private async void DisplayNextImageTimer(object sender, EventArgs e)
        {
            await DisplayGetNextImage(1);
        }

        private async Task DisplayGetNextImage(int i)
        {
            if (1 == Interlocked.Exchange(ref OneInt, 1))
            {
                return;
            }
            await DisplayGetNextImageWithoutCheck(i);
            Interlocked.Exchange(ref OneInt, 0);
        }

        private async Task DisplayGetNextImageWithoutCheck(int i)
        {
            if (ImageListReady == true)
            {

                await Task.Run(() => LoadNextImage(i));
                await DisplayCurrentImage();
            }
        }

        private void LoadNextImage(int i)
        {
            IteratePicList(i);
            imageTimeToDecode.Restart();
            ResizeImageCode();
        }

        private void IteratePicList(int i)
        {
            do
            {
                if (RandomizeImages == true)
                {
                    if (ImageIdxListPtr == 0 && i == -1)
                    {
                        DecryptIdxListCode();
                    }
                    else if (ImageIdxListPtr == (ImageIdxList.Length - 1) && i == 1)
                    {
                        EncryptIdxListCode();
                    }
                }
                ImageIdxListPtr += i;
                ImageIdxListPtr = ((ImageIdxListPtr % ImageIdxList.Length) + ImageIdxList.Length) % ImageIdxList.Length;

            } while (ImageIdxList[ImageIdxListPtr] == -1);
        }
        bool ShowMotd = false;
        private async Task DisplayCurrentImage()
        {
            if (ImageReadyToDisplay == true)
            {
                bool dispatcherPlayingEnabled = dispatcherPlaying.IsEnabled;
                if (dispatcherPlayingEnabled)
                {
                    dispatcherPlaying.Stop();
                }
                if (ImageError == false)
                {
                    ImageIdxListDeletePtr = -1;
                    ImageControl.Source = displayPhoto;
                    imageTimeToDecode.Stop();
                    ImageIdxListDeletePtr = ImageIdxListPtr;
                    ImageReadyToDisplay = false;
                    imagesDisplayed++;
                    updateInfo();
                    PutMotd();
                }
                else
                {
                    InfoTextBoxClass.messageDisplayStart(ErrorMessage, 5, false, false);

                    if (IsUserjgentile)                                                         // delete the picture if it has display errors *and* user is the author
                    {
                        //await DeleteNoInterlock(true);
                        await Task.Delay(1);
                    }
                }
                if (dispatcherPlayingEnabled)
                {
                    dispatcherPlaying.Start();
                }
            }
        }
        int LastDisplayMode = 0;

        private void SetDisplayMode()
        {
            if (AllowMonitorSleepFullScreenOnly == false || (AllowMonitorSleepFullScreenOnly == true && isMaximized == true))
            {
                SetDisplayModeCheckPlay();
            }
            else
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                LastDisplayMode = 1;
            }
            updateInfo();
        }
        private void SetDisplayModeCheckPlay()
        {
            if (AllowMonitorSleepPlaying && dispatcherPlaying.IsEnabled)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                LastDisplayMode = 0;
                return;
            }
            else if (AllowMonitorSleepPaused && !dispatcherPlaying.IsEnabled)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                LastDisplayMode = 0;
                return;
            }
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            LastDisplayMode = 1;
        }

    }
}
