using Frierun.Server.Data;

namespace Tests.Integration.Handlers.Base;

public class RedisHandlerTests : TestWithDocker
{
    [Fact]
    public async Task Install_RedisContract_CredentialsAreCorrect()
    {
        var package = new Package(
            Name: "redis-client",
            Contracts:
            [
                new Redis(),
                new Container(
                    Name: "redis-client"
                )
                {
                    ImageName = "redis:7"
                }
            ]
        );
        var application = InstallPackage(package);

        var container = application.Contracts
            .OfType<Container>()
            .Single(container => container.Name == "redis-client");
        var database = application.Contracts.OfType<Redis>().Single();
        Assert.True(container.Installed);
        Assert.True(database.Installed);

        var host = database.Host.Value;
        Assert.NotNull(host);
        Assert.Equal("redis-client-redis", host);

        // try to connect to the database from the client
        var queries = new[]
        {
            "SET test_key 123",
            "INCRBY test_key 123",
            "GET test_key"
        };

        string stdout = "";
        foreach (var query in queries)
        {
            var command = new List<string>
            {
                "redis-cli",
                "-h", host,
            };
            command.AddRange(query.Split(" "));
            (stdout, _) = await DockerService.ExecInContainer(
                container.ContainerName,
                command
            );
        }

        Assert.Contains("246", stdout);

        // clean up
        UninstallApplication(application);
    }
}