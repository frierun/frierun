using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class HandlerNotFoundException(ContractRef contractRef, Contract contract)
    : HandlerException(
        $"No handler found for contract {contractRef}.",
        "Install the missing dependencies first.",
        contract
    );
