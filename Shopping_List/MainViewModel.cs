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
using System.Windows;

namespace Shopping_List
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string FilePath = "shopping_data.json";

        public ObservableCollection<Product> CurrentProducts { get; set; }
        public ObservableCollection<ShoppingListArchive> Archives { get; set; }
        public ObservableCollection<Suggestion> Suggestions { get; set; } = new ObservableCollection<Suggestion>();
        
        private ObservableCollection<string> _suggestedProducts;
        public ObservableCollection<string> SuggestedProducts
        {
            get => _suggestedProducts;
            set
            {
                _suggestedProducts = value;
                OnPropertyChanged();
            }
        }

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
                UpdateProductSuggestions(); //чтобы при каждом изменении текста — обновлялись подсказки
            }
        }

        public ICommand AddProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand CompleteListCommand { get; }
        public ICommand AddSuggestionCommand { get; }
        public ICommand RepeatListCommand { get; }
        public ICommand SelectSuggestedProductCommand { get; }


        public MainViewModel()
        {
            CurrentProducts = new ObservableCollection<Product>();
            //{new Product { Name = "Тестовый продукт", IsChecked = false }};

            Archives = new ObservableCollection<ShoppingListArchive>();
            AddProductCommand = new RelayCommand(AddProduct, CanAddProduct);
            DeleteProductCommand = new RelayCommand<Product>(DeleteProduct);
            CompleteListCommand = new RelayCommand(CompleteList, CanCompleteList);
            AddSuggestionCommand = new RelayCommand<string>(AddSuggestion);
            RepeatListCommand = new RelayCommand<ShoppingListArchive>(RepeatList);
            SelectSuggestedProductCommand = new RelayCommand<string>(product =>
            {
                NewProductName = product;
                SuggestedProducts.Clear();
            });

            // работа с файлом
            var data = StorageService.Load();
            CurrentProducts = new ObservableCollection<Product>(data.CurrentProducts);
            Archives = new ObservableCollection<ShoppingListArchive>(data.Archives);
            UpdateSuggestions(); // ← принудительный вызов

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

        //подсказки
        private void UpdateSuggestions()
        {
            Suggestions.Clear();

            var productGroups = Archives
                .SelectMany(a => a.Products.Select(p => new { p.Name, a.Date }))
                .GroupBy(p => p.Name)
                .ToList();
            //самые популярные
            var frequentProducts = Archives
                .SelectMany(a => a.Products)
                .GroupBy(p => p.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(p => p.Count)
                .ToList();

            foreach (var group in productGroups)
            {
                string name = group.Key;
                var date = group.Select(g => g.Date).OrderBy(d => d).ToList();
                int count = date.Count;

                bool inCurrent = CurrentProducts.Any(p => p.Name == name);
                if (inCurrent)
                    continue;

                // 1. Часто покупаемые (минимум 3 раза)
                if (count >= 2)
                {
                    Suggestions.Add(new Suggestion
                    {
                        ProductName = name,
                        Message = $"Вы часто покупали {name}. Добавить?"
                    });
                }

                // 2. Регулярно с интервалом (например, каждые 3–5 дней)
                if (count >= 2)
                {
                    var intervals = date.Zip(date.Skip(1), (a, b) => (b - a).TotalDays).ToList();
                    double avgInterval = intervals.Average();

                    if (avgInterval >= 3 && avgInterval <= 5)
                    {
                        Suggestions.Add(new Suggestion
                        {
                            ProductName = name,
                            Message = $"Вы покупаете {name} примерно каждые {Math.Round(avgInterval)} дня. Добавить?"
                        });
                    }
                }

                // 3. Был только один раз — возможно, понравилось?
                //if (count == 1)
                //{
                //    Suggestions.Add(new Suggestion
                //    {
                //        ProductName = name,
                //        Message = $"Вы однажды пробовали {name}. Хотите повторить?"
                //    });
                //}

                //4. продукт был в архиве, но не появляется уже более 30 дней
            }
            //5. Самый популярный товар
            var mostFrequentProduct = frequentProducts[0];
            if (mostFrequentProduct != null)
            {
                Suggestions.Add(new Suggestion
                {
                    ProductName = mostFrequentProduct.Name,
                    Message = $"Ваш любимый продукт - {mostFrequentProduct.Name}. Добавить снова?"
                });
            }
        }
        private void AddSuggestion(string productName)
        {
            if (!string.IsNullOrWhiteSpace(productName))
            {
                CurrentProducts.Add(new Product
                {
                    Name = productName,
                    IsChecked = false,
                    DateAdded = DateTime.Now
                });

                UpdateSuggestions(); // обновим подсказки после добавления
            }
        }
        private void RepeatList(ShoppingListArchive archive)
        {
            if (archive == null || archive.Products == null) return;

            foreach (var product in archive.Products)
            {
                CurrentProducts.Add(new Product
                {
                    Name = product.Name,
                    IsChecked = false,
                    DateAdded = DateTime.Now
                });
            }

            UpdateSuggestions();
        }

        //подсказки при вводе
        private void UpdateProductSuggestions()
        {
            if (string.IsNullOrWhiteSpace(NewProductName))
            {
                SuggestedProducts.Clear();
                return;
            }
            else
            {
                var allProducts = Archives.SelectMany(a => a.Products).Select(p => p.Name).Distinct().Where(n => n.StartsWith(NewProductName, StringComparison.OrdinalIgnoreCase)).Take(5);
                
                SuggestedProducts = new ObservableCollection<string>(allProducts);

                //ObservableCollection<ShoppingListArchive>, а каждый архив содержит .Products,
                //где Product.Name — строка. Значит SelectMany(a => a.Products).Select(p => p.Name)
                //даст тебе список всех продуктов из всех архивов.
                //Distinct() — чтобы не было дублей
            }
        }
    }
}
