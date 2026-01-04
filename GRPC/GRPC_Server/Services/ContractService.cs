namespace GRPC_Server.Services;

public class ContractService:GRPC_Server.Protos.ContractService.ContractServiceBase
{
    public override async Task<Protos.ContractResponse> GetContract(Protos.ContractRequest request, Grpc.Core.ServerCallContext context)
    {
        var response = new Protos.ContractResponse
        {
            ContractId = request.ContractId,
            Title = $"Details of contract {request.ContractId}" , 
            Status = "Ok"
        };
        return await Task.FromResult(response);
    }
}
