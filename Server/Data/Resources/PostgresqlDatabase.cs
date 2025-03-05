namespace Frierun.Server.Data;

public record PostgresqlDatabase(
    string User,
    string Password,
    string Database,
    string Host
) : Resource;