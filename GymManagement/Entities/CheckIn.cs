using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.Entities
{
    public class CheckIn
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime CheckInDate { get; set; }

        // Navigation Property for EF Core (Lazy Loading)
        public virtual Customer Customer { get; set; }

        // UI Helper property: Ignored by DB
        [NotMapped]
        public string CustomerName => Customer?.Name ?? "N/A";
    }
}
