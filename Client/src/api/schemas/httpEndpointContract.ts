/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { HttpEndpointContractAllOf } from "./httpEndpointContractAllOf";
import type { HttpEndpointContractType } from "./httpEndpointContractType";

export type HttpEndpointContract = Contract &
  HttpEndpointContractAllOf & {
    Type: HttpEndpointContractType;
  };
