namespace FitnessCenterApp.Shared.RPC;

public class RpcResponse
{
	public RpcStatusCode StatusCode { get; set; }
	public string? Payload { get; set; }
}