/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { ContainerAllOf } from "./containerAllOf";
import type { ContainerType } from "./containerType";

export type Container = Contract &
  ContainerAllOf & {
    Type: ContainerType;
  };
