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

namespace RottenTomatoesClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ComboAxisX.SelectedIndex = ComboAxisX.Items.Count - 1;
            ComboAxisY.SelectedIndex = ComboAxisY.Items.Count - 2;
            ComboBar.SelectedIndex = ComboBar.Items.Count - 1;
        }
        public void EnableButtons(object sender, MouseButtonEventArgs e)
        {
            UpdateButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
             
        }
    }
}
