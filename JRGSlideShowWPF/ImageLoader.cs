using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        FileInfo[] ImageList;
        int[] ImageIdxList;
        Boolean ImageListReady = false;

        int ImageIdxListPtr = 0;
        int ImageIdxListDeletePtr = -1;
        private async void StartGetFiles()
        {
            while (0 != Interlocked.Exchange(ref OneInt, 1))
            {
                await Task.Delay(10);
            }
            while (0 != Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 1))
            {
                await Task.Delay(10);
            }
            StartGetFilesNoInterlock();
            Interlocked.Exchange(ref StartGetFilesBW_IsBusy, 0);
            Interlocked.Exchange(ref OneInt, 0);
        }
        private void StartGetFilesNoInterlock()
        {
            Boolean ImageListReadyBackup = ImageListReady;
            ImageListReady = false;
            StartGetFiles_Cancel = false;
            List<FileInfo> NewImageList = new List<FileInfo>();
            GetFilesCode(NewImageList);
            if (StartGetFiles_Cancel != true && NewImageList != null && NewImageList.Count > 0)
            {
                ImageList = null;
                ImageList = new FileInfo[NewImageList.Count];
                int i = 0;
                foreach (var n in NewImageList)
                {
                    ImageList[i] = n;
                    i++;
                }
                ImagesNotNull = ImageList.Length;
                CreateIdxListCode();
                ImageListReady = true;
            }
            else
            {
                ImageListReady = ImageListReadyBackup;
            }
        }

        private void CreateIdxListCode()
        {
            if (ImageList != null && ImageList.Length > 0)
            {
                ImageIdxListDeletePtr = -1;
                ImageIdxList = null;
                ImageIdxList = new int[ImageList.Length];
                for (int i = 0; i < ImageList.Length; i++)
                {
                    ImageIdxList[i] = i;
                }
                ImageIdxListPtr = 0;
                if (RandomizeImages == true)
                {
                    InitRNGKeys();
                    EncryptIdxListCode();
                    InitRNGKeys();
                }
                //FilesLoaded = imageFiles.Count;
            }
            else
            {
                MessageBox.Show("No images found in: " + SlideShowDirectory);
                //Focus();
                return;
            }
        }        
        private void GetFilesCode(List<FileInfo> NewImageList)
        {                       
            if (string.IsNullOrEmpty(SlideShowDirectory) || !Directory.Exists(SlideShowDirectory))
            {
                return;
            }
            string searchString = "*.jpg;*.jpeg;*.jpe;*.jif;*.jfif;*.jfi;*.png;*.bmp;*.gif;*.tif;*.tiff;*.webp";
            InfoTextBoxClass.messageDisplayStart("Finding " + searchString + "...", -1, false, true);            
            GetFiles(SlideShowDirectory, searchString, NewImageList);
            InfoTextBoxClass.messageDisplayStart(NewImageList.Count + " images found.", 5, false, true);            
        }
        
        
        public void GetFiles(string path, string searchPattern, List<FileInfo> NewImageList)
        {
            string[] patterns = searchPattern.Split(';');
            Stack<string> dirs = new Stack<string>();
            if (!Directory.Exists(path))
            {
                return;
            }
            dirs.Push(path);
            int NextListUpdate = 0;
            do
            {
                string currentDir = dirs.Pop();
                InfoTextBoxClass.messageDisplayStart("Scanning directories..." + currentDir + Environment.NewLine + "Images found: " + NewImageList.Count, -1, false, true);
                try
                {
                    string[] subDirs = Directory.GetDirectories(currentDir);
                    foreach (string str in subDirs)
                    {
                        dirs.Push(str);
                    }
                }
                catch { }
                try
                {
                    foreach (string filter in patterns)
                    {
                        if (StartGetFiles_Cancel)
                        {
                            InfoTextBoxClass.messageDisplayStart("Scanning directories canceled... " + NewImageList.Count, -1, false, true);
                            NewImageList.Clear();
                            return;
                        }                        
                        DirectoryInfo dirInfo = new DirectoryInfo(currentDir);
                        FileInfo[] fs = dirInfo.GetFiles(filter);
                        NewImageList.AddRange(fs);
                        NextListUpdate += fs.Length;
                        if (NextListUpdate > 100)
                        {
                            NextListUpdate = 0;
                            InfoTextBoxClass.messageDisplayStart("Scanning directories..." + currentDir + Environment.NewLine + "Images found: " + NewImageList.Count, -1, false, true);
                        }
                    }
                }
                catch { }
            } while (dirs.Count > 0);
        }
    }
}