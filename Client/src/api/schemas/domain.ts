/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { DomainAllOf } from "./domainAllOf";
import type { DomainType } from "./domainType";

export type Domain = Contract &
  DomainAllOf & {
    Type: DomainType;
  };
