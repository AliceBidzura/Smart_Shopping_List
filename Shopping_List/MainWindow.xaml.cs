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
using System.ComponentModel;

namespace Shopping_List
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();   // подключаем ViewModel
            this.DataContext = _viewModel;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            _viewModel.Save(); // <-- сохраняем один раз при закрытии
        }
        private void Suggestion_Click(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("working");
            if (sender is ListBoxItem item && item.DataContext is string suggestion)
            {
                _viewModel.NewProductName = suggestion;
                _viewModel.SuggestedProducts.Clear();
            }
        }

    }
}
