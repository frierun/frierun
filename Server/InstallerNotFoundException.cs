using Frierun.Server.Data;

namespace Frierun.Server;

public class InstallerNotFoundException(Contract contract)
    : Exception($"No installer found for contract: {contract.Id}")
{
    public Contract Contract => contract;
}