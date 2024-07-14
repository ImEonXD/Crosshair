using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Crosshair
{
    public partial class MainWindow : Window
    {
        string programVersion = "1.0.1";
        string onlineVerLink = "https://pastebin.com/raw/0SFSmGLE";
        string updateLink = "https://github.com/ImEonXD/Crosshair/releases/latest";
        string tempPath;
        string versionFile;

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public MainWindow()
        {
            // Always on top
            this.Topmost = true;

            CheckForUpdates();

            // Default config values
            string defaultResX = "1920";
            string defaultResY = "1080";
            string defaultText = "X";
            string defaultColor = "#ff0000";
            string defaultSize = "24";
            string defaultWeight = "Regular";

            // Strings, Paths, and Files
            string rootPath = Directory.GetCurrentDirectory();
            string cfgPath = Path.Combine(rootPath, "cfg");
            string fileResX = Path.Combine(cfgPath, "resx.txt");
            string fileResY = Path.Combine(cfgPath, "resy.txt");
            string fileText = Path.Combine(cfgPath, "text.txt");
            string fileColor = Path.Combine(cfgPath, "color.txt");
            string fileFontSize = Path.Combine(cfgPath, "size.txt");
            string fileFontWeight = Path.Combine(cfgPath, "weight.txt");

            // Reset program to default values
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                try
                {
                    File.WriteAllText(fileResX, defaultResX);
                    File.WriteAllText(fileResY, defaultResY);
                    File.WriteAllText(fileText, defaultText);
                    File.WriteAllText(fileColor, defaultColor);
                    File.WriteAllText(fileFontSize, defaultSize);
                    File.WriteAllText(fileFontWeight, defaultWeight);

                    MessageBox.Show("All values have successfully been reset to their default values", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error resetting to default values", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Application.Current.Shutdown();
            }

            // Create config if they do not exist and
            // set them to their deafult values
            Directory.CreateDirectory(cfgPath);

            if (!File.Exists(fileResX))
            {
                File.WriteAllText(fileResX, defaultResX);
            }
            if (!File.Exists(fileResY))
            {
                File.WriteAllText(fileResY, defaultResY);
            }
            if (!File.Exists(fileText))
            {
                File.WriteAllText(fileText, defaultText);
            }
            if (!File.Exists(fileColor))
            {
                File.WriteAllText(fileColor, defaultColor);
            }
            if (!File.Exists(fileFontSize))
            {
                File.WriteAllText(fileFontSize, defaultSize);
            }
            if (!File.Exists(fileFontWeight))
            {
                File.WriteAllText(fileFontWeight, defaultWeight);
            }

            // Read all config values
            string resX = File.ReadAllText(fileResX);
            string resY = File.ReadAllText(fileResY);
            string crosshairText = File.ReadAllText(fileText);
            string color = File.ReadAllText(fileColor);
            string fontSize = File.ReadAllText(fileFontSize);
            string fontWeight = File.ReadAllText(fileFontWeight);

            InitializeComponent();

            // Convert strings and check for errors

            // ResX and ResY
            if (IsDigitsOnly(resX) == false || IsDigitsOnly(resY) == false)
            {
                MessageBox.Show($"Resoulution value in \"resx.txt\" or \"resy.txt\" must be digit value\n\nExample\nResX: {defaultResX}\nResY: {defaultResY}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            // Color
            string color2 = color.Substring(0, 1);
            if (color2 != "#")
            {
                MessageBox.Show($"Color value in \"color.txt\" must be hex value\n\nExample: {defaultColor}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            var converter = new System.Windows.Media.BrushConverter();
            var color3 = (Brush)converter.ConvertFromString(color);

            // Font Size
            if (IsDigitsOnly(fontSize) == false)
            {
                MessageBox.Show($"Size value in \"size.txt\" must be digit value\n\nExample: {defaultSize}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            // Color
            bool fontWeightSupported = false;
            if (fontWeight == "Black" || fontWeight == "Bold" || fontWeight == "Light" || fontWeight == "Medium" || fontWeight == "Regular" || fontWeight == "Thin")
            {
                fontWeightSupported = true;
            }
            if (fontWeightSupported == false)
            {
                MessageBox.Show($"Weight value in \"weight.txt\" must be supported value\n\nExample: {defaultWeight}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            var converter2 = new System.Windows.FontWeightConverter();
            var fontWeight2 = (FontWeight)converter2.ConvertFromString(fontWeight);

            // Set all config values
            this.Width = double.Parse(resX);
            this.Height = double.Parse(resY);

            CrosshairText.Text = crosshairText;
            CrosshairText.Foreground = color3;
            CrosshairText.FontSize = double.Parse(fontSize);
            CrosshairText.FontWeight = fontWeight2;
        }

        private void CheckForUpdates()
        {
            tempPath = Path.GetTempPath();
            versionFile = Path.Combine(tempPath, "CrosshairVersion.txt");

            File.WriteAllText(versionFile, programVersion);

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString(onlineVerLink));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallUpdate(true, onlineVersion);
                    }
                    else
                    {
                        try
                        {
                            File.Delete(versionFile);
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for updates:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                InstallUpdate(false, Version.zero);
            }
        }

        private void InstallUpdate(bool isUpdate, Version _onlineVersion)
        {
            tempPath = Path.GetTempPath();
            versionFile = Path.Combine(tempPath, "CrosshairVersion.txt");

            try
            {
                MessageBoxResult installUpdate = MessageBox.Show("An update has been found!\n\nWould you like to install it?", "Update Found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (installUpdate == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(versionFile);
                    }
                    catch { }

                    Process.Start(updateLink);
                    Application.Current.Shutdown();
                }
                else
                {
                    try
                    {
                        File.Delete(versionFile);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }
    }
}
