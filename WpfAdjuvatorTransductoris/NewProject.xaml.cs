using AdjuvatorTransductorumRCor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAdjuvatorTransductoris.Helpers;

namespace WpfAdjuvatorTransductoris.ViewModel
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProject : Window
    {
        public event Action<string>? Confirmed; 
        private readonly Regex _nameChecker = new Regex("^[a-zA-ZА-Яа-я0-9-_]*$");
        private readonly AppInfo appInfo = AppInfo.GetInstance();
        
        public NewProject()
        {
            InitializeComponent();
        }

        private void CreateProject_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            NewProject window = (NewProject)sender;
            if (Confirmed is not null) Confirmed(window.ProjectNameText.Text);
            window.Close();
        }

        private void CreateProject_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (sender is NewProject np)
            {
                e.CanExecute = 
                    ProjectNameText.Text.Length > 2 && 
                    _nameChecker.IsMatch(ProjectNameText.Text) &&
                    !appInfo.ProjectInfos.Select(pi => pi.Name).Contains(ProjectNameText.Text);
            }
        }
    }
}
