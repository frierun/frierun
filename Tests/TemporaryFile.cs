namespace Frierun.Tests;

public sealed class TemporaryFile(FileInfo temporaryFile) : IDisposable
{
    private volatile bool _disposed;
    public FileInfo FileInfo { get; } = temporaryFile;

    public TemporaryFile() : this(Path.GetTempFileName()) { }
    public TemporaryFile(string fileName) : this(new FileInfo(fileName)) { }

    public static implicit operator string(TemporaryFile temporaryFile)
    {
        return temporaryFile.FileInfo.FullName;
    }    
    
    public void Dispose()
    {
        try
        {
            FileInfo.Delete();
            _disposed = true;
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    ~TemporaryFile()
    {
        if (!_disposed) Dispose();
    }    
}