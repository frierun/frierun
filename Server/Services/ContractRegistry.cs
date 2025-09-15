using Frierun.Server.Data;

namespace Frierun.Server;

public class ContractRegistry
{
    /// <summary>
    /// Gets a contract type by name.
    /// </summary>
    public Type GetContractType(string contractTypeName)
    {
        var contractType = Type.GetType($"Frierun.Server.Data.{contractTypeName}");
        if (contractType == null)
        {
            throw new Exception($"Contract type not found: {contractTypeName}");
        }

        return contractType;
    }

    /// <summary>
    /// Creates an empty contract by id
    /// </summary>
    public Contract CreateContract(ContractId contractId)
    {
        return CreateContract(contractId.TypeName);
    }
    
    /// <summary>
    /// Creates an empty contract by type name and name.
    /// </summary>
    public Contract CreateContract(string typeName)
    {
        return typeName switch
        {
            nameof(CloudflareApiConnection) => new CloudflareApiConnection(),
            nameof(CloudflareTunnel) => new CloudflareTunnel(),
            nameof(Container) => new Container(),
            nameof(DockerApiConnection) => new DockerApiConnection(),
            nameof(Domain) => new Domain(),
            nameof(HttpEndpoint) => new HttpEndpoint(),
            nameof(Mysql) => new Mysql(),
            nameof(Network) => new Network(),
            nameof(Parameter) => new Parameter(),
            nameof(Password) => new Password(),
            nameof(Postgresql) => new Postgresql(),
            nameof(Redis) => new Redis(),
            nameof(SshConnection) => new SshConnection(),
            nameof(Volume) => new Volume(),
            _ => throw new Exception("Can't create contract type: " + typeName)
        };
    }
}