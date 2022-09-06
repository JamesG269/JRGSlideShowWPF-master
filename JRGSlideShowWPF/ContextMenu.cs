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
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

        public static Shell shell = new Shell();

        public static Folder RecyclingBin = shell.NameSpace(10);

        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        private async void GoogleImageSearch_Click(object sender, RoutedEventArgs e)
        {
            if (PrivateModeCheckBox.IsChecked == true)
            {
                MessageBox.Show("Private Mode is enabled, google search not done.");
                return;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            var result = MessageBox.Show("Confirm google look up, Private Mode is DISABLED. ", "Confirm google look up.", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                await Task.Run(() => GoogleImageSearch(ImageList[ImageIdxList[ImageIdxListPtr]].FullName, true, _cancelTokenSource.Token));
            }
            OneInt=0;
        }        
        private async void ContextMenuOpenFolder(object sender, RoutedEventArgs e)
        {
            await OpenDirCheckCancel();
        }

        int StartGetFilesBW_IsBusy = 0;
        Boolean StartGetFiles_Cancel = false;
        private async Task OpenDirCheckCancel()
        {            
            StartGetFiles_Cancel = true;
            while (0 != Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 1))
            {
                await Task.Delay(10);
            }
            StartGetFiles_Cancel = false;
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            if (string.IsNullOrEmpty(SlideShowDirectory) || !Directory.Exists(SlideShowDirectory))
            {
                dialog.SelectedPath = SlideShowDirectory;
            }
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SlideShowDirectory = dialog.SelectedPath;
                await Task.Run(() => StartGetFilesNoInterlock());
                await DisplayGetNextImageWithoutCheck(1);
            }
            OneInt =0;
            StartGetFilesBW_IsBusy=0;            
            return;
        }
        
        private async void Benchmark_Click(object sender, RoutedEventArgs e)
        {
            if (0 != Interlocked.Exchange(ref benchmarkRunning, 1))
            {
                benchmarkStop = true;
                return;
            }
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            bool ShowPictureBackup = dispatcherPlaying.IsEnabled;
            dispatcherPlaying.IsEnabled = false;
            await Task.Run(() => BenchMarkWorker());
            benchmarkRunning = 0;
            OneInt = 0;
            dispatcherPlaying.IsEnabled = ShowPictureBackup;
        }

        

        private void ContextMenuExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private async void ContextMenuNext(object sender, RoutedEventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            await DisplayGetNextImageWithoutCheck(1);            
            OneInt=0;
        }

        private async void ContextMenuPrev(object sender, RoutedEventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            await DisplayGetNextImageWithoutCheck(-1);
            OneInt=0;
        }
               
        private void ContextMenuPlay(object sender, RoutedEventArgs e)
        {
            if (dispatcherPlaying.IsEnabled == false) 
            {
                Play();
            }
            else
            {
                Stop();
            }            
        }
        private async void ContextMenuCopyDelete(object sender, RoutedEventArgs e)
        {
            await CopyDeleteCode();
        }
        private async Task<Boolean> CopyDeleteCode()
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            await CopyDeleteWorker();
            OneInt=0;
            return true;
        }

        private async void ContextMenuDelete(object sender, RoutedEventArgs e)
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }          
            await DeleteNoInterlock();
            OneInt=0;
        }        
        
        private bool DoVerb(FolderItem Item, string Verb)
        {
            foreach (FolderItemVerb FIVerb in Item.Verbs())
            {
                if (FIVerb.Name.ToUpper().Contains(Verb.ToUpper()))
                {
                    FIVerb.DoIt();
                    return true;
                }
            }
            return false;
        }
        
        private void ContextMenuChangeTimer(object sender, RoutedEventArgs e)
        {
            ChangeTimerCode();
        }
        
        private void ContextMenuFullScreen(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private async void CheckedRandomize(object sender, RoutedEventArgs e)
        {
            while (Interlocked.Exchange(ref OneInt, 1) == 1)
            {
                await Task.Delay(10);
            }
            RandomizeImages = ContextMenuCheckBox.IsChecked;
            if (!Starting)
            {
                await Task.Run(() => RandomizeBW_DoWork());
                await DisplayCurrentImage();
            }
            OneInt=0;
        }

        private void RandomizeBW_DoWork()
        {            
            ImageListReady = false;
            CreateIdxListCode();            
            ResizeImageCode();
            ImageListReady = true;            
        }
        private async void OpenInExplorer(object sender, RoutedEventArgs e)
        {
            while (Interlocked.Exchange(ref OneInt, 1) == 1)
            {
                await Task.Delay(10);
            }
            if (ImageIdxListDeletePtr == -1)
            {
                return;
            }
            FileInfo imageInfo = ImageList[ImageIdxList[ImageIdxListDeletePtr]];
            OpenFolderAndSelectItem(imageInfo.DirectoryName, imageInfo.Name);
            OneInt = 0;
            return;            
        }
        
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            PlayXaml.Header = dispatcherPlaying.IsEnabled == false ? "Play (currently Paused)" : "Pause (Currently Playing)";
            GetMonitors();
        }
        
        private async void DisplayInfo_Checked(object sender, RoutedEventArgs e)
        {
            if (displayingInfo == false)
            {
                await DisplayFileInfo();
            }
        }

        private async void DisplayInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            if (displayingInfo == true)
            {
                await DisplayFileInfo();
            }
        }
    }
}