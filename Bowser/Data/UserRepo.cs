using Jaundicedsage.Grpc;
using System.Collections;
using System.Collections.Concurrent;

namespace Bowser.Data;

public class UserRepo : IEnumerable<User>
{
    private readonly ConcurrentBag<User> users = new ConcurrentBag<User>();
    private readonly ILogger<UserRepo> log;

    public UserRepo(ILogger<UserRepo> log)
    {
        this.log = log;
    }

    public void Add(UserRecord user)
    {
        log.LogInformation("Adding user {user}", user.Id);

        users.Add(new User(
              user.Id,
              user.Title,
              user.Name,
              user.Org,
              user.CertExpires.ToDateTime(),
              user.LastOnline.ToDateTime()
          ));
    }

    public IEnumerator<User> GetEnumerator() => users.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => users.GetEnumerator();
}
