using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.Entities
{
    public class Purchase
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
        public DateTime PurchaseDate { get; set; }

        // UI Helper properties: Ignored by DB
        [NotMapped]
        public string CustomerName => Customer?.Name ?? "N/A";
        [NotMapped]
        public string ProductName => Product?.Name ?? "N/A";
        [NotMapped]
        public decimal TotalPrice => Product != null ? Product.Price * Quantity : 0m;
    }
}
