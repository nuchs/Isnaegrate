using Grpc.Net.Client;
using Jaundicedsage.Grpc;
using static Jaundicedsage.Grpc.UserDir;

namespace Nimm2;

internal class UsersTest : ITest
{
    private string? userId;

    public UsersTest(string[] args)
    {
        if (args.Length > 1)
            userId = args[1];
    }

    public async Task Run()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5079");
        var client = new UserDirClient(channel);

        if (userId == null)
        {
            await AllUsersTest(client);
        }
        else
        {
            await SingleUserTest(client);
        }
    }

    private async Task SingleUserTest(UserDirClient client)
    {
        var reply = await client.GetUserAsync(new UserRequest() { Id = userId });

        Console.WriteLine($"Single User : {userId}");
        Console.WriteLine("----------------------------------------------------------");
        ToPrettyString(reply.Value);
    }

    private async Task AllUsersTest(UserDirClient client)
    {
        var reply = await client.GetAllUsersAsync(new AllUserRequest());

        Console.WriteLine("All Users");
        Console.WriteLine("----------------------------------------------------------");
        foreach (var user in reply.Users)
        {
            ToPrettyString(user);
            Console.WriteLine();
        }
    }

    private void ToPrettyString(UserRecord user)
    {
        Console.WriteLine($"Id      : {user.Id}");
        Console.WriteLine($"Name    : {user.Name}");
        Console.WriteLine($"Org     : {user.Org}");
        Console.WriteLine($"Title   : {user.Title}");
        Console.WriteLine($"Expiry  : {user.CertExpires}");
        Console.WriteLine($"Last On : {user.LastOnline}");
    }
}
