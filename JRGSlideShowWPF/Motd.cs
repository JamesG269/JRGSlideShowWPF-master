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
        List<string> motd = new List<string>();
        List<string> tempMotd = new List<string>();
        private void EnableMotd()
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
                if (motd.Count == 0)
                {
                    motdmessage = @"Messages should be in c:\users\username\documents\motd.txt";             
                }
                else
                {
                    int i = Rand.Next(0, tempMotd.Count);
                    motdmessage = tempMotd[i];
                    tempMotd.RemoveAt(i);
                    if (tempMotd.Count == 0)
                    {
                        tempMotd = motd.ToList();
                    }
                }                
                MotdClass.messageDisplayStart(motdmessage, -1, true, false);
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
                motd = File.ReadAllLines(motdFilePath).ToList();
                motd = motd.Where(c => !string.IsNullOrEmpty(c)).ToList();
                tempMotd = motd.ToList();
                InfoTextBoxClass.messageDisplayStart(motd.Count.ToString() + " MOTDs loaded.", 5);                
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