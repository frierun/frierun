namespace Frierun.Server.Data;

public interface ICanMerge
{
    /// <summary>
    /// Merges contracts restrictions of the same type 
    /// </summary>
    public Contract Merge(Contract other);
}