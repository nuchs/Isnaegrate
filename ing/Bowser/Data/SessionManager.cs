using static Epoxy.Grpc.Writer;
using static Epoxy.Grpc.PropositionExtensions;

namespace Bowser.Data;

public class SessionManager
{
    private const string EventStream = "Session";
    private readonly WriterClient epoxy;
    private readonly ILogger<SessionManager> log;

    public SessionManager(WriterClient epoxy, ILogger<SessionManager> log)
    {
        this.log = log;
        this.epoxy = epoxy;
    }

    public async Task StartSession(string id)
    {
        log.LogInformation("Session starting for {id}", id);

        await epoxy.AppendAsync(NewPropositionSet(
            EventStream,
            NewProposition(
                Guid.NewGuid(),
                SessionEvents.SessionStart.ToString(),
                "Bowser",
                new { Id = id }
            )));
    }

    public async Task EndSession(string id)
    {
        log.LogInformation("Session ending for {id}", id);

        await epoxy.AppendAsync(NewPropositionSet(
           EventStream,
           NewProposition(
               Guid.NewGuid(),
               SessionEvents.SessionEnd.ToString(),
               "Bowser",
               new { Id = id }
           )));
    }
}
