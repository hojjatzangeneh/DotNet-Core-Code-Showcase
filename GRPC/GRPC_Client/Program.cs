using Grpc.Net.Client;

using GRPC_Server.Protos;

using static GRPC_Server.Protos.ContractService;

GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:7014/");
ContractServiceClient contractService = new(channel);
var result =contractService.GetContract(new ContractRequest { ContractId = "11" , Title = "Hello Hojjat" });
Console.WriteLine(result);
Console.ReadLine();
