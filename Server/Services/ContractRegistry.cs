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
        return CreateContract(contractId.TypeName, contractId.Name);
    }
    
    /// <summary>
    /// Creates an empty contract by type name and name.
    /// </summary>
    public Contract CreateContract(string typeName, string name)
    {
        return typeName switch
        {
            nameof(CloudflareApiConnection) => new CloudflareApiConnection(name),
            nameof(CloudflareTunnel) => new CloudflareTunnel(name),
            nameof(Container) => new Container(name),
            nameof(DockerApiConnection) => new DockerApiConnection(name),
            nameof(Domain) => new Domain(name),
            nameof(HttpEndpoint) => new HttpEndpoint(name),
            nameof(Mysql) => new Mysql(name),
            nameof(Network) => new Network(name),
            nameof(Package) => new Package(name),
            nameof(Parameter) => new Parameter(name),
            nameof(Password) => new Password(name),
            nameof(Postgresql) => new Postgresql(name),
            nameof(Redis) => new Redis(name),
            nameof(SshConnection) => new SshConnection(name),
            nameof(Volume) => new Volume(name),
            _ => throw new Exception("Can't create contract type: " + typeName)
        };
    }
}