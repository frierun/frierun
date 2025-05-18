using Frierun.Server.Data;

namespace Frierun.Server;

public class HandlerNotFoundException(Contract contract)
    : Exception($"No handler found for contract: {contract.Id}")
{
    public Contract Contract => contract;
}