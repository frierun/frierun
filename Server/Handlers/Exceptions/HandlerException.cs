using System.Runtime.Serialization;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

[DataContract]
public class HandlerException(string message, string solution, Contract contract)
    : Exception(message)
{
    public HandlerExceptionResult Result => new(Message, solution, contract);
}