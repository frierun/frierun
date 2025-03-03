using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Data;

public class ExecutionPlanTests : BaseTests
{
    [Fact]
    public void GetDependentResources_ContractWithoutResource_ReturnsResourcesRecursively()
    {
        var package = Factory<Package>().Generate() with { ApplicationDescription = "{{Parameter::Value}}"};
        var executionService = Resolve<ExecutionService>();
        var plan = executionService.Create(package);
        plan.Install();
        
        var resources = plan.GetDependentResources(package.Id).ToList();
        
        Assert.Single(resources);
        Assert.IsType<ResolvedParameter>(resources[0]);
    }
    
    [Fact]
    public void GetDependentResources_NotUniqueContract_ReturnsResourceOnlyOnce()
    {
        var package = Factory<Package>().Generate() with
        {
            ApplicationDescription = "{{Parameter::Value}}",
            Contracts = [new Parameter("")]
        };
        
        var executionService = Resolve<ExecutionService>();
        var plan = executionService.Create(package);
        plan.Install();
        
        var resources = plan.GetDependentResources(package.Id).ToList();
        
        Assert.Single(resources);
        Assert.IsType<ResolvedParameter>(resources[0]);
    }
    
    [Fact]
    public void GetDependentResources_ResourceDependsOnResource_ReturnsFirstResourceOnly()
    {
        var container = Factory<Container>().Generate();   
        var package = Factory<Package>().Generate() with
        {
            Contracts = [container]
        };
        
        var executionService = Resolve<ExecutionService>();
        var plan = executionService.Create(package);
        plan.Install();
        
        var packageResources = plan.GetDependentResources(package.Id).ToList();
        var containerResources = plan.GetDependentResources(container.Id).ToList();
        
        Assert.Single(packageResources);
        Assert.IsType<DockerContainer>(packageResources[0]);
        Assert.Single(containerResources);
        Assert.IsType<DockerNetwork>(containerResources[0]);
    }    
}