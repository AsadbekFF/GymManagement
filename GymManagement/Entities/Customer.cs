using GymManagement.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.Entities
{
    public class Customer : ObservableObject
    {
        // Primary Key
        public int Id { get; set; }
        public string Name { get; set; }

        public string? Phone { get; set; }
    }
}
