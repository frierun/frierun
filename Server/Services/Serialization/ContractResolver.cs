using Frierun.Server.Models;
using Newtonsoft.Json.Serialization;

namespace Frierun.Server.Services.Serialization;

public class ContractResolver(PackageRegistry packageRegistry) : DefaultContractResolver
{
    /// <inheritdoc />
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        if (objectType == typeof(Package))
        {
            var contract = base.CreateObjectContract(objectType);
            contract.Converter = new PackageConverter(packageRegistry);
            return contract;
        }
        
        return base.CreateObjectContract(objectType);
    }
}