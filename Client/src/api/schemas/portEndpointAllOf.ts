/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Protocol } from "./protocol";

export type PortEndpointAllOf = {
  containerName: string;
  destinationPort: number;
  port: number;
  protocol: Protocol;
};
