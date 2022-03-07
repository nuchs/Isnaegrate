using Grpc.Net.Client;

namespace Nimm2;

internal interface ITest
{
    Task Run(GrpcChannel channel);
}
