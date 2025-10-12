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
            
            // Electronics Category
            var headphones = new InventoryItem
            {
                ProductId = 300,
                ProductName = "Wireless Bluetooth Headphones",
                Price = 79.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 32 }
                }
            };
            
            var smartWatch = new InventoryItem
            {
                ProductId = 301,
                ProductName = "Smart Fitness Watch",
                Price = 149.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 18 }
                }
            };
            
            var powerBank = new InventoryItem
            {
                ProductId = 302,
                ProductName = "Portable Power Bank 20000mAh",
                Price = 39.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 55 }
                }
            };
            
            var webcam = new InventoryItem
            {
                ProductId = 303,
                ProductName = "4K Webcam with Microphone",
                Price = 89.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 28 }
                }
            };
            
            var gamingMouse = new InventoryItem
            {
                ProductId = 304,
                ProductName = "Wireless Gaming Mouse",
                Price = 59.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 42 }
                }
            };
            
            // Home & Kitchen Category
            var coffeeMaker = new InventoryItem
            {
                ProductId = 400,
                ProductName = "Stainless Steel Coffee Maker",
                Price = 129.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 15 }
                }
            };
            
            var cookwareSet = new InventoryItem
            {
                ProductId = 401,
                ProductName = "Non-Stick Cookware Set 10-Piece",
                Price = 99.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 12 }
                }
            };
            
            var electricKettle = new InventoryItem
            {
                ProductId = 402,
                ProductName = "Electric Kettle 1.7L",
                Price = 34.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 38 }
                }
            };
            
            var pillowSet = new InventoryItem
            {
                ProductId = 403,
                ProductName = "Memory Foam Pillow Set",
                Price = 49.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 25 }
                }
            };
            
            var robotVacuum = new InventoryItem
            {
                ProductId = 404,
                ProductName = "Robot Vacuum Cleaner",
                Price = 199.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 8 }
                }
            };
            
            // Books & Media Category
            var mysteryNovel = new InventoryItem
            {
                ProductId = 500,
                ProductName = "Best-Selling Mystery Novel",
                Price = 24.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 48 }
                }
            };
            
            var cookbook = new InventoryItem
            {
                ProductId = 501,
                ProductName = "Complete Cookbook Collection",
                Price = 39.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 35 }
                }
            };
            
            var leatherJournal = new InventoryItem
            {
                ProductId = 502,
                ProductName = "Premium Leather Journal",
                Price = 29.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 44 }
                }
            };
            
            var scienceKit = new InventoryItem
            {
                ProductId = 503,
                ProductName = "Educational Science Kit",
                Price = 44.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 20 }
                }
            };
            
            // Sports & Outdoors Category
            var yogaMat = new InventoryItem
            {
                ProductId = 600,
                ProductName = "Yoga Mat with Carrying Strap",
                Price = 29.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 52 }
                }
            };
            
            var dumbbellSet = new InventoryItem
            {
                ProductId = 601,
                ProductName = "Adjustable Dumbbell Set",
                Price = 299.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 10 }
                }
            };
            
            var campingTent = new InventoryItem
            {
                ProductId = 602,
                ProductName = "Camping Tent 4-Person",
                Price = 159.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 14 }
                }
            };
            
            var jumpRope = new InventoryItem
            {
                ProductId = 603,
                ProductName = "Professional Jump Rope",
                Price = 19.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 65 }
                }
            };
            
            var waterBottle = new InventoryItem
            {
                ProductId = 604,
                ProductName = "Insulated Water Bottle 32oz",
                Price = 24.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 58 }
                }
            };
            
            // Health & Beauty Category
            var faceMoisturizer = new InventoryItem
            {
                ProductId = 700,
                ProductName = "Natural Face Moisturizer",
                Price = 34.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 42 }
                }
            };
            
            var electricToothbrush = new InventoryItem
            {
                ProductId = 701,
                ProductName = "Electric Toothbrush Set",
                Price = 69.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 28 }
                }
            };
            
            var essentialOilSet = new InventoryItem
            {
                ProductId = 702,
                ProductName = "Aromatherapy Essential Oil Set",
                Price = 29.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 50 }
                }
            };
            
            var hairStylingSet = new InventoryItem
            {
                ProductId = 703,
                ProductName = "Hair Styling Tool Set",
                Price = 79.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 22 }
                }
            };
            
            // Toys & Games Category
            var boardGame = new InventoryItem
            {
                ProductId = 800,
                ProductName = "Strategy Board Game",
                Price = 44.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 35 }
                }
            };
            
            var buildingBlocks = new InventoryItem
            {
                ProductId = 801,
                ProductName = "Building Blocks Set 1000 Pieces",
                Price = 39.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 28 }
                }
            };
            
            var rcCar = new InventoryItem
            {
                ProductId = 802,
                ProductName = "Remote Control Racing Car",
                Price = 89.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 18 }
                }
            };
            
            var artSet = new InventoryItem
            {
                ProductId = 803,
                ProductName = "Art Supplies Drawing Set",
                Price = 54.99m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 32 }
                }
            };
            
            // Add all items to inventory
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
            inventory.Add(headphones);
            inventory.Add(smartWatch);
            inventory.Add(powerBank);
            inventory.Add(webcam);
            inventory.Add(gamingMouse);
            inventory.Add(coffeeMaker);
            inventory.Add(cookwareSet);
            inventory.Add(electricKettle);
            inventory.Add(pillowSet);
            inventory.Add(robotVacuum);
            inventory.Add(mysteryNovel);
            inventory.Add(cookbook);
            inventory.Add(leatherJournal);
            inventory.Add(scienceKit);
            inventory.Add(yogaMat);
            inventory.Add(dumbbellSet);
            inventory.Add(campingTent);
            inventory.Add(jumpRope);
            inventory.Add(waterBottle);
            inventory.Add(faceMoisturizer);
            inventory.Add(electricToothbrush);
            inventory.Add(essentialOilSet);
            inventory.Add(hairStylingSet);
            inventory.Add(boardGame);
            inventory.Add(buildingBlocks);
            inventory.Add(rcCar);
            inventory.Add(artSet);
            
            return inventory;
        }
        
        public static List<string> GetSizes()
        {
            return Sizes;
        }
    }
}
