using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnetfashionassistant.Services
{
    /// <summary>
    /// Service to manage inventory filtering based on AI agent tool calls
    /// </summary>
    public class InventoryFilterService
    {
        private HashSet<int> _filteredProductIds = new();
        private bool _isFilterActive = false;
        
        public event Action? OnFilterChanged;

        /// <summary>
        /// Gets the list of product IDs to display (empty means show all)
        /// </summary>
        public IReadOnlySet<int> FilteredProductIds => _filteredProductIds;

        /// <summary>
        /// Indicates whether filtering is currently active
        /// </summary>
        public bool IsFilterActive => _isFilterActive;

        /// <summary>
        /// Updates the filter based on tool calls made by the AI agent
        /// </summary>
        /// <param name="functionNames">List of function names that were called</param>
        /// <param name="functionOutputs">List of function outputs (JSON strings)</param>
        public void UpdateFilterFromToolCalls(List<string> functionNames, List<string> functionOutputs)
        {
            // Check if any inventory-related functions were called
            // Function names may have a prefix like "online_store_tool_"
            var inventoryFunctions = new HashSet<string>
            {
                "getAllInventoryItems",
                "getInventoryItemById",
                "getInventorySizeCount",
                "online_store_tool_getAllInventoryItems",
                "online_store_tool_getInventoryItemById",
                "online_store_tool_getInventorySizeCount"
            };

            // Check if any function matches (with or without prefix)
            bool hasInventoryCall = functionNames.Any(fn => 
                inventoryFunctions.Contains(fn) || 
                inventoryFunctions.Any(invFunc => fn.EndsWith(invFunc)));
            
            if (!hasInventoryCall)
            {
                // No inventory-related calls, don't change filter
                return;
            }

            // Extract product IDs from the function outputs
            var productIds = new HashSet<int>();
            
            for (int i = 0; i < functionNames.Count; i++)
            {
                if (i >= functionOutputs.Count) break;
                
                var functionName = functionNames[i];
                var output = functionOutputs[i];
                
                if (string.IsNullOrWhiteSpace(output)) continue;

                // Parse product IDs from the output based on the function type
                // Function name may have a prefix, so check with EndsWith
                if (functionName.EndsWith("getAllInventoryItems"))
                {
                    // Output is an array of inventory items
                    ExtractProductIdsFromArray(output, productIds);
                }
                else if (functionName.EndsWith("getInventoryItemById") || functionName.EndsWith("getInventorySizeCount"))
                {
                    // Output is a single inventory item
                    ExtractProductIdFromObject(output, productIds);
                }
            }

            // Only update filter if we found product IDs
            if (productIds.Count > 0)
            {
                _filteredProductIds = productIds;
                _isFilterActive = true;
                NotifyFilterChanged();
            }
        }

        /// <summary>
        /// Clears the current filter and shows all products
        /// </summary>
        public void ClearFilter()
        {
            _filteredProductIds.Clear();
            _isFilterActive = false;
            NotifyFilterChanged();
        }

        private void ExtractProductIdsFromArray(string jsonArray, HashSet<int> productIds)
        {
            try
            {
                // Extract productId values from JSON or Python dict format
                // Looking for "productId": <number> or 'productId': <number> pattern
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    jsonArray, 
                    @"['""]productId['""]\s*:\s*(\d+)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int productId))
                    {
                        productIds.Add(productId);
                    }
                }
            }
            catch
            {
                // Silently fail if parsing fails
            }
        }

        private void ExtractProductIdFromObject(string jsonObject, HashSet<int> productIds)
        {
            try
            {
                // Extract productId value from JSON or Python dict format
                // Looking for "productId": <number> or 'productId': <number> pattern
                var match = System.Text.RegularExpressions.Regex.Match(
                    jsonObject, 
                    @"['""]productId['""]\s*:\s*(\d+)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (match.Success && match.Groups.Count > 1 && 
                    int.TryParse(match.Groups[1].Value, out int productId))
                {
                    productIds.Add(productId);
                }
            }
            catch
            {
                // Silently fail if parsing fails
            }
        }

        private void NotifyFilterChanged()
        {
            OnFilterChanged?.Invoke();
        }
    }
}
