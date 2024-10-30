﻿namespace Frierun.Server.Resources;

public record Package(
    string Url,
    IReadOnlyList<ResourceDefinition> Children,
    string? Name = null
) : ResourceDefinition<Application>(Children, Name);