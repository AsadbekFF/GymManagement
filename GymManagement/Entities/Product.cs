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
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        [NotMapped]
        public string StockStatus => StockQuantity < 5 ? "Low Stock" : (StockQuantity == 0 ? "Out of Stock" : "In Stock");
    }
}
