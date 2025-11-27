using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Data;

public class ArgumentTests : BaseTests
{
    [Fact]
    public void Merge_SameResolvedValues_ReturnsValue()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(value);
        var arg2 = new Argument<string>(value);

        var result = arg1.Merge(arg2);

        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Merge_DifferentResolvedValues_ThrowsException()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(value);
        var arg2 = new Argument<string>(value + "_another");

        Assert.Throws<MergeException>(() => arg1.Merge(arg2));
    }

    [Fact]
    public void Merge_ResolverAndEmptyValue_ReturnsResolver()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(_ => value);
        var arg2 = new Argument<string>();

        var result = arg1.Merge(arg2);

        Assert.Null(result.Value);
        Assert.Equal(arg1.Resolver, result.Resolver);
    }

    [Fact]
    public void Merge_ResolverAndValue_ThrowsException()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>(_ => value);
        var arg2 = new Argument<string>(value);

        Assert.Throws<MergeException>(() => arg1.Merge(arg2));
    }

    [Fact]
    public void Merge_SameFuncResolvers_ReturnsResolver()
    {
        var value = Resolve<Faker>().Lorem.Word();
        Func<ExecutionPlan, string> resolver = _ => value;
        var arg1 = new Argument<string>(resolver);
        var arg2 = new Argument<string>(resolver);

        var result = arg1.Merge(arg2);

        Assert.Null(result.Value);
        Assert.Equal(arg1.Resolver, result.Resolver);
    }

    [Fact]
    public void Merge_SameClassResolvers_ReturnsResolver()
    {
        var value = Resolve<Faker>().Lorem.Word();
        Func<ExecutionPlan, string> lambda = _ => value;
        var arg1 = new Argument<string>(new ArgumentResolver<string>(lambda));
        var arg2 = new Argument<string>(new ArgumentResolver<string>(lambda));

        var result = arg1.Merge(arg2);

        Assert.Null(result.Value);
        Assert.Equal(arg1.Resolver, result.Resolver);
    }

    [Fact]
    public void Merge_SameClassWithParameterResolvers_ReturnsResolver()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var param = Resolve<Faker>().Lorem.Word();
        Func<string, ExecutionPlan, string> lambda = (p, _) => "{p} {value}";
        var arg1 = new Argument<string>(new ArgumentResolver<string, string>(param, lambda));
        var arg2 = new Argument<string>(new ArgumentResolver<string, string>(param, lambda));

        var result = arg1.Merge(arg2);

        Assert.Null(result.Value);
        Assert.Equal(arg1.Resolver, result.Resolver);
    }

    [Fact]
    public void Merge_DifferentResolvers_ThrowsException()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>((Func<ExecutionPlan, string>)(_ => value));
        var arg2 = new Argument<string>((Func<ExecutionPlan, string>)(_ => value));

        Assert.Throws<MergeException>(() => arg1.Merge(arg2));
    }


    [Fact]
    public void Merge_WithRequiredContracts_CopiesContracts()
    {
        var name = Resolve<Faker>().Lorem.Word();
        var arg1 = new Argument<string>();
        var arg2 = new Argument<string>($"{{{{Parameter:{name}:Value}}}}");

        var result = arg1.Merge(arg2);

        Assert.Single(result.RequiredContracts);
        Assert.Equal(arg2.RequiredContracts, result.RequiredContracts);
    }

    [Fact]
    public void Merge_Number_ThreatsZeroAsNull()
    {
        var arg1 = new Argument<int>(0);
        var arg2 = new Argument<int>(1);

        var result = arg1.Merge(arg2);

        Assert.Equal(1, result.Value);
    }

    [Fact]
    public void Merge_DifferentNumbers_ThrowsException()
    {
        var arg1 = new Argument<int>(1);
        var arg2 = new Argument<int>(2);

        Assert.Throws<MergeException>(() => arg1.Merge(arg2));
    }

    [Fact]
    public void Resolve_DependsOnArgument_ResolvesArgument()
    {
        var value = Resolve<Faker>().Lorem.Word();
        var parameter = Contract<Parameter>()
            .Set(p => p.Value, new Argument<string>(_ => value))
            .Generate();
        var arg = new Argument<string>($"{{{{{parameter.Ref}:Value}}}}");
        var plan = new ExecutionPlan(
            new Dictionary<ContractRef, Contract>
            {
                [parameter.Ref] = parameter.Contract,
            },
            []
        );
        Assert.False(parameter.Contract.Value.Resolved);

        arg = arg.Resolve(plan);

        Assert.False(parameter.Contract.Value.Resolved);
        Assert.Equal(value, arg.Value);

        var parameterContract = (Parameter)parameter.Contract.ResolveArguments(plan);
        Assert.True(parameterContract.Value.Resolved);
    }

    [Fact]
    public void Resolve_DependsOnProperty_ResolvesAllArguments()
    {
        var container = Contract<Container>().Generate("installed");
        var httpEndpoint = Contract<HttpEndpoint>()
            .Set(p => p.ResultSsl, new Argument<bool?>(_ => true))
            .Set(p => p.ResultHost, new Argument<string>(_ => "test.tld"))
            .Set(p => p.ResultPort, new Argument<int>(_ => 444))
            .Set(p => p.Container, container.Id)
            .Generate();
        var arg = new Argument<string>($"{{{{{httpEndpoint.Ref}:Url}}}}");
        var plan = new ExecutionPlan(
            new Dictionary<ContractRef, Contract>
            {
                [httpEndpoint.Ref] = httpEndpoint.Contract,
                [container.Ref] = container.Contract
            },
            []
        );
        Assert.False(httpEndpoint.Contract.ResultSsl.Resolved);
        Assert.False(httpEndpoint.Contract.ResultHost.Resolved);
        Assert.False(httpEndpoint.Contract.ResultPort.Resolved);

        arg = arg.Resolve(plan);
        Assert.Equal("https://test.tld:444/", arg.Value);

        var httpEndpointContract = (HttpEndpoint)httpEndpoint.Contract.ResolveArguments(plan);
        Assert.True(httpEndpointContract.ResultSsl.Resolved);
        Assert.True(httpEndpointContract.ResultHost.Resolved);
        Assert.True(httpEndpointContract.ResultPort.Resolved);
    }

    [Fact]
    public void Resolve_RecursiveArguments_ThrowsException()
    {
        var parameter1 = Contract<Parameter>().Generate();
        var parameter2 = Contract<Parameter>()
            .Set(p => p.Value, new Argument<string>($"{{{{{parameter1.Ref}:Value}}}}"))
            .Generate();

        var plan = new ExecutionPlan(
            new Dictionary<ContractRef, Contract>
            {
                [parameter1.Ref] = parameter1.Contract with
                {
                    Value = new Argument<string>($"{{{{{parameter2.Ref}:Value}}}}")
                },
                [parameter2.Ref] = parameter2.Contract
            },
            []
        );
        Assert.False(parameter2.Contract.Value.Resolved);

        Assert.Throws<Exception>(() => parameter2.Contract.Value.Resolve(plan));
    }
}