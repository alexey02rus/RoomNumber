using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace RoomNumber
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(ExternalCommandData commandData)
        {
            InitializeComponent();
            MainViewViewModel mvvm = new MainViewViewModel(commandData);
            mvvm.HideRequest += (s, e) => this.Hide();
            mvvm.ShowRequest += (s, e) => this.ShowDialog();
            DataContext = mvvm;
            var dp = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
            dp.AddValueChanged(tbMassege, (sender, args) =>
            {
                if (mvvm.Successfully == true)
                {
                    tbMassege.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    tbMassege.Foreground = new SolidColorBrush(Colors.Red);
                }
            });
            
        }
    }
}
