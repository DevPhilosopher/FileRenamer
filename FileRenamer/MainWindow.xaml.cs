// using System.Text;
// using System.Windows;
// using System.Windows.Controls;
// using System.Windows.Data;
// using System.Windows.Documents;
// using System.Windows.Input;
// using System.Windows.Media;
// using System.Media;
// using System.Windows.Media.Imaging;
// using System.Windows.Navigation;
// using System.Windows.Shapes;

using System;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;


namespace FileRenamer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderPathBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void ApplySelected_Click(object sender, RoutedEventArgs e)
        {
            RenameFiles(selectedOnly: true);
            SystemSounds.Asterisk.Play();
            System.Windows.MessageBox.Show("Selected Options Applied", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyAll_Click(object sender, RoutedEventArgs e)
        {
            CapitalizeWordsOption.IsChecked = true;
            TrimSpacesOption.IsChecked = true;
            RemoveUnderscoresOption.IsChecked = true;
            CustomPatternOption.IsChecked = true;

            RenameFiles(selectedOnly: false);

            CapitalizeWordsOption.IsChecked = false;
            TrimSpacesOption.IsChecked = false;
            RemoveUnderscoresOption.IsChecked = false;
            CustomPatternOption.IsChecked = false;

            SystemSounds.Asterisk.Play();
            System.Windows.MessageBox.Show("All Options Applied", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RenameFiles(bool selectedOnly)
        {
            string folderPath = FolderPathBox.Text.Trim();
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                System.Windows.MessageBox.Show("Please select a valid folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string customPattern = CustomPatternInput.Text.Trim();

            foreach (string file in Directory.GetFiles(folderPath))
            {
                string fileName = Path.GetFileName(file);
                string newFileName = CleanFileName(fileName, selectedOnly, customPattern);
                string newPath = Path.Combine(folderPath, newFileName);
                if (file != newPath)
                {
                    File.Move(file, newPath);
                }
            }
        }

        private string CleanFileName(string filename, bool selectedOnly, string pattern)
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);

            // Apply options
            if (!selectedOnly || (CapitalizeWordsOption.IsChecked ?? false))
            {
                name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            }

            if (!selectedOnly || (TrimSpacesOption.IsChecked ?? false))
            {
                name = Regex.Replace(name, @"\s+", " ").Trim();
            }

            if (!selectedOnly || (RemoveUnderscoresOption.IsChecked ?? false))
            {
                name = name.Replace("_", " ");
            }

            if ((!selectedOnly || (CustomPatternOption.IsChecked ?? false)) && !string.IsNullOrWhiteSpace(pattern))
            {
                name = ApplyCustomPattern(name, pattern);
            }

            return name + ext;
        }

        private string ApplyCustomPattern(string name, string pattern)
        {
            try
            {
                if (pattern.StartsWith("*") && pattern.Length > 1)
                {
                    string suffix = pattern.Substring(1);
                    int index = name.IndexOf(suffix);
                    if (index >= 0)
                        name = name.Substring(index + suffix.Length);
                }
                else if (pattern.EndsWith("*") && pattern.Length > 1)
                {
                    string prefix = pattern.Substring(0, pattern.Length - 1);
                    int index = name.IndexOf(prefix);
                    if (index >= 0)
                        name = name.Substring(0, index);
                }
                else
                {
                    name = name.Replace(pattern, "");
                }
            }
            catch
            {
                // Optional: handle pattern parsing errors
            }

            return name;
        }

        private void CustomPatternOption_Checked(object sender, RoutedEventArgs e)
        {
            CustomPatternPanel.Visibility = Visibility.Visible;
        }

        private void CustomPatternOption_Unchecked(object sender, RoutedEventArgs e)
        {
            CustomPatternPanel.Visibility = Visibility.Collapsed;
        }
    }
}
