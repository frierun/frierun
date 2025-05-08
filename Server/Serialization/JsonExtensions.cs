using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Frierun.Server;

/// <summary>
/// Extension methods for JSON serialization.
/// From https://stackoverflow.com/questions/76866758/system-text-json-ignore-jsonignore-attribute-when-de-serializing
/// </summary>
public static class JsonExtensions
{
    public static Action<JsonTypeInfo> UnIgnoreProperties(Type type, params string[] properties) =>
        typeInfo =>
        {
            if (!type.IsAssignableFrom(typeInfo.Type) || typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            // [JsonIgnore] is implemented by setting ShouldSerialize to a function that returns false.
            foreach (var property in typeInfo.Properties.Where(p => ShouldUnIgnore(p, properties)))
            {
                property.Get ??= CreatePropertyGetter(property);
                property.Set ??= CreatePropertySetter(property);
                if (property.Get != null)
                    property.ShouldSerialize = null;
            }
        };

    public static Action<JsonTypeInfo> UnIgnorePropertiesForDeserialize(Type type, params string[] properties) =>
        typeInfo =>
        {
            if (!type.IsAssignableFrom(typeInfo.Type) || typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            // [JsonIgnore] is implemented by setting ShouldSerialize to a function that returns false.
            foreach (var property in typeInfo.Properties.Where(p => ShouldUnIgnore(p, properties)))
            {
                property.Set ??= CreatePropertySetter(property);
            }
        };

    private static bool ShouldUnIgnore(JsonPropertyInfo property, string[] properties) =>
        property.ShouldSerialize != null &&
        property.AttributeProvider?.IsDefined(typeof(JsonIgnoreAttribute), true) == true &&
        properties.Contains(property.GetMemberName());

    // CreateGetter() and CreateSetter() taken from this answer https://stackoverflow.com/a/76296944/3744182
    // To https://stackoverflow.com/questions/61869393/get-net-core-jsonserializer-to-serialize-private-members
    private delegate TValue RefFunc<TObject, TValue>(ref TObject arg);

    static Func<object, object?>? CreatePropertyGetter(JsonPropertyInfo property) =>
        property.GetPropertyInfo() is { ReflectedType: not null } info && info.GetGetMethod() is { } getMethod
            ? CreateGetter(info.ReflectedType, getMethod)
            : null;

    static Func<object, object?>? CreateGetter(Type type, MethodInfo? method)
    {
        if (method == null)
            return null;
        var myMethod = typeof(JsonExtensions).GetMethod(
            nameof(CreateGetterGeneric),
            BindingFlags.NonPublic | BindingFlags.Static
        )!;
        return (Func<object, object?>)myMethod.MakeGenericMethod(type, method.ReturnType).Invoke(null, [method])!;
    }

    static Func<object, object?> CreateGetterGeneric<TObject, TValue>(MethodInfo method)
    {
        if (method == null)
            throw new ArgumentNullException();
        if (typeof(TObject).IsValueType)
        {
            // https://stackoverflow.com/questions/4326736/how-can-i-create-an-open-delegate-from-a-structs-instance-method
            // https://stackoverflow.com/questions/1212346/uncurrying-an-instance-method-in-net/1212396#1212396
            var func = (RefFunc<TObject, TValue>)Delegate.CreateDelegate(
                typeof(RefFunc<TObject, TValue>), null, method
            );
            return o =>
            {
                var tObj = (TObject)o;
                return func(ref tObj);
            };
        }
        else
        {
            var func = (Func<TObject, TValue>)Delegate.CreateDelegate(typeof(Func<TObject, TValue>), method);
            return o => func((TObject)o);
        }
    }

    private static Action<object, object?>? CreatePropertySetter(JsonPropertyInfo property) =>
        property.GetPropertyInfo() is { ReflectedType: not null } info && info.GetSetMethod() is { } setMethod
            ? CreateSetter(info.ReflectedType, setMethod)
            : null;

    private static Action<object, object?>? CreateSetter(Type type, MethodInfo? method)
    {
        if (method == null)
            return null;
        var myMethod = typeof(JsonExtensions).GetMethod(
            nameof(CreateSetterGeneric),
            BindingFlags.NonPublic | BindingFlags.Static
        )!;
        return (Action<object, object?>)myMethod
            .MakeGenericMethod(type, method.GetParameters().Single().ParameterType).Invoke(
                null,
                [method]
            )!;
    }

    private static Action<object, object?> CreateSetterGeneric<TObject, TValue>(MethodInfo method)
    {
        if (method == null)
            throw new ArgumentNullException();
        if (typeof(TObject).IsValueType)
        {
            return (o, v) => method.Invoke(o, [v]);
        }

        var func = (Action<TObject, TValue?>)Delegate.CreateDelegate(typeof(Action<TObject, TValue?>), method);
        return (o, v) => func((TObject)o, (TValue?)v);
    }

    private static PropertyInfo? GetPropertyInfo(this JsonPropertyInfo property) =>
        property.AttributeProvider as PropertyInfo;

    private static string? GetMemberName(this JsonPropertyInfo property) =>
        (property.AttributeProvider as MemberInfo)?.Name;
}