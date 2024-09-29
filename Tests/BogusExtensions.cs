using System.Linq.Expressions;
using System.Runtime.Serialization;
using Bogus;

namespace Frierun.Tests;

public static class BogusExtensions
{
    public static Faker<T> SkipConstructor<T>(this Faker<T> fakerOfT) where T : class
    {
#pragma warning disable SYSLIB0050
        return fakerOfT.CustomInstantiator( _ => (T)FormatterServices.GetUninitializedObject(typeof(T)));
#pragma warning restore SYSLIB0050
    }
    
    public static Faker<T> UniqueRuleFor<T,TProperty>(
        this Faker<T> fakerOfT,
        Expression<Func<T, TProperty>> property,
        Func<Faker, T, TProperty> setter,
        HashSet<TProperty> uniqueValues
        )
        where T:class
    {
        return fakerOfT.RuleFor(property, UniqueValue(setter, uniqueValues));
    }

    public static Faker<T> UniqueRuleFor<T,TProperty>(
        this Faker<T> fakerOfT,
        Expression<Func<T, TProperty>> property,
        Func<Faker, TProperty> setter,
        HashSet<TProperty> uniqueValues
    )
        where T:class
    {
        return fakerOfT.RuleFor(property, UniqueValue<T, TProperty>((f,t) => setter(f), uniqueValues));
    }
    

    /// <summary>
    /// Creates delegate that generates a unique value for the property
    /// </summary>
    private static Func<Faker, T,TProperty> UniqueValue<T, TProperty>(
        Func<Faker, T, TProperty> setter,
        HashSet<TProperty> uniqueValues
    ) 
    {
        return (f, t) =>
        {
            TProperty value;
            int tries = 0;
            do
            {
                if (++tries > 100)
                {
                    throw new InvalidOperationException("Could not generate a unique value");
                }
                value = setter(f, t);
            } while (!uniqueValues.Add(value));
            return value;
        };
    }
}