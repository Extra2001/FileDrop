using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WinRT.Interop;
using WinRT;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using FileDrop.Helpers.WiFiDirect;
using FileDrop.Helpers.WiFiDirect.Advertiser;
using FileDrop.Helpers.WiFiDirect.Connector;
using FileDrop.Helpers.TransferHelper.Reviever;

namespace FileDrop.Helpers
{
    public class MainWindowConfigurator
    {
        private MainWindow mainWindow;
        private WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        private AppWindow m_AppWindow;
        private MicaController m_backdropController;
        private DesktopAcrylicController m_AbackdropController;
        private SystemBackdropConfiguration m_configurationSource;
        private TextBlock AppTitle;
        public MainWindowConfigurator(MainWindow mainWindow, TextBlock AppTitle)
        {
            this.mainWindow = mainWindow;
            this.AppTitle = AppTitle;
        }
        public void Configure(Border customTitleBar)
        {
            mainWindow.Closed += MainWindow_Closed;
            mainWindow.Activated += MainWindow_Activated;
            App.mainWindow = mainWindow;
            SetCustomTitleBar(customTitleBar);
            TrySetSystemBackdrop();
        }

        #region 设置应用背景
        private bool TrySetSystemBackdrop()
        {
            if (MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
                // Create the policy object.
                m_configurationSource = new SystemBackdropConfiguration();
                mainWindow.Activated += Window_Activated;
                mainWindow.Closed += Window_Closed;
                ((FrameworkElement)mainWindow.Content).ActualThemeChanged += Window_ThemeChanged;
                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();
                m_backdropController = new MicaController() { Kind = MicaKind.Base };
                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_backdropController.
                    AddSystemBackdropTarget(mainWindow.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // succeeded
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
                m_configurationSource = new SystemBackdropConfiguration();
                mainWindow.Activated += Window_Activated;
                mainWindow.Closed += Window_Closed;
                ((FrameworkElement)mainWindow.Content).ActualThemeChanged += Window_ThemeChanged;
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();
                m_AbackdropController = new DesktopAcrylicController();
                m_AbackdropController.
                    AddSystemBackdropTarget(mainWindow.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_AbackdropController.SetSystemBackdropConfiguration(m_configurationSource);
                m_AbackdropController.LuminosityOpacity = 0.9f;
                m_AbackdropController.TintColor = Colors.AliceBlue;
                m_AbackdropController.TintOpacity = 0.6f;
                return true; // succeeded
            }
            return false; // Mica is not supported on this system
        }
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed
            // so it doesn't try to use this closed window.
            if (m_backdropController != null)
            {
                m_backdropController.Dispose();
                m_backdropController = null;
            }
            mainWindow.Activated -= Window_Activated;
            m_configurationSource = null;
        }
        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }
        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)mainWindow.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
            }
        }
        #endregion
        private void SetCustomTitleBar(Border customTitleBar)
        {
            m_AppWindow = GetAppWindowForCurrentWindow();
            // Check to see if customization is supported.
            // Currently only supported on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = m_AppWindow.TitleBar;
                // Hide default title bar.
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else
            {
                // Title bar customization using these APIs is currently
                // supported only on Windows 11. In other cases, hide
                // the custom title bar element.
                mainWindow.ExtendsContentIntoTitleBar = true;
                mainWindow.SetTitleBar(customTitleBar);
            }
            mainWindow.Title = "FileDrop";
            // SetIcon("Assets/Logos/WindowLogo.ico");
        }
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(mainWindow);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }
        private void SetIcon(string filePath)
        {
            IntPtr windowHandle = WindowNative.GetWindowHandle(mainWindow);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, filePath));
        }
        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                AppTitle.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
            }
            else
            {
                AppTitle.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
            }
        }
        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            Repo.SaveAndClose();
            if (WiFiDirectAdvertiser.Started)
                WiFiDirectAdvertiser.StopAdvertisement();
            WiFiDirectConnector.StopWatcher();
            WiFiDirectAdvertiser.CloseDevice();
            WiFiDirectConnector.StopWatcher();
            WiFiDirectConnector.CloseDevice();
            RecieveTask.StopWaitForTransfer();
        }

        private class WindowsSystemDispatcherQueueHelper
        {
            [StructLayout(LayoutKind.Sequential)]
            struct DispatcherQueueOptions
            {
                internal int dwSize;
                internal int threadType;
                internal int apartmentType;
            }

            [DllImport("CoreMessaging.dll")]
            private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

            object m_dispatcherQueueController = null;
            public void EnsureWindowsSystemDispatcherQueueController()
            {
                if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
                {
                    // one already exists, so we'll just use it.
                    return;
                }

                if (m_dispatcherQueueController == null)
                {
                    DispatcherQueueOptions options;
                    options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                    options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                    options.apartmentType = 2; // DQTAT_COM_STA

                    CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
                }
            }
        }
    }
}
