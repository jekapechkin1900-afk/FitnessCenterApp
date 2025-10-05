namespace FitnessCenterApp.Shared.RPC;

public class RpcRequest
{
	public string? ServiceName { get; set; }
	public string? MethodName { get; set; }
	public string? Payload { get; set; } 
}
