using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Jaundicedsage.Grpc;
using static Jaundicedsage.Grpc.UserDir;

namespace JaundicedSage.Services;

public class UserDirService : UserDirBase
{
    private readonly ILogger<UserDirService> log;
    private readonly UserRepo userRepo;

    public UserDirService(UserRepo userRepo, ILogger<UserDirService> log)
    {
        this.userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        this.log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public override Task<UserListing> GetAllUsers(AllUserRequest request, ServerCallContext context)
    {
        log.LogInformation("Getting all users");

        var listing = new UserListing();
        listing.Users.AddRange(userRepo.GetAllUsers().Select(u => new UserRecord() { 
            Id = u.Id.ToString(),
            Name = u.Name,
            Org = u.Org,
            Title= u.Title,
            CertExpires = Timestamp.FromDateTime(u.CertExpiry),
            LastOnline = Timestamp.FromDateTime(u.LastOnline)
        }));

        return Task.FromResult(listing);
    }

    public override Task<UserResponse> GetUser(UserRequest request, ServerCallContext context)
    {
        log.LogInformation("Getting user with id {}", request.Id);

        var user = userRepo.GetUser(request.Id);

        if (user == null)
        {
            log.LogWarning("No such user {}", request.Id);

            return Task.FromResult(new UserResponse());
        }
        else
        {
            return Task.FromResult(new UserResponse() { 
                Value = new UserRecord() { 
                    Id = user.Id.ToString(), 
                    Name = user.Name,
                    Org= user.Org,
                    Title= user.Title,
                    CertExpires = Timestamp.FromDateTime(user.CertExpiry),
                    LastOnline = Timestamp.FromDateTime(user.LastOnline)
                }
            });
        }

    }
}
