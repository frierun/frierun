/**
 * Generated by orval v7.5.0 🍺
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
  } & Required<
    Pick<
      Resource &
        DockerContainerAllOf & {
          Type: DockerContainerType;
        },
      "Type" | "name"
    >
  >;
