using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.IO;
using System.Text.Json;

namespace Shopping_List
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string FilePath = "shopping_data.json";

        public ObservableCollection<Product> CurrentProducts { get; set; }
        public ObservableCollection<ShoppingListArchive> Archives { get; set; }

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
        public ICommand DeleteProductCommand { get; }
        public ICommand CompleteListCommand { get; }

        public MainViewModel()
        {
            CurrentProducts = new ObservableCollection<Product>();
            //{new Product { Name = "Тестовый продукт", IsChecked = false }};

            Archives = new ObservableCollection<ShoppingListArchive>();
            AddProductCommand = new RelayCommand(AddProduct, CanAddProduct);
            DeleteProductCommand = new RelayCommand<Product>(DeleteProduct);
            CompleteListCommand = new RelayCommand(CompleteList, CanCompleteList);

            // работа с файлом
            var data = StorageService.Load();
            CurrentProducts = new ObservableCollection<Product>(data.CurrentProducts);
            Archives = new ObservableCollection<ShoppingListArchive>(data.Archives);

            CurrentProducts.CollectionChanged += (_, __) =>
            {
                (CompleteListCommand as RelayCommand)?.RaiseCanExecuteChanged();
                //как только изменится CurrentProducts — команда CompleteListCommand проверит, можно ли снова быть активной
            };
        }
        public void Save()
        {
            StorageService.Save(new AppData
            {
                CurrentProducts = CurrentProducts.ToList(),
                Archives = Archives.ToList()
            });
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

        private void DeleteProduct(Product product)
        {
            if (product != null && CurrentProducts.Contains(product))
                CurrentProducts.Remove(product);
        }


        private bool CanAddProduct()
        {
            return !string.IsNullOrWhiteSpace(NewProductName);
        }

        private void CompleteList()
        {
            // Архивируем текущий список
            Archives.Insert(0, new ShoppingListArchive
            {
                Date = DateTime.Now,
                Products = CurrentProducts.ToList()
            });

            // Очищаем текущий список
            CurrentProducts.Clear();
        }

        private bool CanCompleteList()
        {
            return CurrentProducts.Any();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
