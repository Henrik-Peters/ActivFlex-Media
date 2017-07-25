#region License
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
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinInterop = System.Windows.Interop;
using ActivFlex.ViewModels;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Data;

namespace ActivFlex
{
    /// <summary>
    /// Viewlogic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// View model instance for this window
        /// </summary>
        private MainViewModel vm;

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

            //other window properties
            this.vm = new MainViewModel();
            this.DataContext = vm;
            this.SourceInitialized += new EventHandler(Window_SourceInitialized);
            Window_StateChanged(this, null);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //toggle between the maximize and restore icon
            if (this.WindowState == WindowState.Normal) {
                this.btnMaximize.ContentDefault = FindResource("MaximizeIcon");
                this.btnMaximize.ContentHover   = FindResource("MaximizeIconHover");
                this.btnMaximize.ContentPressed = FindResource("MaximizeIconPressed");
                this.outerBorder.Visibility = Visibility.Visible;

            } else {
                this.btnMaximize.ContentDefault = FindResource("RestoreIcon");
                this.btnMaximize.ContentHover   = FindResource("RestoreIconHover");
                this.btnMaximize.ContentPressed = FindResource("RestoreIconPressed");
                this.outerBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void NavView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = NavView.SelectedItem;

            if (item is LogicalDriveNavItem) {
                var driveItem = item as LogicalDriveNavItem;
                vm.BrowseFileSystem.Execute(driveItem.DisplayName);
            }
        }

        private void MediaScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                if (e.Delta > 0) {
                    vm.IncreaseZoom?.Execute(null);
                } else {
                    vm.DecreaseZoom?.Execute(null);
                }
            }
        }

        private void MediaPresenter_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed && vm.ImagePresentActive) {
                MediaPresenter.ResetRenderTransform();
                e.Handled = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Add:
                    if (vm.ImagePresentActive) {
                        MediaPresenter.IncreaseZoom();
                    } else {
                        vm.IncreaseZoom?.Execute(null);
                    }
                    e.Handled = true;
                    break;

                case Key.Subtract:
                    if (vm.ImagePresentActive) {
                        MediaPresenter.DecreaseZoom();
                    } else {
                        vm.DecreaseZoom?.Execute(null);
                    }
                    e.Handled = true;
                    break;

                case Key.Left:
                    if (vm.ImagePresentActive) {
                        vm.PreviousImage?.Execute(null);
                    }
                    break;

                case Key.Right:
                    if (vm.ImagePresentActive) {
                        vm.NextImage?.Execute(null);
                    }
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //The ItemPanelTemplate of MediaItemControl is loaded after the zoom is set.
            //The EventTrigger of the scale animation will miss the first UpdateTarget-Event.
            //This is fixed by firing the UpdateTarget-Event or changing the zoom again:
            try {
                Border border = VisualTreeHelper.GetChild(this.MediaItemControl, 0) as Border;
                ItemsPresenter itemsPresenter = VisualTreeHelper.GetChild(border, 0) as ItemsPresenter;
                WrapPanel wrapPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as WrapPanel;

                DoubleAnimation animationX = wrapPanel.FindName("thumbScaleAnimationX") as DoubleAnimation;
                DoubleAnimation animationY = wrapPanel.FindName("thumbScaleAnimationY") as DoubleAnimation;
                
                BindingOperations.GetBindingExpression(animationX, DoubleAnimation.ToProperty).UpdateTarget();
                BindingOperations.GetBindingExpression(animationY, DoubleAnimation.ToProperty).UpdateTarget();
            } catch {
                vm.Zoom = 0.0;
                vm.Zoom = 1.0;
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