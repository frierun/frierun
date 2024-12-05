namespace Frierun.Server.Data;

public interface IHasStrings
{
    /// <summary>
    /// Applies a decorator to all strings in the contract and returns modified contract.
    /// </summary>
    public Contract ApplyStringDecorator(Func<string, string> decorator);
}