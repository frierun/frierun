using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;
using NSubstitute;

namespace Frierun.Tests;

public class UninstallServiceTests : BaseTests
{
    [Fact]
    public void Handle_FrierunApplication_ClearsState()
    {
        var docker = InstallPackage("docker");
        var frierun = InstallPackage("frierun");
        var state = State;
        Assert.Equal(2, state.Applications.Count());
        var uninstallService = Resolve<UninstallService>();

        uninstallService.Handle(frierun);
        uninstallService.Handle(docker);

        Assert.Empty(state.Applications);
    }

    [Fact]
    public void Handle_DependentApplication_WorksProperlyOnCorrectOrder()
    {
        var docker = InstallPackage("docker");
        var traefik = InstallPackage("traefik");
        var frierun = InstallPackage("frierun");
        var state = State;
        Assert.Equal(3, state.Applications.Count());
        var uninstallService = Resolve<UninstallService>();

        uninstallService.Handle(frierun);
        uninstallService.Handle(traefik);
        uninstallService.Handle(docker);

        Assert.Empty(state.Applications);
    }

    [Fact]
    public void Handle_DependentApplication_ThrowsExceptionOnWrongOrder()
    {
        InstallPackage("docker");
        InstallPackage("static-zone");
        var traefik = InstallPackage("traefik");
        var application = InstallPackage("frierun");

        var httpEndpoint = State.GetContract<HttpEndpoint>(application);
        Assert.True(httpEndpoint.Installed);

        Assert.Throws<Exception>(() => Resolve<UninstallService>().Handle(traefik));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Handle_DependentContracts_UninstallsInCorrectOrder(bool reverse)
    {
        var handler = Substitute.For<IHandler>();
        handler.Initialize(Arg.Any<Contract>(), Arg.Any<ApplicationContext>())
            .Returns(info => [new ContractList { [info.Arg<ApplicationContext>()] = info.Arg<Contract>() }]);
        handler.Install(Arg.Any<Contract>(), Arg.Any<ExecutionPlan>())
            .Returns(info => info.Arg<Contract>());

        var parameters = Contract<Parameter>().Set(p => p.Handler, handler).Generate(5);

        handler.When(h => h.Uninstall(Arg.Any<Parameter>()))
            .Do(
                Callback.First(info => Assert.Equal(parameters[4].Contract.Value, info.Arg<Parameter>().Value))
                    .Then(info => Assert.Equal(parameters[3].Contract.Value, info.Arg<Parameter>().Value))
                    .Then(info => Assert.Equal(parameters[2].Contract.Value, info.Arg<Parameter>().Value))
                    .Then(info => Assert.Equal(parameters[1].Contract.Value, info.Arg<Parameter>().Value))
                    .Then(info => Assert.Equal(parameters[0].Contract.Value, info.Arg<Parameter>().Value))
            );

        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                parameters[0],
                parameters[1].With(p => p with { DependsOn = [parameters[0].Ref] }),
                parameters[2].With(p => p with { DependsOn = [parameters[1].Ref] }),
                parameters[3].With(p => p with { DependsOn = [parameters[2].Ref] }),
                parameters[4].With(p => p with { DependsOn = [parameters[3].Ref] }),
            ]
        };
        var application = InstallPackage(package);
        if (reverse)
        {
            application = application with { Contracts = new ContractList(application.Contracts.Reverse()) };
        }

        Resolve<UninstallService>().Handle(application);
    }
}