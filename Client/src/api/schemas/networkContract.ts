/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { NetworkContractAllOf } from "./networkContractAllOf";
import type { NetworkContractType } from "./networkContractType";

export type NetworkContract = Contract &
  NetworkContractAllOf & {
    Type: NetworkContractType;
  };
