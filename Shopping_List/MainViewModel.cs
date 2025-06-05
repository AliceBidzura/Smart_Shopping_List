using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Shopping_List
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Product> CurrentProducts { get; set; }

        private string _newProductName;
        public string NewProductName 
        {
            get => _newProductName;
            set
            {
                _newProductName = value;
                OnPropertyChanged();

                // Обновляем доступность команды
                (AddProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddProductCommand { get; }

        public MainViewModel()
        {
            CurrentProducts = new ObservableCollection<Product>();
            //{new Product { Name = "Тестовый продукт", IsChecked = false }};
            AddProductCommand = new RelayCommand(AddProduct, CanAddProduct);
        }

        private void AddProduct()
        {
            CurrentProducts.Add(new Product
            {
                Name = NewProductName,
                IsChecked = false,
                DateAdded = DateTime.Now
            });
            NewProductName = ""; // очистить поле ввода
        }

        private bool CanAddProduct()
        {
            return !string.IsNullOrWhiteSpace(NewProductName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
