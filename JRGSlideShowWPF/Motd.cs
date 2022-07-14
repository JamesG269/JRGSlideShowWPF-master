using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace JRGSlideShowWPF
{
    public partial class MainWindow : Window
    {
        private void EnableMotdCode()
        {
            if (Starting)
            {
                return;
            }
            getMotd();
            PutMotd();
        }
        Random Rand = new Random();
        private void PutMotd()
        {
            if (ShowMotd)
            {
                string motdmessage = "";
                if (motd.Length == 0)
                {
                    motdmessage = @"Messages should be in c:\users\username\documents\motd.txt";
                    return;                
                }
                else
                {
                    int i = Rand.Next(0, motd.Length);
                    motdmessage = motd[i];
                }                
                MotdClass.messageDisplayStart(motdmessage, -1, true, true);
            }
        }
        public void getMotd()
        {
            if (!ShowMotd)
            {
                return;
            }
            string motdFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\motd.txt";
            if (File.Exists(motdFilePath))
            {
                motd = File.ReadAllLines(motdFilePath);
                InfoTextBoxClass.messageDisplayStart(motd.Length.ToString() + " MOTDs loaded.", 5);
            }
            else
            {
                ShowMotd = false;
                InfoTextBoxClass.messageDisplayStart("no MOTD found at " + motdFilePath, 5, false, false);
                MotdXaml.IsChecked = false;
            }
        }
    }
}