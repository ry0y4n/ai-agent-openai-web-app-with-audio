namespace dotnetfashionassistant.Models
{
    /// <summary>
    /// Information about a tool call made by the AI agent
    /// </summary>
    public class AgentToolCallInfo
    {
        public string? ToolCallId { get; set; }
        public string? FunctionName { get; set; }
        public string? Arguments { get; set; }
        public string? Output { get; set; }
    }

    /// <summary>
    /// Response from the AI agent including tool call information
    /// </summary>
    public class AgentResponse
    {
        public string ResponseText { get; set; } = string.Empty;
        public List<AgentToolCallInfo> ToolCalls { get; set; } = new();
    }
}
