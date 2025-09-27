using GymManagement.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.Entities
{
    public class Product : ObservableObject
    {
        public int Id { get; set; }
        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }

        private decimal _price;
        // FIX: Explicitly map C# decimal to SQLite TEXT for accurate storage/retrieval
        [Column(TypeName = "TEXT")]
        public decimal Price { get => _price; set { _price = value; OnPropertyChanged(nameof(Price)); } }

        private int _stockQuantity;
        public int StockQuantity { get => _stockQuantity; set { _stockQuantity = value; OnPropertyChanged(nameof(StockQuantity)); } }

        // UI Helper property: Ignored by DB, used for display colors
        [NotMapped]
        public string StockStatus => StockQuantity < 5 ? "Low Stock" : (StockQuantity == 0 ? "Out of Stock" : "In Stock");
    }
}
