using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class HandlerNotFoundException(ContractId contractId, Contract contract)
    : HandlerException(
        $"No handler found for contract {contractId}.",
        "Install the missing dependencies first.",
        contract
    );
