/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { PortEndpointAllOf } from "./portEndpointAllOf";
import type { PortEndpointType } from "./portEndpointType";

export type PortEndpoint = Contract &
  PortEndpointAllOf & {
    type: PortEndpointType;
  };
