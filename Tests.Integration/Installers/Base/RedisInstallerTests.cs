using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Installers.Base;

public class RedisInstallerTests : TestWithDocker
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
                    Name: "redis-client",
                    ImageName: "redis:7"
                )
            ]
        );
        var application = InstallPackage(package);
        
        var container = application.Resources.OfType<DockerContainer>().First();
        var database = application.Resources.OfType<RedisDatabase>().First();
        Assert.Equal("redis-client-redis", database.Host);
        
        // try to connect to the database from the client
        var dockerService = Services.GetRequiredService<DockerService>();

        var queries = new[]{
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
                "-h", database.Host,
            };
            command.AddRange(query.Split(" "));
            (stdout, _) = await dockerService.ExecInContainer(
                container.Name,
                command
            );
        }
        
        Assert.Contains("246", stdout);
        
        // clean up
        UninstallApplication(application);
    }
}