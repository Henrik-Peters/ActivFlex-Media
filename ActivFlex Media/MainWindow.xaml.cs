﻿#region License
// ActivFlex Media - Manage your media libraries
// Copyright(C) 2017 Henrik Peters
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using WinInterop = System.Windows.Interop;
using System.Runtime.InteropServices;

namespace ActivFlex
{
    /// <summary>
    /// Viewlogic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Will be true when the window
        /// was set into fullscreen state
        /// </summary>
        private bool Fullscreen { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            //titlebar button events
            btnMinimize.Click += (s, e) => this.WindowState = WindowState.Minimized;
            btnMaximize.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized && Fullscreen)
                    Fullscreen = false;

                this.WindowState = (this.WindowState == WindowState.Maximized
                                                        ? WindowState.Normal
                                                        : WindowState.Maximized);

            };
            btnClose.Click += (s, e) => this.Close();

            //fullscreen key bindings
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F11 || (e.SystemKey == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)) {
                    Fullscreen = !Fullscreen;

                    if (this.WindowState == WindowState.Maximized && Fullscreen)
                        this.WindowState = WindowState.Normal;
                    
                    //toggle the window state
                    this.WindowState = (Fullscreen ? WindowState.Maximized : WindowState.Normal);
                }
            };

            //window size limits
            this.SourceInitialized += new EventHandler(Window_SourceInitialized);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //toggle between the maximize and restore icon
            if (this.WindowState == WindowState.Normal) {
                this.btnMaximize.ContentDefault = FindResource("MaximizeIcon");
                this.btnMaximize.ContentHover   = FindResource("MaximizeIconHover");
                this.btnMaximize.ContentPressed = FindResource("MaximizeIconPressed");

            } else {
                this.btnMaximize.ContentDefault = FindResource("RestoreIcon");
                this.btnMaximize.ContentHover   = FindResource("RestoreIconHover");
                this.btnMaximize.ContentPressed = FindResource("RestoreIconPressed");
            }
        }

        #region WindowSizeLimits

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {         
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));        
            public RECT rcMonitor = new RECT();      
            public RECT rcWork = new RECT();       
            public int dwFlags = 0;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            //Get the current window handle pointer
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;

            //Add an event handler to the window handle. Forward all events to the WindowProc function
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }
        
        private System.IntPtr WindowProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            switch (msg) {
                //Fired when the size or position of the window is about to change
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            //Adjust the maximized size to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero) {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                
                //Override the maximized width and height of the window
                if (Fullscreen) {
                    mmi.ptMaxSize.x = Math.Abs(rcMonitorArea.right - rcMonitorArea.left);
                    mmi.ptMaxSize.y = Math.Abs(rcMonitorArea.bottom - rcMonitorArea.top);

                } else {
                    mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                    mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                }

            } else {
                Debug.WriteLine("Monitor-PTR is zero: Maximized window limits not set");
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        #endregion
    }
}
