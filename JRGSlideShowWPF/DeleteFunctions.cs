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
        private async Task CopyDeleteWorker()
        {
            if (PrivateMode == true)
            {
                MessageBox.Show("Private Mode is enabled, copy not done.");
                return;
            }
            if (ImageIdxListDeletePtr != -1 && ImageIdxList[ImageIdxListDeletePtr] != -1)
            {
                string destPath = "";
                string sourcePath = ImageList[ImageIdxList[ImageIdxListDeletePtr]].FullName;
                try
                {
                    destPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    destPath = Path.Combine(destPath, Path.GetFileName(sourcePath));
                    File.Copy(sourcePath, destPath);
                    MessageBox.Show("Image copied to " + destPath);
                    await DeleteNoInterlock();
                }
                catch
                {
                    MessageBox.Show("Error: image not copied to " + destPath);
                }
            }
        }
        private bool Undelete()
        {
            InfoBlockControl.Visibility = Visibility.Visible;
            if (DeletedFiles.Count == 0)
            {
                InfoTextBoxClass.messageDisplayStart("No more files to undelete.", 5);
                return false;
            }
            string LastDeleted = DeletedFiles.Pop();
            if (LastDeleted == "")
            {
                InfoTextBoxClass.messageDisplayStart(LastDeleted + " UNDELETE ERROR.", 5);
                return false;
            }
            FolderItems folderItems = RecyclingBin.Items();
            for (int i = 0; i < folderItems.Count; i++)
            {
                FolderItem FI = folderItems.Item(i);
                string FileName = RecyclingBin.GetDetailsOf(FI, 0);
                if (Path.GetExtension(FileName) == "")
                {
                    FileName += Path.GetExtension(FI.Path);
                }
                //Necessary for systems with hidden file extensions.
                string FilePath = RecyclingBin.GetDetailsOf(FI, 1);
                if (String.Compare(LastDeleted, Path.Combine(FilePath, FileName), true) == 0)
                {
                    FileInfo undelFile;
                    try
                    {
                        DoVerb(FI, "ESTORE");
                        undelFile = new FileInfo(LastDeleted);
                    }
                    catch
                    {
                        InfoTextBoxClass.messageDisplayStart(LastDeleted + " Could not be undeleted, file not found.", 5);
                        return false;
                    }
                    InfoTextBoxClass.messageDisplayStart(LastDeleted + " Restored.", 5);
                    Array.Resize(ref ImageList, ImageList.Length + 1);
                    ImageList[ImageList.Length - 1] = undelFile;
                    Array.Resize(ref ImageIdxList, ImageIdxList.Length + 1);
                    ImageIdxList[ImageIdxList.Length - 1] = ImageIdxList.Length - 1;
                    ImagesNotNull++;
                    return true;
                }
            }
            return false;
        }
        public System.Collections.Generic.Stack<string> DeletedFiles = new System.Collections.Generic.Stack<string>();

        private async Task DeleteNoInterlock()
        {
            if (ImageIdxListDeletePtr == -1 || ImageIdxList[ImageIdxListDeletePtr] == -1)
            {
                return;
            }
            var fileName = ImageList[ImageIdxList[ImageIdxListDeletePtr]].FullName;
            bool result = true;
            if (!IsUserjgentile)
            {
                result = MessageBox.Show("Confirm delete: " + fileName, "Confirm delete image.", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            }
            if (result)
            {
                try
                {
                    if (memStream != null)
                    {
                        memStream.Dispose();
                    }
                    memStream = null;
                    displayPhoto = null;
                    ImageControl.Source = null;
                    RecyclingBin.MoveHere(fileName);
                    if (!File.Exists(fileName))
                    {
                        DeletedFiles.Push(fileName);
                        ImageIdxList[ImageIdxListDeletePtr] = -1;
                        ImagesNotNull--;
                        ImageIdxListDeletePtr = -1;
                        InfoTextBoxClass.messageDisplayStart("Deleted: " + fileName, 5);
                    }
                    else
                    {
                        MessageBox.Show("Error: Could not delete image.");
                    }
                }
                catch
                {
                    MessageBox.Show("Exception: Could not delete image.");
                }
            }
            if (ImagesNotNull <= 0)
            {
                ImageListReady = false;
            }
            await DisplayGetNextImageWithoutCheck(1);
        }
    }
}