# Inventory Filtering Architecture

## Component Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Interaction                         │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      ChatComponent.razor                         │
│  - Sends user message to AI agent                               │
│  - Receives AgentResponse with tool calls                       │
│  - Updates InventoryFilterService                               │
└───────────────┬───────────────────────────┬─────────────────────┘
                │                           │
                ▼                           ▼
┌───────────────────────────┐   ┌──────────────────────────────┐
│  AzureAIAgentService      │   │  InventoryFilterService      │
│  - Creates thread         │   │  - Stores filter state       │
│  - Sends message          │   │  - Extracts product IDs      │
│  - Gets run steps         │   │  - Notifies subscribers      │
│  - Extracts tool calls    │   └──────────┬───────────────────┘
└───────┬───────────────────┘              │
        │                                  │ Filter Changed Event
        ▼                                  │
┌───────────────────────────┐              │
│  Azure AI Agent           │              │
│  (Azure AI Foundry)       │              │
│  - Processes message      │              │
│  - Calls OpenAPI tools    │              │
│  - Returns response       │              │
└───────┬───────────────────┘              │
        │                                  │
        ▼                                  │
┌───────────────────────────┐              │
│  Backend API              │              │
│  - Inventory endpoints    │              │
│  - Cart endpoints         │              │
│  - Returns JSON data      │              │
└───────────────────────────┘              │
                                           │
                                           ▼
                            ┌──────────────────────────────┐
                            │   Inventory.razor            │
                            │   - Subscribes to filter     │
                            │   - Applies filtering        │
                            │   - Shows filtered products  │
                            │   - Displays filter banner   │
                            └──────────────────────────────┘
```

## Data Flow

### 1. User Asks Question
```
User → ChatComponent → AzureAIAgentService → Azure AI Agent
```

### 2. Agent Calls APIs
```
Azure AI Agent → Backend API (Inventory/Cart) → Returns JSON
```

### 3. Tool Call Extraction
```
AzureAIAgentService ← Azure AI Agent (with Run Steps)
  ↓
AgentResponse { ResponseText, ToolCalls[] }
  ↓
ChatComponent
```

### 4. Filter Update
```
ChatComponent → InventoryFilterService.UpdateFilterFromToolCalls()
  ↓
Parse function names and outputs
  ↓
Extract product IDs
  ↓
Update filter state
  ↓
Raise OnFilterChanged event
```

### 5. UI Update
```
InventoryFilterService → OnFilterChanged event
  ↓
Inventory.razor.OnFilterChanged()
  ↓
ApplyFilter()
  ↓
Update displayedInventory
  ↓
StateHasChanged()
  ↓
UI re-renders with filtered products
```

## Key Classes and Their Responsibilities

### AgentToolCallInfo
- Data model for individual tool calls
- Properties: ToolCallId, FunctionName, Arguments, Output

### AgentResponse
- Container for agent response and tool calls
- Properties: ResponseText, ToolCalls

### InventoryFilterService (Singleton)
- **Manages filter state**: Maintains set of product IDs to display
- **Detects inventory operations**: Identifies inventory-related API calls
- **Extracts product IDs**: Parses JSON responses to get product IDs
- **Notifies subscribers**: Raises events when filter changes
- **Public API**:
  - `UpdateFilterFromToolCalls(functionNames, functionOutputs)`
  - `ClearFilter()`
  - `IsFilterActive` property
  - `FilteredProductIds` property
  - `OnFilterChanged` event

### AzureAIAgentService (Scoped)
- **Enhanced return type**: Returns `AgentResponse` instead of `string`
- **Captures tool calls**: Retrieves run steps and extracts tool call info
- **Public API**:
  - `SendMessageAsync(threadId, userMessage)` → `AgentResponse`
  - `GetToolCallsFromRunAsync(threadId, runId)` → `List<AgentToolCallInfo>`

### ChatComponent
- **Sends messages**: Communicates with AI agent
- **Updates filter**: Calls InventoryFilterService after receiving response
- **Clears filter**: Resets filter on new conversation

### Inventory.razor
- **Subscribes to changes**: Listens to InventoryFilterService events
- **Applies filter**: Filters product list based on service state
- **Shows UI indicators**: Displays filter banner and "Show All" button
- **Unsubscribes on dispose**: Proper cleanup

## Tool Call Detection Logic

```csharp
// Inventory-related functions (WILL trigger filtering)
- getAllInventoryItems
- getInventoryItemById  
- getInventorySizeCount

// Cart-related functions (will NOT trigger filtering)
- getCartSummary
- addToCart
- removeCartItem
- updateCartItemQuantity
- clearCart
```

## JSON Parsing Strategy

Uses regex to extract product IDs from API responses:

```csharp
Pattern: "productId"\s*:\s*(\d+)

Examples:
  {"productId": 123, ...}        → Extracts: 123
  [{"productId": 1}, {"productId": 2}] → Extracts: 1, 2
```

## State Management

```
InventoryFilterService (Singleton)
  ├── _filteredProductIds: HashSet<int>
  ├── _isFilterActive: bool
  └── OnFilterChanged: event Action

Shared across all components
Persists for application lifetime
Does NOT persist across page refreshes
```
