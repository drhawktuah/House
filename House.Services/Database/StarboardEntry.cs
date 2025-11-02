namespace House.House.Services.Database;

public class StarboardEntry : DatabaseEntity
{
    public ulong MessageID { get; set; } = default!;
    public ulong StaboardMessageID { get; set; } = default!;
    public ulong GuildID { get; set; } = default!;
}
