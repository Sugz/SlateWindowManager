using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Xml;

namespace SlateWindowManager
{
    public class PositionManager
    {
        #region INTEROP

        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;
        
        private const int SW_SHOWMAXIMIZED = 3;

        

        struct RECT
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        static extern bool GetWindowRect(IntPtr hwnd, ref RECT rectangle);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion INTEROP


        AutomationElement _SlateWindowAutomationElement;
        bool _HasPosition = false;
        string _xmlFile;
        int _Left = 0;
        int _Top = 0;


        public void Init(string xmlDirectory)
        {
            _xmlFile = Path.Combine(xmlDirectory, "SlatePositionManager.xml");
            if (File.Exists(_xmlFile))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_xmlFile);

                XmlElement root = doc.DocumentElement;
                _Left = int.Parse(root.Attributes["Left"].Value);
                _Top = int.Parse(root.Attributes["Top"].Value);

                _HasPosition = true;
            }
        }

        public void Start(int maxWindowHandle)
        {
            AutomationElement maxWindowAutomationElement = AutomationElement.FromHandle(new IntPtr(maxWindowHandle));
            Automation.AddAutomationEventHandler(WindowPattern.WindowOpenedEvent,
                                                 maxWindowAutomationElement,
                                                 TreeScope.Descendants,
                                                 OnWindowOpened);
        }

        public void Stop()
        {
            Automation.RemoveAllEventHandlers();
        }

        void OnWindowOpened(object sender, AutomationEventArgs automationEventArgs)
        {
            try
            {
                if (sender is AutomationElement element && element.Current.ClassName == "NodeJoeMainWindow")
                {
                    IntPtr handle = new IntPtr(element.Current.NativeWindowHandle);
                    if (handle != IntPtr.Zero)
                    {
                        _SlateWindowAutomationElement = element;

                        if (_HasPosition)
                        {
                            SetWindowPos(handle, 0, _Left, _Top, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
                            ShowWindow(handle, SW_SHOWMAXIMIZED);
                            Stop();
                        }

                        // The slate desired position has never been stored yet
                        else
                        {
                            // Store the position when the window closes
                            Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent,
                                                                 element,
                                                                 TreeScope.Subtree,
                                                                 OnWindowClosed);
                        }
                    }
                }

            }
            catch (ElementNotAvailableException)
            {
            }
        }

        void OnWindowClosed(object sender, AutomationEventArgs automationEventArgs)
        {
            Rect pos = _SlateWindowAutomationElement.Current.BoundingRectangle;
            _Left = (int)pos.Left;
            _Top = (int)pos.Top;
            Serialize();
            Stop();
        }


        public void Serialize()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("SlatePosition");
            root.SetAttribute("Left", _Left.ToString());
            root.SetAttribute("Top", _Top.ToString());
            doc.AppendChild(root);
            doc.Save(_xmlFile);
        }
    }
}
