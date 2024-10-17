namespace Frierun.Server.Providers;

public class Parameter<T>(T defaultValue): Parameter
{
    public T DefaultValue { get; } = defaultValue;
    public T? UserValue { get; set; }
    
    public T Value => UserValue ?? DefaultValue;
}

public class Parameter
{
    
}