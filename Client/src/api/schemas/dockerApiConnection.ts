/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { DockerApiConnectionAllOf } from "./dockerApiConnectionAllOf";
import type { DockerApiConnectionType } from "./dockerApiConnectionType";

export type DockerApiConnection = Contract &
  DockerApiConnectionAllOf & {
    type: DockerApiConnectionType;
  };
