using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public record HandlerExceptionResult(string Message, string Solution, Contract Contract);