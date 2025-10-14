using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnetfashionassistant.Models
{    public class InventoryItem
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
        
        [JsonPropertyName("productName")]
        public required string ProductName { get; set; }
        
        [JsonPropertyName("sizeInventory")]
        public Dictionary<string, int> SizeInventory { get; set; }
        
        [JsonPropertyName("price")]
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
        private static List<InventoryItem>? _cachedInventory = null;

        public static List<InventoryItem> GetInventory()
        {
            // Return cached inventory if already loaded
            if (_cachedInventory != null)
            {
                return _cachedInventory;
            }

            // Attempt to resolve inventory JSON from multiple possible locations (see reasoning in Home.razor change).
            string? ResolveInventoryJson()
            {
                var fileName = "inventory.json";
                var candidates = new List<string>
                {
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "data", fileName),                                           // Published output scenario
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "wwwroot", "data", fileName)), // Development run (bin path -> project root)
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", fileName)                                    // Working directory root
                };
                foreach (var path in candidates.Distinct())
                {
                    if (File.Exists(path)) return path;
                }
                return null;
            }

            var resolvedInventoryPath = ResolveInventoryJson();
            if (resolvedInventoryPath != null)
            {
                try
                {
                    var jsonContent = File.ReadAllText(resolvedInventoryPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _cachedInventory = JsonSerializer.Deserialize<List<InventoryItem>>(jsonContent, options);
                    if (_cachedInventory != null)
                    {
                        Console.WriteLine($"Loaded inventory from JSON: {resolvedInventoryPath}");
                        return _cachedInventory;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading inventory from JSON at '{resolvedInventoryPath}': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("inventory.json not found in any expected location. Using fallback data.");
            }

            // Fallback simplified: Large English hardcoded inventory removed post-localization.
            // We now rely on inventory.json as the single source of truth. Provide a minimal
            // placeholder so UI components can still render gracefully if the JSON is missing.
            var placeholder = new InventoryItem
            {
                ProductId = 0,
                ProductName = "在庫データ未取得 (inventory.json を配置してください)",
                Price = 0m,
                SizeInventory = new Dictionary<string, int>
                {
                    { "Unit", 0 }
                }
            };
            _cachedInventory = new List<InventoryItem> { placeholder };
            Console.WriteLine("inventory.json fallback: using minimal placeholder inventory instead of duplicated hardcoded English data.");
            return _cachedInventory;
        }
        
        public static List<string> GetSizes()
        {
            return Sizes;
        }
    }
}
