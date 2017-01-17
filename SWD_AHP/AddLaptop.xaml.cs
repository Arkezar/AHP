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
using System.Windows.Shapes;

namespace SWD_AHP
{
    /// <summary>
    /// Interaction logic for AddLaptop.xaml
    /// </summary>
    public partial class AddLaptop : Window
    {
        public AddLaptop()
        {
            InitializeComponent();
        }

        public Laptop laptop { get; set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            laptop = new Laptop(manu.Text,model.Text,cpu.Text,gpu.Text,double.Parse(ram.Text),double.Parse(price.Text));
            this.Close();
        }

    }
}
