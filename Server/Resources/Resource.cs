﻿using System.Text.Json.Serialization;

namespace Frierun.Server.Resources;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Container), "Container")]
[JsonDerivedType(typeof(HttpEndpoint), "HttpEndpoint")]
[JsonDerivedType(typeof(Volume), "Volume")]
public abstract record Resource(Guid Id, IReadOnlyList<Resource> Children)
{
    /// <summary>
    /// Enumerates all resources in the hierarchy.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Resource> AllResources => Children.SelectMany(resource => resource.AllResources).Prepend(this);

}