using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using PInvoke;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScratchNote
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            //Set the title of app
            Title = "ScratchNote";

            this.InitializeComponent();

            //Set size of window to size of the notepad
            ChangeWindowSize(Notepad.Width, Notepad.Height);

            //React to the window changing size by changing size of textbox
            this.SizeChanged += MainWindow_SizeChanged;
        }

        //Methods------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Changes the size of the window using a given width and height, taking into account the
        /// DPI of the screen
        /// </summary>
        /// <param name="width">Desired width</param>
        /// <param name="height">Desired Height</param>
        public void ChangeWindowSize(double width, double height)
        {
            //Set the size of window taking into account DPI (https://stackoverflow.com/questions/67169712/winui-3-0-reunion-0-5-window-size)
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            double dpi = (double)User32.GetDpiForWindow(hWnd);
            double scaling = dpi / 96;
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = (int)(width * scaling), Height = (int)(height * scaling) });
        }


        //Events Handlers----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            Notepad.Width = args.Size.Width;
            Notepad.MaxWidth = args.Size.Width;
            Notepad.Height = args.Size.Height;
            Notepad.MaxHeight = args.Size.Height;
        }
    }
}
