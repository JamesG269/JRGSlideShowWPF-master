using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {

        public string RegKeyName = @"SOFTWARE\JRGSlideShowWPF";
        public RegistryKey RegKeyCurrent;
        public int TimerSeconds = 5;
        public bool PrivateMode = false;
        public bool LoadSettingsRegistry()
        {
            bool ret = true;                        
            try
            {
                RegKeyCurrent = Registry.CurrentUser.OpenSubKey(RegKeyName);
                if (RegKeyCurrent != null)              
                {                                        
                    TimerSeconds = Convert.ToInt32(RegKeyCurrent.GetValue("TimerSeconds", RegistryValueKind.DWord));
                    RandomizeImages = Convert.ToInt32(RegKeyCurrent.GetValue("Randomize", RegistryValueKind.DWord)) == 0 ? false : true;                    
                    AllowMonitorSleepPaused = Convert.ToInt32(RegKeyCurrent.GetValue("AllowSleepPaused", RegistryValueKind.DWord)) == 0 ? false : true;                    
                    AllowMonitorSleepPlaying = Convert.ToInt32(RegKeyCurrent.GetValue("AllowSleepPlay", RegistryValueKind.DWord)) == 0 ? false : true;
                    AllowMonitorSleepFullScreenOnly = Convert.ToInt32(RegKeyCurrent.GetValue("AllowSleepFull", RegistryValueKind.DWord)) == 0 ? false : true;                    
                    SlideShowDirectory = RegKeyCurrent.GetValue("SlideShowFolder", RegistryValueKind.String).ToString();
                    PrivateMode = Convert.ToInt32(RegKeyCurrent.GetValue("PrivateMode", RegistryValueKind.DWord)) == 0 ? false : true;                    
                    ShowMotd = Convert.ToInt32(RegKeyCurrent.GetValue("ShowMOTD", RegistryValueKind.DWord)) == 0 ? false : true;
                    RegKeyCurrent.Close();
                }                
            }
            catch
            {
                MessageBox.Show("Could not access registry keys.");
                ret = false;
            }            
            return ret;
        }
        public bool SaveSettingsRegistry()
        {
            bool ret = false;
            try
            {
                RegKeyCurrent = Registry.CurrentUser.CreateSubKey(RegKeyName);
                if (RegKeyCurrent != null)
                {
                    RegKeyCurrent.SetValue("TimerSeconds", dispatcherPlaying.Interval.Seconds, RegistryValueKind.DWord);
                    RegKeyCurrent.SetValue("Randomize", RandomizeImages == false ? 0 : 1, RegistryValueKind.DWord);
                    RegKeyCurrent.SetValue("AllowSleepPaused", AllowMonitorSleepPaused == false ? 0 : 1, RegistryValueKind.DWord);
                    RegKeyCurrent.SetValue("AllowSleepPlay", AllowMonitorSleepPaused == false ? 0 : 1, RegistryValueKind.DWord);
                    RegKeyCurrent.SetValue("AllowSleepFull", AllowMonitorSleepFullScreenOnly == false ? 0 : 1, RegistryValueKind.DWord);
                    RegKeyCurrent.SetValue("SlideShowFolder", SlideShowDirectory, RegistryValueKind.String);
                    RegKeyCurrent.SetValue("PrivateMode", PrivateMode == false ? 0 : 1, RegistryValueKind.DWord);
                    RegKeyCurrent.SetValue("ShowMOTD", ShowMotd == false ? 0 : 1, RegistryValueKind.DWord);
                    RegKeyCurrent.Close();
                    ret = true;
                }                
            }
            catch { }
            if (ret == false)
            {
                MessageBox.Show("Could not access registry keys.");
            }
            return ret;
        } 
    }
}