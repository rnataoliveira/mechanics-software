using System.Text.Json.Serialization;

namespace MechanicsSoftware.API.Transport.ServiceOrders;

[JsonConverter(typeof(JsonStringEnumConverter<BudgetDecision>))]
public enum BudgetDecision
{
    [JsonPropertyName("approve")] Approve,
    [JsonPropertyName("reject")] Reject
}

public sealed record BudgetDecisionRequest(BudgetDecision Decision);
