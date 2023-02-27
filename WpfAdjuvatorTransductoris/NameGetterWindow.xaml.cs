using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfAdjuvatorTransductoris
{
    /// <summary>
    /// Interaction logic for NameGetterWindow.xaml
    /// </summary>
    public partial class NameGetterWindow : Window
    {
        static private Regex nameChecker = new Regex(@"[\W\s]");
        static private Regex extensionExtractor = new Regex(@"(?<=\.)\w+$");
        private bool isFolder { get; set; }
        public event Action<string>? NameChanged;
        private List<string> Formats { get; }
        private static readonly Regex extensionTrimmer = new Regex(@"\.\w+$");

        public NameGetterWindow(bool isFolder, List<string>? formats = null, string? initial = null)
        {
            InitializeComponent();
            this.isFolder = isFolder;
            Formats = formats?.Count > 0 ? formats : new List<string>(new[] {"uns"} );

            if (!isFolder)
            {
                EnterNameLabel.Text = "Enter file name\n" +
                                      "the file extension is specified for convenience only";
                FileFormatBox.ItemsSource = Formats;
                if (!string.IsNullOrEmpty(initial))
                {
                    var extension = extensionExtractor.Match(initial);
                    FileFormatBox.SelectedIndex = FileFormatBox.Items.IndexOf(extension.Value);
                    initial = extensionTrimmer.Replace(initial, "");
                }
            } 
            else
            {
                NameTextBlock.Width = 275;
                FileFormatBox.Visibility = Visibility.Collapsed;
            }

            Title = string.IsNullOrEmpty(initial)? 
                (isFolder? "New folder" : "New file"):
                (isFolder? "Edit folder name" : "Edit file name");

            if (string.IsNullOrEmpty(initial)) return;
            NameTextBlock.Text = initial;
        }

        public void Confirm_Executed(object sender, ExecutedRoutedEventArgs e) {
            NameChanged?.Invoke(isFolder? NameTextBlock.Text : $"{NameTextBlock.Text}.{FileFormatBox.Text}");
            Close();
        }

        public void Confirm_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = NameTextBlock.Text.Length > 0 &&
                !nameChecker.IsMatch(NameTextBlock.Text) &&
                ((isFolder) || (!isFolder && FileFormatBox.Text.Length > 0));
        }

        public void Abort_Executed(object sender, ExecutedRoutedEventArgs e) => Close();
    }
}
