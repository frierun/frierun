/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Resource } from "./resource";
import type { GenericHttpEndpointAllOf } from "./genericHttpEndpointAllOf";
import type { GenericHttpEndpointType } from "./genericHttpEndpointType";

export type GenericHttpEndpoint = Resource &
  GenericHttpEndpointAllOf & {
    Type: GenericHttpEndpointType;
  };
