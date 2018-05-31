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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using WinInterop = System.Windows.Interop;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using static ActivFlex.Configuration.Parameter;
using static ActivFlex.FileSystem.FileSystemBrowser;
using ActivFlex.Configuration;
using ActivFlex.Libraries;
using ActivFlex.Controls;
using ActivFlex.ViewModels;
using ActivFlex.Navigation;
using ActivFlex.Media;

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

        /// <summary>
        /// Left repeat button of the media playback slider
        /// </summary>
        private RepeatButton TimeSliderActiveButton;

        /// <summary>
        /// True when the selection rectangle is currently dragged.
        /// </summary>
        private bool IsDragging = false;

        /// <summary>
        /// The starting point of the selection rectangle.
        /// </summary>
        private Point SelectionAnchor = new Point();

        /// <summary>
        /// The point with both coordinates zero.
        /// </summary>
        private static readonly Point Origin = new Point(0, 0);

        /// <summary>
        /// True when the left mouse button is down.
        /// </summary>
        private bool LeftMouseButtonDown = false;

        /// <summary>
        /// The panel that contains the items for drag selections.
        /// </summary>
        private WrapPanel WrapSelectionPanel;

        /// <summary>
        /// Selection rectangles of drag selection items.
        /// </summary>
        private Rect[] ItemRectangles;

        /// <summary>
        /// Point for the position of selection items.
        /// </summary>
        private Point ItemPos = new Point(0, 0);

        /// <summary>
        /// Rectangle for the current selection area.
        /// </summary>
        private Rect DragRect = new Rect(0, 0, 0, 0);

        /// <summary>
        /// Distance the cursor has to move before the selection starts.
        /// </summary>
        private static readonly double DragThreshold = 5;

        public MainWindow()
        {
            InitializeComponent();

            //titlebar button events
            btnMinimize.Click += (s, e) => this.WindowState = WindowState.Minimized;
            btnMaximize.Click += (s, e) => {
                if (this.WindowState == WindowState.Maximized && Fullscreen) {
                    ChangeFullscreenMode(false);

                } else {
                    this.WindowState = (this.WindowState == WindowState.Maximized
                                                        ? WindowState.Normal
                                                        : WindowState.Maximized);
                }
            };
            btnClose.Click += (s, e) => this.Close();

            //fullscreen key bindings
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F11 || (e.SystemKey == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)) {
                    ChangeFullscreenMode(!Fullscreen);
                }
            };

            //time slider
            TimeSlider.ApplyTemplate();
            TimeSliderActiveButton = TimeSlider.Template.FindName("LeftRepeatButton", TimeSlider) as RepeatButton;
            TimeSlider_ValueChanged(this, null);

            //other window properties
            this.vm = new MainViewModel(MediaPlayer, NavView, CurrentTimeLbl, MaxTimeLbl, MediaInfoIcon, LibraryItemControl,
                () => {
                    if (Fullscreen) {
                        this.Cursor = Cursors.None;
                    }
                },
                () => this.Cursor = null
            );

            this.DataContext = vm;
            this.MediaPlayer.Volume = MainViewModel.Config.Volume;
            this.SourceInitialized += new EventHandler(Window_SourceInitialized);
            Window_StateChanged(this, null);
            ChangeFullscreenMode(false);
            HandleStartupArguments();
            HandleStartupLayout();
        }

        /// <summary>
        /// Change the current mode for the fullscreen window.
        /// If enableFullscreen is true the window will enter
        /// the fullscreen mode, for false the normal mode.
        /// </summary>
        /// <param name="enableFullscreen">True to switch to fullscreen</param>
        public void ChangeFullscreenMode(bool enableFullscreen)
        {
            Fullscreen = enableFullscreen;

            //Check for a maximized state
            if (this.WindowState == WindowState.Maximized && Fullscreen)
                this.WindowState = WindowState.Normal;

            //toggle the window state
            this.WindowState = (Fullscreen ? WindowState.Maximized : WindowState.Normal);

            //cursor during video
            if (Fullscreen && vm.ShowVideoPlayback == Visibility.Visible) {
                this.Cursor = Cursors.None;
            } else {
                this.Cursor = null;
            }

            //Set the image presenter
            if (enableFullscreen) {
                Grid.SetRow(MediaPresenter, 0);
                Grid.SetColumn(MediaPresenter, 0);
                Grid.SetRowSpan(MediaPresenter, 4);
                Grid.SetColumnSpan(MediaPresenter, 3);

                Grid.SetRow(MediaBorder, 0);
                Grid.SetColumn(MediaBorder, 0);
                Grid.SetRowSpan(MediaBorder, 4);
                Grid.SetColumnSpan(MediaBorder, 3);

            } else {
                Grid.SetRow(MediaPresenter, 1);
                Grid.SetColumn(MediaPresenter, 0);
                Grid.SetRowSpan(MediaPresenter, 2);
                Grid.SetColumnSpan(MediaPresenter, 3);

                Grid.SetRow(MediaBorder, 1);
                Grid.SetColumn(MediaBorder, 0);
                Grid.SetRowSpan(MediaBorder, 2);
                Grid.SetColumnSpan(MediaBorder, 3);
            }
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

        private void NavViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                StackPanel stackPanel = (StackPanel)sender;
                var item = stackPanel.DataContext as NavItem;

                if (item is LogicalDriveNavItem driveItem) {
                    vm.BrowseFileSystem.Execute(driveItem.DisplayName);

                } else if (item is DirectoryNavItem directoryItem) {
                    vm.BrowseFileSystem.Execute(directoryItem.Path);

                } else if (item is LibraryNavItem libraryNavItem) {
                    vm.OpenMediaLibrary.Execute(libraryNavItem.MediaLibrary);

                } else if (item is ContainerNavItem containerNavItem) {
                    vm.OpenMediaContainer.Execute(containerNavItem.MediaContainer);
                }
            }
        }

        private void NavViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            item.IsSelected = false;
        }

        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (vm.editItem != null) {
                ContainerNavItem navItem = (ContainerNavItem)vm.editItem.DataContext;
                this.Focus();
                navItem.NameBox = Visibility.Visible;

                //Hide the edit box
                ContentPresenter presenter = vm.editItem.Template.FindName("PART_Header", vm.editItem) as ContentPresenter;
                TextBlock nameBox = presenter.ContentTemplate.FindName("NameBox", presenter) as TextBlock;
                TextBox editBox = presenter.ContentTemplate.FindName("EditBox", presenter) as TextBox;
                navItem.DisplayName = editBox.Text;
                editBox.Height = 0;

                if (navItem.MediaContainer.ContainerID == -1) {
                    //New container created
                    MediaContainer container = navItem.MediaContainer;
                    MediaContainer storedContainer = MainViewModel.StorageEngine.CreateContainer(container.Name, container.Parent, container.Library, false);

                    container.ContainerID = storedContainer.ContainerID;

                } else {
                    //Container update
                    MediaContainer container = navItem.MediaContainer;
                    MainViewModel.StorageEngine.UpdateContainer(container.ContainerID, container.Name, container.Parent.ContainerID, container.Expanded);
                    LibraryNavigator.Container_PropertyChanged(LibraryPathNavigator, new DependencyPropertyChangedEventArgs());
                }

                //Update the current browsing view when the new container should be visible
                if (MainViewModel.Config.ShowMediaContainers &&
                    vm.ActiveContainer != null && vm.ActiveContainer.ContainerID == navItem.MediaContainer.Parent.ContainerID) {
                    vm.OpenMediaContainer.Execute(vm.ActiveContainer);
                }

                vm.editItem = null;
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

        private void ToggleFullscreenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeFullscreenMode(!Fullscreen);
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TimeSlider.Value.Equals(0)) {
                TimeSliderActiveButton.Visibility = Visibility.Hidden;

            } else if (TimeSliderActiveButton.Visibility == Visibility.Hidden) {
                TimeSliderActiveButton.Visibility = Visibility.Visible;
            }
        }

        private void TimeSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.TimelineDragActive = true;
        }

        private void TimeSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            MediaPlayer.Position = TimeSpan.FromMilliseconds(TimeSlider.Value);
            vm.TimelineDragActive = false;
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.NaturalDuration.HasTimeSpan) {
                vm.MaxPlaybackTime = MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            vm.Stop.Execute(null);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vm.editItem == null && vm.renameItem == null) {
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

                    case Key.Space:
                        vm.PlayMode = !vm.PlayMode;
                        e.Handled = true;
                        break;

                    case Key.S:
                        vm.Stop.Execute(null);
                        e.Handled = true;
                        break;

                    case Key.M:
                        MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                        e.Handled = true;
                        break;
                }
            } else if (vm.editItem != null) {
                //Container name edit mode
                switch (e.Key) {
                    case Key.Enter:
                    case Key.Escape:
                        EditBox_LostFocus(sender, null);
                        e.Handled = true;
                        break;

                    //Replace these hotkeys with an input for the edit box
                    case Key.Add:
                        HotkeyEditBox(e.Key);
                        e.Handled = true;
                        break;

                    case Key.Subtract:
                        HotkeyEditBox(e.Key);
                        e.Handled = true;
                        break;
                }
            } else if (vm.renameItem != null) {
                //Library item rename mode
                switch (e.Key) {
                    case Key.Enter:
                    case Key.Escape:
                        vm.FinishLibraryItemRename.Execute(vm.renameItem);
                        e.Handled = true;
                        break;

                    //Replace these hotkeys with an input for the edit box
                    case Key.Add:
                        HotkeyRenameBox(e.Key);
                        e.Handled = true;
                        break;

                    case Key.Subtract:
                        HotkeyRenameBox(e.Key);
                        e.Handled = true;
                        break;
                }
            }

            //Delete hotkey
            if (e.Key == Key.Delete &&
                vm.renameItem == null &&
                vm.LibraryBrowsing &&
                vm.LibraryItems.Any(item => item.IsSelected)) {

                vm.DeleteLibraryItem.Execute(vm.LibraryItems.First(item => item.IsSelected).Proxy);
            }
        }

        private void HotkeyEditBox(Key key)
        {
            if (vm.editItem != null) {
                ContentPresenter presenter = vm.editItem.Template.FindName("PART_Header", vm.editItem) as ContentPresenter;
                TextBlock nameBox = presenter.ContentTemplate.FindName("NameBox", presenter) as TextBlock;
                TextBox editBox = presenter.ContentTemplate.FindName("EditBox", presenter) as TextBox;

                switch (key) {
                    case Key.Add:
                        editBox.Text += "+";
                        break;

                    case Key.Subtract:
                        editBox.Text += "-";
                        break;
                }

                editBox.SelectionStart = editBox.Text.Length;
                editBox.SelectionLength = 0;
            }
        }

        private void HotkeyRenameBox(Key key)
        {
            if (vm.renameItem != null) {
                UIElement itemPresenter = (UIElement)LibraryItemControl.ItemContainerGenerator.ContainerFromItem(vm.renameItem);
                var itemControl = VisualTreeHelper.GetChild(itemPresenter, 0);
                TextBox editBox = null;

                if (itemControl is ImageThumbnail imgThumb) {
                    editBox = imgThumb.FindName("NameEditBox") as TextBox;

                } else if (itemControl is MusicThumbnail musicThumb) {
                    editBox = musicThumb.FindName("NameEditBox") as TextBox;
                }

                if (editBox != null) {
                    //Append the keyboard input to the editbox
                    switch (key) {
                        case Key.Add:
                            editBox.Text += "+";
                            break;

                        case Key.Subtract:
                            editBox.Text += "-";
                            break;
                    }

                    editBox.SelectionStart = editBox.Text.Length;
                    editBox.SelectionLength = 0;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.editItem != null) {
                EditBox_LostFocus(sender, null);
                e.Handled = true;
            }

            if (vm.renameItem != null) {
                vm.FinishLibraryItemRename.Execute(vm.renameItem);
                e.Handled = true;
            }
        }

        private void SortModeButton_Click(object sender, RoutedEventArgs e)
        {
            //DropDown context menu for the sort modes
            SortModeButton.ContextMenu.PlacementTarget = SortModeButton;
            SortModeButton.ContextMenu.Placement = PlacementMode.Bottom;
            SortModeButton.ContextMenu.IsOpen = true;
        }

        private void MediaPresenter_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChangeFullscreenMode(!Fullscreen);
            e.Handled = true;
        }

        private void MediaBorder_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChangeFullscreenMode(!Fullscreen);
            e.Handled = true;
        }

        private void LibraryScrollViewer_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Copy;
            } else {
                e.Effects = DragDropEffects.None;
            }
        }

        private void LibraryScrollViewer_Drop(object sender, DragEventArgs e)
        {
            var dataObject = e.Data as DataObject;

            if (dataObject.ContainsFileDropList()) {
                ImportFiles(dataObject.GetFileDropList(), vm.ActiveContainer);

                //Update the current library browsing view
                vm.OpenMediaContainer.Execute(vm.ActiveContainer);
            }
        }

        private int ImportFiles(StringCollection fileList, MediaContainer container)
        {
            int importCount = 0;
            string[] mediaExtensions = MediaImage.ImageExtensions
                    .Concat(MediaMusic.MusicExtensions)
                    .Concat(MediaVideo.VideoExtensions)
                    .ToArray();

            foreach (string filePath in fileList) {
                string extension = LibraryItemFactory.GetPathExtension(filePath);

                if (File.Exists(filePath) && mediaExtensions.Contains(extension)) {
                    string name = Path.GetFileNameWithoutExtension(filePath);
                    MainViewModel.StorageEngine.CreateLibraryItem(name, filePath, container, DateTime.Now, null);
                    importCount++;
                }
            }

            return importCount;
        }

        private void NavViewItem_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement) {

                if (frameworkElement.DataContext is LibraryNavItem libraryItem) {
                    libraryItem.IsDropOver = true;

                } else if (frameworkElement.DataContext is ContainerNavItem navItem) {
                    navItem.IsDropOver = true;
                }

                NavViewItem_DragOver(sender, e);
            }

            e.Handled = true;
        }
        
        private void NavViewItem_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement) {

                if (frameworkElement.DataContext is LibraryNavItem libraryItem) {
                    libraryItem.IsDropOver = false;

                } else if (frameworkElement.DataContext is ContainerNavItem navItem) {
                    navItem.IsDropOver = false;
                }
            }

            e.Handled = true;
        }

        private void NavViewItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                MediaContainer targetContainer = null;
                var dataObject = e.Data as DataObject;
                StringCollection fileList = dataObject.GetFileDropList();

                //Find the media container for the import
                if (sender is FrameworkElement frameworkElement) {

                    if (frameworkElement.DataContext is LibraryNavItem libraryItem) {
                        libraryItem.IsDropOver = false;
                        targetContainer = libraryItem.MediaLibrary.RootContainer;

                    } else if (frameworkElement.DataContext is ContainerNavItem navItem) {
                        navItem.IsDropOver = false;
                        targetContainer = navItem.MediaContainer;
                    }
                }

                int importCount = 0;
                if (targetContainer != null) {
                    importCount = ImportFiles(dataObject.GetFileDropList(), targetContainer);
                }

                //Show the import container
                if (importCount > 0) {
                    vm.SelectMediaContainer.Execute(targetContainer);
                }
                e.Handled = true;
            }
        }

        private void NavViewItem_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Copy;
            } else {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void EditBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            NavViewItem_DragOver(sender, e);
        }

        private void EditBox_PreviewDragEnter(object sender, DragEventArgs e)
        {
            NavViewItem_DragEnter(sender, e);
            e.Handled = true;
        }

        private void EditBox_PreviewDragLeave(object sender, DragEventArgs e)
        {
            NavViewItem_DragLeave(sender, e);
            e.Handled = true;
        }

        private void ResetSelection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vm.ResetItemSelection();
        }

        private void LibraryScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.editItem != null) {
                EditBox_LostFocus(sender, null);
                e.Handled = true;
            }

            if (vm.renameItem != null) {
                //Only finish the renaming when the source is not the editbox
                if (e.OriginalSource.GetType().FullName != "System.Windows.Controls.TextBoxView") {
                    vm.FinishLibraryItemRename.Execute(vm.renameItem);
                    e.Handled = true;
                }
            }

            //Drag selection initializer
            if (e.ChangedButton == MouseButton.Left && e.OriginalSource == LibraryScrollViewer) {
                LeftMouseButtonDown = true;
                SelectionAnchor = e.GetPosition(LibraryScrollViewer);

                //Find the wrap panel of the media items
                var border = VisualTreeHelper.GetChild(LibraryItemControl, 0);
                var itemPresenter = VisualTreeHelper.GetChild(border, 0);
                WrapSelectionPanel = VisualTreeHelper.GetChild(itemPresenter, 0) as WrapPanel;
                ItemRectangles = new Rect[WrapSelectionPanel.Children.Count];

                for (int i = 0; i < ItemRectangles.Length; i++) {
                    ItemRectangles[i] = new Rect(0, 0, 10, 10);
                }

                vm.ResetItemSelection();

                //Force mouse event forwarding to the selection control
                LibraryScrollViewer.CaptureMouse();
                e.Handled = true;

            } else if (e.ChangedButton == MouseButton.Right && vm.HasItemSelection && e.OriginalSource != LibraryScrollViewer) {

                if (e.OriginalSource is FrameworkElement source) {
                    ILibraryItemViewModel sourceContext = source.DataContext as ILibraryItemViewModel;

                    if (!sourceContext.IsSelected) {
                        vm.ResetItemSelection();
                    }
                }
            }
        }

        private void LibraryScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {

                if (IsDragging) {
                    IsDragging = false;
                    DragSelection.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }

                if (LeftMouseButtonDown) {
                    LeftMouseButtonDown = false;
                    LibraryScrollViewer.ReleaseMouseCapture();
                    e.Handled = true;
                }
            }
        }

        private void LibraryScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDragging) {
                Point mousePos = e.GetPosition(LibraryScrollViewer);
                UpdateDragSelection(mousePos);
                e.Handled = true;

            } else if (LeftMouseButtonDown) {
                Point mousePos = e.GetPosition(LibraryScrollViewer);
                var dragDelta = mousePos - SelectionAnchor;
                double dragDistance = Math.Abs(dragDelta.Length);

                if (dragDistance > DragThreshold) {
                    IsDragging = true;
                    UpdateDragSelection(mousePos);
                    DragSelection.Visibility = Visibility.Visible;
                }
                
                e.Handled = true;
            }
        }

        private void UpdateDragSelection(Point mousePos)
        {
            if (IsDragging) {
                //limit the mouse area to the browsing area
                mousePos.X = Math.Min(LibraryScrollViewer.ActualWidth, mousePos.X);
                mousePos.Y = Math.Min(LibraryScrollViewer.ActualHeight, mousePos.Y);

                //Update the selection rectangle
                double x = Math.Max(0, mousePos.X);
                double y = Math.Max(0, mousePos.Y);
                double width = Math.Abs(x - SelectionAnchor.X);
                double height = Math.Abs(y - SelectionAnchor.Y);

                DragSelectionRect.SetValue(Canvas.LeftProperty, Math.Min(x, SelectionAnchor.X));
                DragSelectionRect.SetValue(Canvas.TopProperty, Math.Min(y, SelectionAnchor.Y));
                DragSelectionRect.Width = width;
                DragSelectionRect.Height = height;

                //Update the item selection
                int i = 0;
                DragRect.X = Math.Min(x, SelectionAnchor.X);
                DragRect.Y = Math.Min(y, SelectionAnchor.Y);
                DragRect.Width = width;
                DragRect.Height = height;
                
                foreach (var child in WrapSelectionPanel.Children) {

                    if (child is ContentPresenter item) {
                        object thumbnailControl = VisualTreeHelper.GetChild(item, 0);
                        ILibraryItemViewModel itemContext = item.DataContext as ILibraryItemViewModel;
                        
                        double ItemWidth = 0;
                        double ItemHeight = 0;
                        
                        if (thumbnailControl is ImageThumbnail imageThumb) {
                            ItemWidth = imageThumb.ActualWidth;
                            ItemHeight = imageThumb.ActualHeight;
                            ItemPos = imageThumb.TransformToAncestor(LibraryScrollViewer).Transform(Origin);

                        } else if (thumbnailControl is MusicThumbnail musicThumb) {
                            ItemWidth = musicThumb.ActualWidth;
                            ItemHeight = musicThumb.ActualHeight;
                            ItemPos = musicThumb.TransformToAncestor(LibraryScrollViewer).Transform(Origin);
                        }

                        ItemRectangles[i].X = ItemPos.X;
                        ItemRectangles[i].Y = ItemPos.Y;
                        ItemRectangles[i].Width = ItemWidth;
                        ItemRectangles[i].Height = ItemHeight - 7;

                        if (DragRect.IntersectsWith(ItemRectangles[i])) {
                            itemContext.IsSelected = true;
                        } else {
                            itemContext.IsSelected = false;
                        }

                        i++;
                    }
                }
            }
        }

        private void MediaScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.OriginalSource == MediaScrollViewer) {
                LeftMouseButtonDown = true;
                SelectionAnchor = e.GetPosition(MediaScrollViewer);

                //Find the wrap panel of the media items
                var border = VisualTreeHelper.GetChild(MediaItemControl, 0);
                var itemPresenter = VisualTreeHelper.GetChild(border, 0);
                WrapSelectionPanel = VisualTreeHelper.GetChild(itemPresenter, 0) as WrapPanel;
                ItemRectangles = new Rect[WrapSelectionPanel.Children.Count];

                for (int i = 0; i < ItemRectangles.Length; i++) {
                    ItemRectangles[i] = new Rect(0, 0, 10, 10);
                }

                vm.ResetItemSelection();

                //Force mouse event forwarding to the selection control
                MediaScrollViewer.CaptureMouse();
                e.Handled = true;

            } else if (e.ChangedButton == MouseButton.Right && vm.HasItemSelection && e.OriginalSource != MediaScrollViewer) {

                if (e.OriginalSource is FrameworkElement source) {
                    IThumbnailViewModel sourceContext = source.DataContext as IThumbnailViewModel;

                    if (!sourceContext.IsSelected) {
                        vm.ResetItemSelection();
                    }
                }
            }
        }

        private void MediaScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {

                if (IsDragging) {
                    IsDragging = false;
                    DragSelection.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }

                if (LeftMouseButtonDown) {
                    LeftMouseButtonDown = false;
                    MediaScrollViewer.ReleaseMouseCapture();
                    e.Handled = true;
                }
            }
        }

        private void MediaScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDragging) {
                Point mousePos = e.GetPosition(MediaScrollViewer);
                UpdateFileSystemSelection(mousePos);
                e.Handled = true;

            } else if (LeftMouseButtonDown) {
                Point mousePos = e.GetPosition(MediaScrollViewer);
                var dragDelta = mousePos - SelectionAnchor;
                double dragDistance = Math.Abs(dragDelta.Length);

                if (dragDistance > DragThreshold) {
                    IsDragging = true;
                    UpdateFileSystemSelection(mousePos);
                    DragSelection.Visibility = Visibility.Visible;
                }

                e.Handled = true;
            }
        }

        private void UpdateFileSystemSelection(Point mousePos)
        {
            if (IsDragging) {
                //limit the mouse area to the browsing area
                mousePos.X = Math.Min(MediaScrollViewer.ActualWidth, mousePos.X);
                mousePos.Y = Math.Min(MediaScrollViewer.ActualHeight, mousePos.Y);

                //Update the selection rectangle
                double x = Math.Max(0, mousePos.X);
                double y = Math.Max(0, mousePos.Y);
                double width = Math.Abs(x - SelectionAnchor.X);
                double height = Math.Abs(y - SelectionAnchor.Y);

                DragSelectionRect.SetValue(Canvas.LeftProperty, Math.Min(x, SelectionAnchor.X));
                DragSelectionRect.SetValue(Canvas.TopProperty, Math.Min(y, SelectionAnchor.Y));
                DragSelectionRect.Width = width;
                DragSelectionRect.Height = height;

                //Update the item selection
                int i = 0;
                DragRect.X = Math.Min(x, SelectionAnchor.X);
                DragRect.Y = Math.Min(y, SelectionAnchor.Y);
                DragRect.Width = width;
                DragRect.Height = height;

                foreach (var child in WrapSelectionPanel.Children) {

                    if (child is ContentPresenter item) {
                        object thumbnailControl = VisualTreeHelper.GetChild(item, 0);
                        IThumbnailViewModel itemContext = item.DataContext as IThumbnailViewModel;

                        double ItemWidth = 0;
                        double ItemHeight = 0;

                        if (thumbnailControl is DirectoryThumbnail directoryThumb) {
                            ItemWidth = directoryThumb.ActualWidth;
                            ItemHeight = directoryThumb.ActualHeight;
                            ItemPos = directoryThumb.TransformToAncestor(WrapSelectionPanel).Transform(Origin);
                            i++; continue;

                        } else if (thumbnailControl is ImageThumbnail imageThumb) {
                            ItemWidth = imageThumb.ActualWidth;
                            ItemHeight = imageThumb.ActualHeight;
                            ItemPos = imageThumb.TransformToAncestor(WrapSelectionPanel).Transform(Origin);

                        } else if (thumbnailControl is MusicThumbnail musicThumb) {
                            ItemWidth = musicThumb.ActualWidth;
                            ItemHeight = musicThumb.ActualHeight;
                            ItemPos = musicThumb.TransformToAncestor(WrapSelectionPanel).Transform(Origin);
                        }

                        ItemRectangles[i].X = ItemPos.X;
                        ItemRectangles[i].Y = ItemPos.Y;
                        ItemRectangles[i].Width = ItemWidth;
                        ItemRectangles[i].Height = ItemHeight - 7;

                        if (DragRect.IntersectsWith(ItemRectangles[i])) {
                            itemContext.IsSelected = true;
                        } else {
                            itemContext.IsSelected = false;
                        }

                        i++;
                    }
                }
            }
        }

        private void HandleStartupArguments()
        {
            if (StartupOptions.HasOptions) {
                if (StartupOptions.MusicPaths.Count > 0) {
                    //Play the first music item
                    vm.LaunchMusicPlayback.Execute(new MediaMusic(StartupOptions.MusicPaths[0]));
                }

                if (StartupOptions.VideoPaths.Count > 0 && StartupOptions.MusicPaths.Count == 0) {
                    //Play the first video item
                    vm.LaunchVideoPlayback.Execute(new MediaVideo(StartupOptions.VideoPaths[0]));
                }

                if (StartupOptions.ImagePaths.Count > 0 && StartupOptions.VideoPaths.Count == 0) {
                    //Browse to the directory of the first image
                    vm.BrowseFileSystem.Execute(GetParentPath(StartupOptions.ImagePaths[0]));

                    if (StartupOptions.ImagePaths.Count == 1) {
                        //Single image provided
                        MediaImage firstImage = vm.FileSystemItems
                            .Select(item => item.Proxy)
                            .Where(item => item.Path == StartupOptions.ImagePaths[0])
                            .Cast<MediaImage>()
                            .First();

                        vm.LaunchPresenter.Execute(firstImage);
                        
                    } else {
                        //Multiple images provided
                        vm.ActiveImages = new MediaImage[StartupOptions.ImagePaths.Count];

                        for (int i = 0; i < vm.ActiveImages.Length; i++) {
                            vm.ActiveImages[i] = new MediaImage(StartupOptions.ImagePaths[i]);
                        }

                        vm.PresentImage.Execute(new MediaImage(StartupOptions.ImagePaths[0]));
                    }

                } else if (StartupOptions.DirectoryPaths.Count == 1) {
                    //Single directory and no images provided
                    vm.BrowseFileSystem.Execute(StartupOptions.DirectoryPaths[0]);
                }
            }
        }

        private void HandleStartupLayout()
        {
            var startupOptions = vm.ImagePresentActive ? MainViewModel.Config.PresenterStartup : MainViewModel.Config.NormalStartup;

            switch (startupOptions) {
                case WindowStartupState.Fullscreen:
                    ChangeFullscreenMode(true);
                    break;

                case WindowStartupState.Maximised:
                    this.WindowState = WindowState.Maximized;
                    break;

                case WindowStartupState.RestoreSizeCentered:
                    this.Width = MainViewModel.Config.RestoreWidth;
                    this.Height = MainViewModel.Config.RestoreHeight;
                    this.RestoreWindowState();
                    break;

                case WindowStartupState.RestoreAll:
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    this.Width = MainViewModel.Config.RestoreWidth;
                    this.Height = MainViewModel.Config.RestoreHeight;
                    this.Left = MainViewModel.Config.RestoreLeft;
                    this.Top = MainViewModel.Config.RestoreTop;
                    this.RestoreWindowState();
                    break;
            }

            Window_StateChanged(this, null);
        }

        private void RestoreWindowState()
        {
            switch (MainViewModel.Config.RestoreState) {
                case WindowRestoreState.Fullscreen:
                    ChangeFullscreenMode(true);
                    break;

                case WindowRestoreState.Maximised:
                    this.WindowState = WindowState.Maximized;
                    break;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            vm.loadThumbsInterrupt = true;
            vm.StopPreloadingThreads();
            MainViewModel.StorageEngine?.Dispose();

            //Save the current window layout when restoring is active or volume has changed
            if (MainViewModel.Config.NormalStartup == WindowStartupState.RestoreAll || MainViewModel.Config.NormalStartup == WindowStartupState.RestoreSizeCentered ||
                MainViewModel.Config.PresenterStartup == WindowStartupState.RestoreAll || MainViewModel.Config.PresenterStartup == WindowStartupState.RestoreSizeCentered ||
                !MainViewModel.Config.Volume.Equals(VolumeSlider.Value)) {

                ConfigData config = MainViewModel.Config;
                WindowRestoreState restoreState = WindowRestoreState.Default;

                if (this.WindowState == WindowState.Maximized) {
                    restoreState = WindowRestoreState.Maximised;
                }

                if (Fullscreen) {
                    restoreState = WindowRestoreState.Fullscreen;
                }

                ConfigProvider.SaveConfig(new ConfigData(config.Username, config.Language, config.NormalStartup, config.PresenterStartup, restoreState,
                                                         this.Width, this.Height, this.Left, this.Top, config.ThumbnailDecodeSize, config.PreloadPresenterImages,
                                                         config.ImageLaunchBehavior, config.MusicLaunchBehavior, config.VideoLaunchBehavior, VolumeSlider.Value,
                                                         config.ShowTimelineSideLabels, config.RestoreNavExpansions, config.DirectLibraryItemDelete, config.ShowMediaContainers,
                                                         config.UseThumbnailCache, config.ItemSortMode, config.ItemSortOrder));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //The ItemPanelTemplate of MediaItemControl is loaded after the zoom is set.
            //The EventTrigger of the scale animation will miss the first UpdateTarget-Event.
            //This is fixed by firing the UpdateTarget-Event or changing the zoom again:
            try {
                //Filesystem browsing
                Border border = VisualTreeHelper.GetChild(this.MediaItemControl, 0) as Border;
                ItemsPresenter itemsPresenter = VisualTreeHelper.GetChild(border, 0) as ItemsPresenter;
                WrapPanel wrapPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as WrapPanel;

                DoubleAnimation animationX = wrapPanel.FindName("thumbScaleAnimationX") as DoubleAnimation;
                DoubleAnimation animationY = wrapPanel.FindName("thumbScaleAnimationY") as DoubleAnimation;
                
                BindingOperations.GetBindingExpression(animationX, DoubleAnimation.ToProperty).UpdateTarget();
                BindingOperations.GetBindingExpression(animationY, DoubleAnimation.ToProperty).UpdateTarget();


                //Library browsing
                border = VisualTreeHelper.GetChild(this.LibraryItemControl, 0) as Border;
                itemsPresenter = VisualTreeHelper.GetChild(border, 0) as ItemsPresenter;
                wrapPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as WrapPanel;

                animationX = wrapPanel.FindName("thumbScaleAnimationX") as DoubleAnimation;
                animationY = wrapPanel.FindName("thumbScaleAnimationY") as DoubleAnimation;

                BindingOperations.GetBindingExpression(animationX, DoubleAnimation.ToProperty).UpdateTarget();
                BindingOperations.GetBindingExpression(animationY, DoubleAnimation.ToProperty).UpdateTarget();
            } catch {
                vm.Zoom = 0.0;
                vm.Zoom = 1.0;
            }
        }

        #region GlobalHotkeys

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private WinInterop.HwndSource _source;
        const int WM_HOTKEY = 0x0312;

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(WindowProc);
            _source = null;
            UnregisterHotKeys();
            base.OnClosed(e);
        }

        private void RegisterHotKeys()
        {
            var helper = new WinInterop.WindowInteropHelper(this);
            const uint VK_MEDIA_NEXT_TRACK = 0xB0;
            const uint VK_MEDIA_PREV_TRACK = 0xB1;
            const uint VK_MEDIA_STOP = 0xB2;
            const uint VK_MEDIA_PLAY_PAUSE = 0xB3;

            if (!RegisterHotKey(helper.Handle, 0, 0x0000, VK_MEDIA_NEXT_TRACK)) {
                Debug.WriteLine("RegisterHotKey failed: VK_MEDIA_NEXT_TRACK");
            }

            if (!RegisterHotKey(helper.Handle, 1, 0x0000, VK_MEDIA_PREV_TRACK)) {
                Debug.WriteLine("RegisterHotKey failed: VK_MEDIA_PREV_TRACK");
            }

            if (!RegisterHotKey(helper.Handle, 2, 0x0000, VK_MEDIA_STOP)) {
                Debug.WriteLine("RegisterHotKey failed: VK_MEDIA_STOP");
            }

            if (!RegisterHotKey(helper.Handle, 3, 0x0000, VK_MEDIA_PLAY_PAUSE)) {
                Debug.WriteLine("RegisterHotKey failed: VK_MEDIA_PLAY_PAUSE");
            }
        }

        private void UnregisterHotKeys()
        {
            var helper = new WinInterop.WindowInteropHelper(this);

            for (int i = 0; i < 4; i++) {
                if (UnregisterHotKey(helper.Handle, i)) {
                    Debug.WriteLine("UnregisterHotKey failed: " + i.ToString());
                }
            }
        }

        #endregion
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

            //Global hotkey register
            var helper = new WinInterop.WindowInteropHelper(this);
            _source = WinInterop.HwndSource.FromHwnd(helper.Handle);
            RegisterHotKeys();
        }
        
        private System.IntPtr WindowProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            switch (msg) {
                //Fired when the size or position of the window is about to change
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;

                //Global registered hotkey pressed
                case WM_HOTKEY:
                    switch (wParam.ToInt32()) {
                        case 0:
                            vm.Next.Execute(null);
                            handled = true;
                            break;
                        case 1:
                            vm.Previous.Execute(null);
                            handled = true;
                            break;
                        case 2:
                            vm.Stop.Execute(null);
                            handled = true;
                            break;
                        case 3:
                            vm.PlayMode = !vm.PlayMode;
                            handled = true;
                            break;
                    }
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