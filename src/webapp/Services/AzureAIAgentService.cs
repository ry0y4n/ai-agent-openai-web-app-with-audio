using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using dotnetfashionassistant.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace dotnetfashionassistant.Services
{
    public class AzureAIAgentService
    {
        private readonly string? _connectionString;
        private AgentsClient? _client;
        private string? _agentId;
        private readonly string? _originalAgentId;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureAIAgentService> _logger;
        private bool _isConfigured = false;
        private bool _isInitialized = false;
        private readonly object _initLock = new object();
        private readonly int _maxCacheEntries = 100;
        
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _threadHistoryCache = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastCacheUpdateTime = new();

        public AzureAIAgentService(IConfiguration configuration, ILogger<AzureAIAgentService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Get configuration values from environment variables (App Service configuration)
            var envConnectionString = FirstNonEmpty(
                Environment.GetEnvironmentVariable("AzureAIAgent__ConnectionString"),
                Environment.GetEnvironmentVariable("AzureAIAgent_ConnectionString"));
            var envAgentId = FirstNonEmpty(
                Environment.GetEnvironmentVariable("AzureAIAgent__AgentId"),
                Environment.GetEnvironmentVariable("AzureAIAgent_AgentId"));

            // Fallback to appsettings configuration (useful for local development)
            var configSection = _configuration.GetSection("AzureAIAgent");
            var configuredConnectionString = configSection["ConnectionString"];
            var configuredAgentId = configSection["AgentId"];

            _connectionString = FirstNonEmpty(envConnectionString, configuredConnectionString);
            _agentId = FirstNonEmpty(envAgentId, configuredAgentId);
            _originalAgentId = _agentId;

            _isConfigured = !string.IsNullOrEmpty(_connectionString) && !string.IsNullOrEmpty(_agentId);

            if (!_isConfigured)
            {
                _logger.LogWarning("Azure AI Agent configuration missing: ConnectionString={ConnectionStringConfigured}, AgentId={AgentIdConfigured}",
                    !string.IsNullOrEmpty(_connectionString),
                    !string.IsNullOrEmpty(_agentId));
            }
            else
            {
                var source = envConnectionString != null || envAgentId != null
                    ? "environment variables"
                    : "configuration";
                _logger.LogInformation("Azure AI Agent initialized from {Source} with Agent ID: {AgentId}",
                    source,
                    _agentId?.Substring(0, Math.Min(8, _agentId.Length)) + "...");
            }
        }
          // Lazy initialization of the client only when actually needed
        private void EnsureInitialized()
        {
            if (_isInitialized || !_isConfigured)
                return;
                
            lock (_initLock)
            {
                if (_isInitialized)
                    return;
                      try
                {
                    // Use ManagedIdentityCredential for Azure deployment
                    _client = new AgentsClient(_connectionString, new DefaultAzureCredential());
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    // Log the exception but don't set _isConfigured to false - we might succeed next time
                    _logger.LogError(ex, "Error initializing AzureAIAgentService client");
                }
            }
        }public async Task<string> CreateThreadAsync()
        {            if (!_isConfigured)
            {
                _logger.LogWarning("Attempted to create thread with unconfigured AI Agent service");
                return "agent-not-configured";
            }
            
            // Initialize client on demand
            EnsureInitialized();
            
            if (_client == null)
            {
                _logger.LogWarning("Failed to initialize AI Agent client");
                return "agent-initialization-failed";
            }
            
            try
            {                Response<AgentThread> threadResponse = await _client.CreateThreadAsync();
                AgentThread thread = threadResponse.Value;
                return thread.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AI Agent thread");
                return "agent-not-configured";
            }
        }        public async Task<AgentResponse> SendMessageAsync(string threadId, string userMessage)
        {
            if (!_isConfigured || threadId == "agent-not-configured")
            {
                _logger.LogWarning("Attempted to send message with unconfigured AI Agent service");
                return new AgentResponse 
                { 
                    ResponseText = "The AI agent is not properly configured. Please add AzureAIAgent__ConnectionString and AzureAIAgent__AgentId via environment variables or appsettings." 
                };
            }
            
            // Initialize client on demand
            EnsureInitialized();
            
            if (_client == null || _agentId == null)
            {
                _logger.LogWarning("Failed to initialize AI Agent client");
                return new AgentResponse 
                { 
                    ResponseText = "The AI agent client could not be initialized. Please check the configuration and try again." 
                };
            }
              try
            {
                // Send user message to the thread
                Response<ThreadMessage> messageResponse = await _client.CreateMessageAsync(
                    threadId,
                    MessageRole.User,
                    userMessage);

                // Create and run a request with the agent
                Response<ThreadRun> runResponse = await _client.CreateRunAsync(
                    threadId,
                    _agentId);

                ThreadRun run = runResponse.Value;                // Poll until the run reaches a terminal status
                do
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    runResponse = await _client.GetRunAsync(threadId, run.Id);
                }
                while (runResponse.Value.Status == RunStatus.Queued
                    || runResponse.Value.Status == RunStatus.InProgress);                // Get all messages in the thread
                Response<PageableList<ThreadMessage>> messagesResponse = await _client.GetMessagesAsync(threadId);
                IReadOnlyList<ThreadMessage> messages = messagesResponse.Value.Data;
                
                // Get the latest assistant message (using historical convention of "assistant" role)
                ThreadMessage? latestAssistantMessage = messages
                    .Where(m => m.Role.ToString().Equals("Assistant", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                string responseText = "";
                if (latestAssistantMessage != null)
                {
                    foreach (MessageContent contentItem in latestAssistantMessage.ContentItems)
                    {
                        if (contentItem is MessageTextContent textItem)
                        {
                            responseText += textItem.Text;
                        }
                    }
                }
                
                // Get run steps to extract tool call information
                var toolCalls = await GetToolCallsFromRunAsync(threadId, run.Id);

                if (string.IsNullOrEmpty(responseText))
                {
                    _logger.LogWarning("No assistant response found in thread {ThreadId}", threadId);
                    responseText = "No response from AI agent.";
                }

                return new AgentResponse 
                { 
                    ResponseText = responseText,
                    ToolCalls = toolCalls
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with AI agent for thread {ThreadId}", threadId);
                return new AgentResponse 
                { 
                    ResponseText = $"Error communicating with AI agent: {ex.Message}" 
                };
            }
        }

        private async Task<List<AgentToolCallInfo>> GetToolCallsFromRunAsync(string threadId, string runId)
        {
            var toolCallsList = new List<AgentToolCallInfo>();
            
            if (_client == null)
            {
                return toolCallsList;
            }
            
            try
            {
                // Get run steps which contain tool call information
                Response<PageableList<RunStep>> runStepsResponse = await _client.GetRunStepsAsync(threadId, runId);
                IReadOnlyList<RunStep> runSteps = runStepsResponse.Value.Data;

                foreach (var step in runSteps)
                {
                    // Check if this step contains tool calls
                    if (step.StepDetails is RunStepToolCallDetails toolCallDetails)
                    {
                        foreach (var toolCall in toolCallDetails.ToolCalls)
                        {
                            // We're interested in function tool calls (API calls)
                            if (toolCall is RunStepFunctionToolCall functionCall)
                            {
                                toolCallsList.Add(new AgentToolCallInfo
                                {
                                    ToolCallId = toolCall.Id,
                                    FunctionName = functionCall.Name,
                                    Arguments = functionCall.Arguments,
                                    Output = functionCall.Output
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving tool calls from run {RunId}", runId);
            }

            return toolCallsList;
        }public async Task<List<ChatMessage>> GetThreadHistoryAsync(string threadId)
        {
            if (!_isConfigured || threadId == "agent-not-configured")
            {
                return new List<ChatMessage> {
                    new ChatMessage {
                        Content = "The AI agent is not properly configured. Please add AzureAIAgent__ConnectionString and AzureAIAgent__AgentId via environment variables or appsettings.",
                        IsUser = false,
                        Timestamp = DateTime.Now
                    }
                };
            }
            
            // Initialize client on demand
            EnsureInitialized();
            
            if (_client == null)
            {
                return new List<ChatMessage> {
                    new ChatMessage {
                        Content = "The AI agent client could not be initialized. Please check the configuration and try again.",
                        IsUser = false,
                        Timestamp = DateTime.Now
                    }
                };
            }            // Check if we already have this thread history cached and is very recent (less than 5 seconds old)
            // This ensures more frequent refreshes of the chat history
            if (_threadHistoryCache.TryGetValue(threadId, out var cachedHistory) && 
                _lastCacheUpdateTime.TryGetValue(threadId, out var lastUpdate) &&
                (DateTime.UtcNow - lastUpdate).TotalSeconds < 5)
            {
                // Return the cached history if it exists and is very recent
                return new List<ChatMessage>(cachedHistory);
            }
            
            var chatHistory = new List<ChatMessage>();
            
            try
            {                // Set a cancellation timeout to prevent hanging
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)); // Increased timeout for pagination
                
                // Get all messages - the SDK handles pagination internally
                var allMessages = new List<ThreadMessage>();
                Response<PageableList<ThreadMessage>> messagesResponse = await _client.GetMessagesAsync(
                    threadId, 
                    cancellationToken: cts.Token);
                
                // Add messages from the response
                allMessages.AddRange(messagesResponse.Value.Data);

                // Process messages in chronological order (oldest to newest)
                foreach (ThreadMessage message in allMessages.OrderBy(m => m.CreatedAt))
                {
                    string messageContent = "";
                    foreach (MessageContent contentItem in message.ContentItems)
                    {
                        if (contentItem is MessageTextContent textItem)
                        {
                            messageContent += textItem.Text ?? string.Empty;
                        }
                    }
                      // For AI messages, format the content for proper HTML display
                    string formattedContent = message.Role != MessageRole.User ? 
                        FormatMessageContent(messageContent) : "";
                        
                    chatHistory.Add(new ChatMessage
                    {
                        Content = messageContent,
                        FormattedContent = formattedContent,
                        IsUser = message.Role == MessageRole.User,
                        Timestamp = message.CreatedAt.DateTime
                    });
                }
                  // Implement cache eviction policy - if cache exceeds limit, remove oldest entries
                if (_threadHistoryCache.Count >= _maxCacheEntries)
                {
                    // Get oldest entries based on last update time
                    var oldestEntries = _lastCacheUpdateTime
                        .OrderBy(x => x.Value)
                        .Take(_threadHistoryCache.Count - _maxCacheEntries + 1)
                        .ToList();
                    
                    // Remove oldest entries
                    foreach (var entry in oldestEntries)
                    {
                        _threadHistoryCache.TryRemove(entry.Key, out _);
                        _lastCacheUpdateTime.TryRemove(entry.Key, out _);
                    }
                }
                
                // Update the cache using thread-safe methods
                _threadHistoryCache.AddOrUpdate(threadId, new List<ChatMessage>(chatHistory), 
                    (key, oldValue) => new List<ChatMessage>(chatHistory));
                _lastCacheUpdateTime.AddOrUpdate(threadId, DateTime.UtcNow, 
                    (key, oldValue) => DateTime.UtcNow);            }
            catch (OperationCanceledException ex)
            {
                // If the operation timed out, return cached data if available or empty list
                _logger.LogWarning(ex, "Thread history operation timed out for thread {ThreadId}", threadId);
                return cachedHistory ?? new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                // If there's an error, return cached data if available or empty list
                _logger.LogError(ex, "Error retrieving thread history for thread {ThreadId}", threadId);
                return cachedHistory ?? new List<ChatMessage>();
            }            return chatHistory;
        }
        
        // Helper method to format message content with HTML tags
        private string FormatMessageContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
                
            // Handle markdown-style bold text (convert **text** to <strong>text</strong>)
            content = System.Text.RegularExpressions.Regex.Replace(
                content, 
                @"\*\*([^*]+)\*\*", 
                "<strong>$1</strong>");
                
            // Handle line breaks and bullet points
            return content
                .Replace("\n\n", "<br><br>")
                .Replace("\n", "<br>")
                .Replace("•", "<br>•");
        }

        private static string? FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Updates the Agent ID for testing purposes
        /// </summary>
        /// <param name="newAgentId">The new agent ID to use</param>
        public void SetAgentId(string newAgentId)
        {
            if (string.IsNullOrWhiteSpace(newAgentId))
            {
                _logger.LogWarning("Attempted to set empty or null Agent ID");
                return;
            }
            
            _agentId = newAgentId;
            _isConfigured = !string.IsNullOrEmpty(_connectionString) && !string.IsNullOrEmpty(_agentId);
            
            // Force re-initialization on next use
            _isInitialized = false;
            
            _logger.LogInformation("Agent ID updated for testing purposes");
        }
        
        /// <summary>
        /// Gets the current Agent ID
        /// </summary>
        /// <returns>The current agent ID or null if not set</returns>
        public string? GetCurrentAgentId()
        {
            return _agentId;
        }
        
        /// <summary>
        /// Gets the original Agent ID from environment variables
        /// </summary>
        /// <returns>The original agent ID from environment variables</returns>
        public string? GetOriginalAgentId()
        {
            return _originalAgentId;
        }
        
        /// <summary>
        /// Checks if the current Agent ID differs from the original
        /// </summary>
        /// <returns>True if the agent ID has been modified from the original</returns>
        public bool IsAgentIdModified()
        {
            return _agentId != _originalAgentId;
        }
        
        /// <summary>
        /// Checks if the service is properly configured
        /// </summary>
        /// <returns>True if both connection string and agent ID are set</returns>
        public bool IsConfigured()
        {
            return _isConfigured;
        }
    }
}
