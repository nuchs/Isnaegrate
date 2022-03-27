namespace JaundicedSage.Services;

public record Session()
{
    public Guid Id { get; set; }
    public DateTime When { get; set; }
}
