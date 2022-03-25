using System.Collections.Concurrent;
using static Jaundicedsage.Grpc.UserDir;

namespace Bowser.Data;

public class UserRepoWorker : BackgroundService
{
    private readonly ConcurrentBag<User> users = new ConcurrentBag<User>();
    private readonly ILogger<UserRepo> log;
    private readonly UserDirClient userDir;
    private readonly UserRepo repo;

    public UserRepoWorker(UserDirClient userDir, UserRepo repo, ILogger<UserRepo> log)
    {
        this.log = log;
        this.userDir = userDir;
        this.repo = repo;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Loading users");

        var listing = await userDir.GetAllUsersAsync(new Jaundicedsage.Grpc.AllUserRequest());

        foreach (var user in listing.Users)
        {
            repo.Add(user);
            users.Add(new User(
                user.Id,
                user.Title,
                user.Name,
                user.Org,
                user.CertExpires.ToDateTime(),
                user.LastOnline.ToDateTime()
            ));
        }

        log.LogInformation("Users loaded");
    }
}
