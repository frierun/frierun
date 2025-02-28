using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public record ContractDependency(ContractId Preceding, ContractId Following);