/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Resource } from "./resource";
import type { DockerContainerAllOf } from "./dockerContainerAllOf";
import type { DockerContainerType } from "./dockerContainerType";

export type DockerContainer = Resource &
  DockerContainerAllOf & {
    Type: DockerContainerType;
  };
