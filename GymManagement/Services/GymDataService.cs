using GymManagement.Entities.Context;
using GymManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using GymManagement.Models;
using System.Configuration;

namespace GymManagement.Services
{
    public class GymDataService
    {
        // Helper method to create a new DbContext instance
        private GymDbContext CreateContext()
        {
            return new GymDbContext();
        }

        // =========================================================================
        // INITIALIZATION AND SEEDING
        // =========================================================================

        public void InitializeDatabase()
        {
            using (var context = CreateContext())
            {
                // Note: Database.Migrate() is handled in MainWindow.xaml.cs, 
                // but we use EnsureCreated here to check if tables exist for seeding.

                // Check if any customers exist before seeding
                if (!context.Customers.Any())
                {
                    // === 1. Seed Customers (Must be saved first to get IDs) ===
                    var alice = new Customer { Name = "Alice Johnson", Phone = "555-1001", Email = "alice@gym.com" };
                    var bob = new Customer { Name = "Bob Smith", Phone = "555-1002", Email = "bob@gym.com" };
                    var charlie = new Customer { Name = "Charlie Davis", Phone = "555-1003", Email = "charlie@gym.com" };

                    context.Customers.AddRange(alice, bob, charlie);
                    context.SaveChanges(); // CRITICAL: Forces ID generation

                    // === 2. Seed Products (Must be saved second to get IDs) ===
                    var protein = new Product { Name = "Protein Powder (2kg)", Price = 49.99m, StockQuantity = 15 };
                    var bottle = new Product { Name = "Water Bottle (1L)", Price = 12.50m, StockQuantity = 3 }; // Low stock
                    var towel = new Product { Name = "Gym Towel", Price = 7.00m, StockQuantity = 0 }; // Out of stock

                    context.Products.AddRange(protein, bottle, towel);
                    context.SaveChanges(); // CRITICAL: Forces ID generation

                    // === 3. Seed Check-Ins (Uses generated IDs) ===
                    context.CheckIns.Add(new CheckIn { CustomerId = alice.Id, CheckInDate = DateTime.Now.AddDays(-1) });
                    context.CheckIns.Add(new CheckIn { CustomerId = alice.Id, CheckInDate = DateTime.Now.AddDays(-1).AddHours(2) }); // Alice visited twice
                    context.CheckIns.Add(new CheckIn { CustomerId = bob.Id, CheckInDate = DateTime.Now.AddDays(-2) });

                    // === 4. Seed Purchases (Uses generated IDs and reduces stock) ===
                    ProcessPurchase(context, bob.Id, bottle.Id, 1);
                    ProcessPurchase(context, charlie.Id, protein.Id, 2);

                    // Note: We don't need a final SaveChanges() here as ProcessPurchase handles its own transaction.
                }
            }
        }

        // =========================================================================
        // CORE CRUD OPERATIONS
        // =========================================================================

        public List<Customer> GetCustomers()
        {
            using (var context = CreateContext())
            {
                return context.Customers.ToList();
            }
        }

        public void AddCustomer(Customer customer)
        {
            using (var context = CreateContext())
            {
                context.Customers.Add(customer);
                context.SaveChanges();
            }
        }

        public void UpdateCustomer(Customer customer)
        {
            using (var context = CreateContext())
            {
                // Use Attach to update existing entity tracked by EF
                context.Customers.Attach(customer).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public List<Product> GetProducts()
        {
            using (var context = CreateContext())
            {
                return context.Products.ToList();
            }
        }

        public void AddProduct(Product product)
        {
            using (var context = CreateContext())
            {
                context.Products.Add(product);
                context.SaveChanges();
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var context = CreateContext())
            {
                context.Products.Attach(product).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        // =========================================================================
        // ATTENDANCE & TRANSACTIONS
        // =========================================================================

        public void AddCheckIn(CheckIn checkIn)
        {
            using (var context = CreateContext())
            {
                context.CheckIns.Add(checkIn);
                context.SaveChanges();
            }
        }

        public List<CheckIn> GetCheckIns()
        {
            using (var context = CreateContext())
            {
                // CRITICAL: Eager load Customer for the display name
                return context.CheckIns
                    .Include(ci => ci.Customer)
                    .OrderByDescending(ci => ci.CheckInDate)
                    .ToList();
            }
        }

        public List<Purchase> GetPurchases()
        {
            using (var context = CreateContext())
            {
                // CRITICAL: Eager load Customer and Product for display names and price
                return context.Purchases
                    .Include(p => p.Customer)
                    .Include(p => p.Product)
                    .OrderByDescending(p => p.PurchaseDate)
                    .Select(p => new Purchase
                    {
                        Id = p.Id,
                        CustomerId = p.CustomerId,
                        Customer = p.Customer,
                        Product = p.Product,
                        ProductId = p.ProductId,
                        Quantity = p.Quantity,
                        PurchaseDate = p.PurchaseDate
                    })
                    .ToList();
            }
        }

        public bool ProcessPurchase(int customerId, int productId, int quantity)
        {
            using (var context = CreateContext())
            {
                // Use the helper method for transaction safety during runtime sales
                return ProcessPurchase(context, customerId, productId, quantity);
            }
        }

        // Private method to handle the transaction locally (used by runtime and seeding)
        private bool ProcessPurchase(GymDbContext context, int customerId, int productId, int quantity)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var product = context.Products.FirstOrDefault(p => p.Id == productId);

                    if (product == null || product.StockQuantity < quantity)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    // 1. Decrease stock
                    product.StockQuantity -= quantity;
                    context.Products.Update(product);

                    // 2. Record purchase
                    var purchase = new Purchase
                    {
                        CustomerId = customerId,
                        ProductId = productId,
                        Quantity = quantity,
                        PurchaseDate = DateTime.Now
                    };
                    context.Purchases.Add(purchase);

                    context.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        // =========================================================================
        // CHART DATA AGGREGATION
        // =========================================================================

        public List<AttendanceDataPoint> GetCustomerAttendanceData()
        {
            using (var context = CreateContext())
            {
                return context.CheckIns
                    .Include(ci => ci.Customer)
                    .GroupBy(ci => ci.Customer.Name)
                    .Select(g => new AttendanceDataPoint
                    {
                        CustomerName = g.Key,
                        CheckInCount = g.Count()
                    })
                    .OrderByDescending(dp => dp.CheckInCount)
                    .Take(10) // Limit to top 10 for chart clarity
                    .ToList();
            }
        }

        public List<ProductSalesDataPoint> GetProductSalesData()
        {
            using (var context = CreateContext())
            {
                return context.Purchases
                    .Include(p => p.Product)
                    .GroupBy(p => p.Product.Name)
                    .Select(g => new ProductSalesDataPoint
                    {
                        ProductName = g.Key,
                        TotalQuantitySold = g.Sum(p => p.Quantity)
                    })
                    .OrderByDescending(dp => dp.TotalQuantitySold)
                    .ToList();
            }
        }
    }
}
