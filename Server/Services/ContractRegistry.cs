using Frierun.Server.Data;

namespace Frierun.Server.Services;

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
            "Container" => new Container(name),
            "Parameter" => new Parameter(name),
            _ => throw new Exception("Can't create contract type: " + typeName)
        };
    }
}