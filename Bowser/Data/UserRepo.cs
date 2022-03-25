using Jaundicedsage.Grpc;
using System.Collections;
using System.Collections.Concurrent;

namespace Bowser.Data;

public class UserRepo : IEnumerable<User>
{
    private readonly ConcurrentBag<User> users = new ConcurrentBag<User>();

    public void Add(UserRecord user)
    {
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
