# AI Agent Inventory Filtering Feature

## Overview

This feature implements intelligent inventory filtering on the Inventory page based on the AI agent's tool calls. When the AI agent searches for products using inventory-related APIs, the inventory display automatically filters to show only the products that the agent is discussing.

## How It Works

### 1. Tool Call Detection

The `AzureAIAgentService` has been enhanced to capture tool call information from the Azure AI Agent's run steps. When the agent executes a run, it:
- Retrieves run steps which contain information about which APIs were called
- Extracts function tool calls (OpenAPI tool calls)
- Returns both the response text and the list of tool calls

### 2. Filtering Logic

The `InventoryFilterService` manages the filtering state:
- Monitors for inventory-related API calls: `getAllInventoryItems`, `getInventoryItemById`, `getInventorySizeCount`
- Ignores cart-related API calls: `getCartSummary`, `addToCart`, `removeCartItem`, etc.
- Extracts product IDs from the API response outputs using regex pattern matching
- Notifies subscribed components when the filter changes

### 3. UI Integration

The `Inventory.razor` page:
- Subscribes to filter change events from `InventoryFilterService`
- Dynamically filters the displayed products based on the active filter
- Shows a clear indicator when filtering is active
- Provides a "Show All" button to clear the filter

The `ChatComponent`:
- Updates the filter service after receiving responses from the AI agent
- Clears the filter when starting a new conversation

## User Experience

### Scenario 1: Inventory Search
1. User asks: "○○な商品って何がある？" (What products are like XX?)
2. AI agent calls inventory APIs to search for products
3. AI responds: "商品A・B・Cがあります" (There are products A, B, and C)
4. The inventory list automatically filters to show only products A, B, and C
5. A blue banner appears showing "AI Filter Active: Showing 3 of 50 products based on AI assistant recommendations"

### Scenario 2: Follow-up Query
1. User asks: "それに追加でおすすめは？" (What else would you recommend in addition to those?)
2. AI agent searches again and responds: "B・D・Fがおすすめです" (I recommend B, D, and F)
3. The filter updates to show only products B, D, and F

### Scenario 3: Non-Inventory Query
1. User asks: "カートを確認して" (Check my cart)
2. AI agent calls cart APIs (not inventory APIs)
3. The filter remains unchanged - no filtering occurs

### Scenario 4: Clear Filter
1. User clicks the "Show All" button in the blue banner
2. All products are displayed again
3. The filter indicator disappears

## Technical Details

### New Files

- `Models/AgentToolCallInfo.cs`: Data models for tool call information
- `Services/InventoryFilterService.cs`: Manages filtering state and logic
- `Models/SpeechModels.cs`: Speech recognition models (fix for existing build errors)

### Modified Files

- `Services/AzureAIAgentService.cs`: Enhanced to capture and return tool call information
- `Components/ChatComponent.razor`: Integrated with filtering service
- `Components/Pages/Home.razor`: Updated to use new AgentResponse type
- `Components/Pages/Inventory.razor`: Implemented filtering UI and logic
- `Program.cs`: Registered InventoryFilterService as a singleton

### API Response Parsing

The service uses regex pattern matching to extract product IDs from JSON responses:
- Pattern: `"productId"\s*:\s*(\d+)`
- Handles both single object responses and array responses
- Case-insensitive matching
- Gracefully handles parsing errors

## Configuration

No configuration is required. The feature is automatically enabled when:
1. The Azure AI Agent is properly configured with connection string and agent ID
2. The OpenAPI tools are registered with the agent
3. The backend APIs are accessible

## Future Enhancements

Possible improvements for future versions:
- Support for more complex filtering scenarios (e.g., by category, price range)
- Filter persistence across page refreshes
- Multiple concurrent filters
- Filter history and undo functionality
- Visual highlighting of filtered products in the grid
