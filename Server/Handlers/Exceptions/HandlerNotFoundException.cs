using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class HandlerNotFoundException(Contract contract)
    : HandlerException(
        $"No handler found for contract {contract.Id}.",
        "Install the missing dependencies first.",
        contract
    );
