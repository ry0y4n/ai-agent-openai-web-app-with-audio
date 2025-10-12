# Testing Inventory Filtering Feature

## Prerequisites

1. The application must be running with proper Azure AI Agent configuration
2. The Azure AI Agent must have the OpenAPI tool registered with the backend APIs
3. Navigate to the `/inventory` page which shows the product catalog with the chat assistant on the right side

## Test Scenarios

### Test 1: Basic Inventory Search Filtering

**Steps:**
1. Open the Inventory page
2. In the chat assistant on the right, type: "What fashion items do you have?" or "商品を見せて"
3. Wait for the AI agent to respond

**Expected Result:**
- The AI agent should call inventory API(s) like `getAllInventoryItems` or `getInventoryItemById`
- After the response, the inventory grid on the left should filter to show only products mentioned by the agent
- A blue banner should appear at the top showing: "AI Filter Active: Showing X of Y products based on AI assistant recommendations"
- A "Show All" button should be visible in the banner

### Test 2: Product ID Extraction

**Steps:**
1. Clear any existing filter by clicking "Show All"
2. Ask the AI: "Show me product with ID 3" or "商品IDが3の商品を見せて"
3. Wait for the AI agent to respond

**Expected Result:**
- Only product with ID 3 (Navy Single-Breasted Slim Fit Formal Blazer) should be visible
- The filter banner should show "Showing 1 of X products"

### Test 3: Multiple Product Search

**Steps:**
1. Clear the filter
2. Ask the AI: "Show me some clothing items" or "服を何個か見せて"
3. Note which products the AI mentions in its response
4. Check the inventory grid

**Expected Result:**
- The inventory should show only the products that the AI mentioned in its response
- The count in the filter banner should match the number of products shown

### Test 4: Cart Operations (Should NOT Filter)

**Steps:**
1. Clear any existing filter
2. Add an item to the cart from the inventory
3. Ask the AI: "What's in my cart?" or "カートを確認して"
4. Wait for the AI agent to respond

**Expected Result:**
- The AI should show cart contents
- **No filtering should occur** - all inventory items should remain visible
- No filter banner should appear
- This confirms that cart-related API calls don't trigger filtering

### Test 5: Clear Filter Functionality

**Steps:**
1. Apply a filter by asking about products (e.g., "Show me some items")
2. Verify the filter is active (blue banner visible)
3. Click the "Show All" button in the blue banner

**Expected Result:**
- All products should become visible again
- The blue filter banner should disappear
- The inventory should show all X products

### Test 6: New Conversation Clears Filter

**Steps:**
1. Apply a filter by asking about products
2. Verify the filter is active
3. In the chat assistant, click the "New" button (+ icon) to start a new conversation
4. Confirm the action

**Expected Result:**
- The chat history should clear
- The inventory filter should also clear automatically
- All products should be visible
- No filter banner should be shown

### Test 7: Sequential Filtering

**Steps:**
1. Ask: "Show me blazers" or "ブレザーを見せて"
2. Note which products are shown
3. Then ask: "Now show me shirts instead" or "今度はシャツを見せて"
4. Check the inventory

**Expected Result:**
- After the first query, only blazers should be visible
- After the second query, the filter should update to show only shirts
- The previous filter (blazers) should be replaced, not combined

### Test 8: No Results Scenario

**Steps:**
1. Ask about a very specific product that might not exist: "Show me purple elephant costumes"
2. Wait for the AI response

**Expected Result:**
- The AI should respond that such items don't exist or aren't available
- If the AI calls inventory APIs but returns no products, the filter should activate with 0 products
- A message should appear: "No products match the AI assistant's recommendations"

## Debugging Tips

If filtering doesn't work as expected:

1. **Check Browser Console**: Open developer tools (F12) and check for any JavaScript errors
2. **Check Server Logs**: Look for log messages from `AzureAIAgentService` about tool calls
3. **Verify Agent Configuration**: Ensure `AzureAIAgent__ConnectionString` and `AzureAIAgent__AgentId` are set
4. **Check OpenAPI Tool Registration**: Verify the agent has access to the inventory APIs
5. **Test API Directly**: Try calling the inventory APIs directly (e.g., `/api/Inventory`) to ensure they work

## Manual Verification Checklist

- [ ] Filter activates when asking about products
- [ ] Filter shows correct product count
- [ ] "Show All" button clears the filter
- [ ] Cart operations don't trigger filtering
- [ ] New conversation clears the filter
- [ ] Filter banner displays correct information
- [ ] UI remains responsive during filtering
- [ ] Multiple sequential filters work correctly
- [ ] Products are correctly displayed after filtering

## Known Limitations

1. The filter only works with products that the AI agent explicitly retrieves via API calls
2. If the AI mentions products by name without calling the API, filtering won't occur
3. The filter is based on productId, not product names or other attributes
4. Filter state is not persisted across page refreshes
