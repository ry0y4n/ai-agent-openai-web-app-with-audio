using System.Collections.Generic;

namespace dotnetfashionassistant.Models
{    public class InventoryItem
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public Dictionary<string, int> SizeInventory { get; set; }
        public decimal Price { get; set; }

        public InventoryItem()
        {
            SizeInventory = new Dictionary<string, int>();
            Price = 29.99m; // Default price if not specified
        }
    }

    public static class InventoryService
    {
        private static readonly List<string> Sizes = new List<string> { "XS", "S", "M", "L", "XL", "XXL", "XXXL" };

        public static List<InventoryItem> GetInventory()
        {
            // Create dummy inventory data
            var inventory = new List<InventoryItem>();
              // Navy Formal Blazer
            var blazer = new InventoryItem
            {
                ProductId = 3,
                ProductName = "Navy Single-Breasted Slim Fit Formal Blazer",
                Price = 89.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "XS", 0 },
                    { "S", 0 },
                    { "M", 0 },
                    { "L", 0 },
                    { "XL", 0 },
                    { "XXL", 0 },
                    { "XXXL", 0 }
                }
            };
              // White & Navy Blue Shirt
            var whiteNavyShirt = new InventoryItem
            {
                ProductId = 111,
                ProductName = "White & Navy Blue Slim Fit Printed Casual Shirt",
                Price = 34.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "XS", 8 },
                    { "S", 15 },
                    { "M", 0 },
                    { "L", 18 },
                    { "XL", 12 },
                    { "XXL", 0 },
                    { "XXXL", 4 }
                }
            };
              // Red Checked Shirt
            var redShirt = new InventoryItem
            {
                ProductId = 116,
                ProductName = "Red Slim Fit Checked Casual Shirt",
                Price = 39.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "XS", 10 },
                    { "S", 18 },
                    { "M", 22 },
                    { "L", 16 },
                    { "XL", 14 },
                    { "XXL", 7 },
                    { "XXXL", 5 }
                }
            };
              // Navy Blue Denim Jacket
            var denimJacket = new InventoryItem
            {
                ProductId = 10,
                ProductName = "Navy Blue Washed Denim Jacket",
                Price = 59.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "XS", 6 },
                    { "S", 14 },
                    { "M", 19 },
                    { "L", 17 },
                    { "XL", 11 },
                    { "XXL", 9 },
                    { "XXXL", 4 }
                }
            };
            
            // Organic Fresh Strawberries
            var strawberries = new InventoryItem
            {
                ProductId = 200,
                ProductName = "Organic Fresh Strawberries",
                Price = 5.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 45 }
                }
            };
            
            // Whole Grain Artisan Bread
            var bread = new InventoryItem
            {
                ProductId = 201,
                ProductName = "Whole Grain Artisan Bread",
                Price = 4.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 30 }
                }
            };
            
            // Extra Virgin Olive Oil
            var oliveOil = new InventoryItem
            {
                ProductId = 202,
                ProductName = "Extra Virgin Olive Oil",
                Price = 12.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 25 }
                }
            };
            
            // Organic Free-Range Eggs
            var eggs = new InventoryItem
            {
                ProductId = 203,
                ProductName = "Organic Free-Range Eggs",
                Price = 6.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 50 }
                }
            };
            
            // Grass-Fed Ground Beef
            var beef = new InventoryItem
            {
                ProductId = 204,
                ProductName = "Grass-Fed Ground Beef",
                Price = 8.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 35 }
                }
            };
            
            // Wild Alaskan Salmon Fillet
            var salmon = new InventoryItem
            {
                ProductId = 205,
                ProductName = "Wild Alaskan Salmon Fillet",
                Price = 14.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 20 }
                }
            };
            
            // Organic Mixed Greens Salad
            var salad = new InventoryItem
            {
                ProductId = 206,
                ProductName = "Organic Mixed Greens Salad",
                Price = 3.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 40 }
                }
            };
            
            // Aged Cheddar Cheese
            var cheese = new InventoryItem
            {
                ProductId = 207,
                ProductName = "Aged Cheddar Cheese",
                Price = 7.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 28 }
                }
            };
            
            // Greek Style Yogurt
            var yogurt = new InventoryItem
            {
                ProductId = 208,
                ProductName = "Greek Style Yogurt",
                Price = 5.49m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 38 }
                }
            };
            
            // Organic Honey
            var honey = new InventoryItem
            {
                ProductId = 209,
                ProductName = "Organic Honey",
                Price = 9.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 22 }
                }
            };
            
            inventory.Add(blazer);
            inventory.Add(whiteNavyShirt);
            inventory.Add(redShirt);
            inventory.Add(denimJacket);
            inventory.Add(strawberries);
            inventory.Add(bread);
            inventory.Add(oliveOil);
            inventory.Add(eggs);
            inventory.Add(beef);
            inventory.Add(salmon);
            inventory.Add(salad);
            inventory.Add(cheese);
            inventory.Add(yogurt);
            inventory.Add(honey);
            
            return inventory;
        }
        
        public static List<string> GetSizes()
        {
            return Sizes;
        }
    }
}
