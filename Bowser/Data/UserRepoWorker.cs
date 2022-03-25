using static Jaundicedsage.Grpc.UserDir;

namespace Bowser.Data;

public class UserRepoWorker : BackgroundService
{
    private readonly ILogger<UserRepoWorker> log;
    private readonly UserDirClient userDir;
    private readonly UserRepo repo;

    public UserRepoWorker(UserDirClient userDir, UserRepo repo, ILogger<UserRepoWorker> log)
    {
        this.log = log;
        this.userDir = userDir;
        this.repo = repo;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Loading users");

        var listing = await userDir.GetAllUsersAsync(
            new Jaundicedsage.Grpc.AllUserRequest(), 
            cancellationToken: stoppingToken);

        foreach (var user in listing.Users)
        {
            repo.Add(user);
        }

        log.LogInformation("Users loaded");
    }
}
