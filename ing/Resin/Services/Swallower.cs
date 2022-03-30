namespace Resin.Services;

internal static class Swallower
{
    internal static void Swallow<T>(Action operation, ILogger<T> log)
    {
        try
        {
            operation();
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Swallowed {0} exception: {1}", ex.GetType(), ex.Message);
        }
    }

    internal static async Task Swallow<T>(Func<Task> operation, ILogger<T> log)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Swallowed {0} exception: {1}", ex.GetType(), ex.Message);
        }
    }
}
