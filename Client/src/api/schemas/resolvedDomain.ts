/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Resource } from "./resource";
import type { ResolvedDomainAllOf } from "./resolvedDomainAllOf";
import type { ResolvedDomainType } from "./resolvedDomainType";

export type ResolvedDomain = Resource &
  ResolvedDomainAllOf & {
    Type: ResolvedDomainType;
  };
