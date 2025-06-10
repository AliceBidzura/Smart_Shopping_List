using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace Shopping_List
{
    public static class StorageService
    {
        private static readonly string FilePath = "shopping_data.json";

        public static void Save(AppData data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true // красивый формат, читаемый
            });
            File.WriteAllText(FilePath, json);
        }
        // Сериализация = объект → текст
        // Десериализация = текст → объект
        public static AppData Load()
        { //Метод загружает JSON и превращает его обратно в объект AppData
            if (!File.Exists(FilePath))
            {
                return new AppData(); // если файл отсутствует — возвращаем пустые списки
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppData>(json);
        }
    }
    public class AppData
    {
        //Когда мы сериализуем/сохраняем данные, нам нужно упаковать их в один объект,
        //чтобы JsonSerializer мог это сохранить в виде цельного JSON:
        public List<Product> CurrentProducts { get; set; } = new List<Product>();
        public List<ShoppingListArchive> Archives { get; set; } = new List<ShoppingListArchive>();
    }
}
