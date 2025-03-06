namespace Frierun.Server.Data;

public record MysqlDatabase(
    string User,
    string Password,
    string Database,
    string Host
) : Resource;