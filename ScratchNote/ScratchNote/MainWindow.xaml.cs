using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using PInvoke;
using Microsoft.UI;
using Windows.ApplicationModel;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScratchNote
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //Save file properties
        public string ScratchNote_SaveFolderPath { get; private set; }
        public string ScratchNote_SaveFolderName { get; private set; } = "ScratchNote Saves";
        public string ScratchNote_TextSaveFilePath { get; private set; } 
        public string ScratchNote_TextSaveFileName { get; private set; } = "ScratchNote.txt";

        //Lock which ensures saves only happen one at a time (between timer and exit)
        public object SaveLock { get; private set; } = new object();

        //Window Settings
        public IntPtr HWND { get; private set; }
        public AppWindow APP_WINDOW { get; private set; }

    public MainWindow()
        {
            //Set the title of app
            Title = "ScratchNote";

            //Check if the user has the Save Folder and make it if not
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            ScratchNote_SaveFolderPath = Path.Combine(userPath, ScratchNote_SaveFolderName);
            if(!Directory.Exists(ScratchNote_SaveFolderPath))
            {
                Directory.CreateDirectory(ScratchNote_SaveFolderPath);
            }

            //Check if the text file has already been made and make it if not
            ScratchNote_TextSaveFilePath = Path.Combine(ScratchNote_SaveFolderPath, ScratchNote_TextSaveFileName);
            if(!File.Exists(ScratchNote_TextSaveFilePath))
            {
                FileStream newFile = File.Create(ScratchNote_TextSaveFilePath);
                newFile.Close();
            }

            this.InitializeComponent();

            //Set icon of window (also getting the window handle and app window)
            HWND = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(HWND);
            APP_WINDOW = AppWindow.GetFromWindowId(windowId);
            APP_WINDOW.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Captionless.ico"));

            //Read the saved text file and paste its text into the Notepad TextBox
            Notepad.Text = File.ReadAllText(ScratchNote_TextSaveFilePath);

            //Initialize timer that will save every 10 seconds
            System.Timers.Timer saveTimer = new System.Timers.Timer();
            saveTimer.Interval = 10 * 1000;
            saveTimer.Elapsed += SaveTimer_Elapsed;
            saveTimer.Start();

            //React to the window changing size by changing size of textbox
            this.SizeChanged += MainWindow_SizeChanged;

            //React to the program exiting (ensure the Notepad text is saved)
            this.Closed += MainWindow_Closed;

            //Set size of window to size of the notepad
            ChangeWindowSize(Notepad.Width, Notepad.Height);
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
            double dpi = (double)User32.GetDpiForWindow(HWND);
            double scaling = dpi / 96;
            APP_WINDOW.Resize(new Windows.Graphics.SizeInt32 { Width = (int)(width * scaling), Height = (int)(height * scaling) });
        }

        //Events Handlers----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            Notepad.Width = args.Size.Width;
            Notepad.MaxWidth = args.Size.Width;
            Notepad.Height = args.Size.Height;
            Notepad.MaxHeight = args.Size.Height;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            lock(SaveLock)
            {
                string notepadText = Notepad.Text;
                File.WriteAllText(ScratchNote_TextSaveFilePath, notepadText);
            }
        }

        private void SaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (SaveLock)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    string notepadText = Notepad.Text;
                    File.WriteAllText(ScratchNote_TextSaveFilePath, notepadText);
                });
            }
        }
    }
}
