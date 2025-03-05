using Frierun.Server.Data;
using File = Frierun.Server.Data.File;

namespace Frierun.Server;

public class ContractRegistry
{
    public Type GetContractType(string contractTypeName)
    {
        var contractType = Type.GetType($"Frierun.Server.Data.{contractTypeName}");
        if (contractType == null)
        {
            throw new Exception($"Contract type not found: {contractTypeName}");
        }

        return contractType;
    }

    public Contract CreateContract(ContractId contractId)
    {
        return CreateContract(contractId.Type.Name, contractId.Name);
    }
    
    public Contract CreateContract(string typeName, string name)
    {
        return typeName switch
        {
            nameof(Container) => new Container(name),
            nameof(File) => new File(name),
            nameof(HttpEndpoint) => new HttpEndpoint(name),
            nameof(Mysql) => new Mysql(name),
            nameof(Network) => new Network(name),
            nameof(Package) => new Package(name),
            nameof(Parameter) => new Parameter(name),
            nameof(Password) => new Password(name),
            nameof(Volume) => new Volume(name),
            _ => throw new Exception("Can't create contract type: " + typeName)
        };
    }
}