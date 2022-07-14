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
        public class TextBoxStack
        {
            public string message = "test message";
            public int time = 0;
            public bool interruptable = true;
            public bool highPriority = false;
            public bool live = false;
        }
        public class TextBoxMessage
        {
            public System.Windows.Threading.DispatcherTimer dispatchTimer;
            public System.Windows.Controls.TextBlock textBlock;
            public static int InterlockInt = 0;
            public TextBoxStack textBoxStackCurrent = new TextBoxStack();
            public Stack<TextBoxStack> textBoxStackList = new Stack<TextBoxStack>();
            public void messageDisplayEndUninterruptable(Action action)
            {
                dispatchTimer.Stop();
                dispatchTimer.IsEnabled = false;
                textBoxStackCurrent.live = false;
                if (textBoxStackList.Count > 0)
                {
                    var textBoxStackSend = textBoxStackList.Pop();
                    messageDisplayStart(textBoxStackSend.message, textBoxStackSend.time, textBoxStackSend.interruptable, textBoxStackSend.highPriority);
                }
                else
                {
                    DispatcherWait(action + new Action(() => {
                        textBlock.Visibility = Visibility.Hidden;
                    }));
                }
            }
            public void messageDisplayEnd(object sender, EventArgs e)
            {
                messageDisplayEndUninterruptable(new Action(() => { }));
            }
            public void DispatcherWait(Action action)
            {
                while (0 != Interlocked.Exchange(ref InterlockInt, 1))
                {
                    Thread.Sleep(10);
                }
                System.Windows.Application.Current.Dispatcher.Invoke(action + new Action(() => { Interlocked.Exchange(ref InterlockInt, 0); }));
                while (InterlockInt != 0)
                {
                    Thread.Sleep(10);
                }
            }
            public void messageDisplayStart(string displayText, int seconds, bool Interruptable = false, bool highPriority = false)
            {
                var textBoxStackNew = new TextBoxStack();
                textBoxStackNew.message = displayText;
                textBoxStackNew.time = seconds;
                textBoxStackNew.interruptable = Interruptable;
                textBoxStackNew.highPriority = highPriority;
                textBoxStackNew.live = true;
                if (textBoxStackNew.highPriority == false && (textBoxStackCurrent.live == true && dispatchTimer.IsEnabled == true && textBoxStackCurrent.interruptable == false))
                {
                    textBoxStackList.Push(textBoxStackNew);
                    InterlockInt = 0;
                    return;
                }
                textBoxStackCurrent = textBoxStackNew;
                DispatcherWait(new Action(() => {
                    textBlock.Visibility = Visibility.Visible;
                    textBlock.Text = displayText;
                }));
                if (seconds == -1)
                {
                    return;
                }
                dispatchTimer.Stop();
                dispatchTimer.Interval = new TimeSpan(0, 0, 0, seconds, 0);
                dispatchTimer.Tick -= messageDisplayEnd;
                dispatchTimer.Tick += messageDisplayEnd;
                dispatchTimer.Start();
                return;
            }
        }
    }
}