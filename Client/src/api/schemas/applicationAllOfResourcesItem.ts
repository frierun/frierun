/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Application } from "./application";
import type { DockerAttachedNetwork } from "./dockerAttachedNetwork";
import type { DockerContainer } from "./dockerContainer";
import type { DockerNetwork } from "./dockerNetwork";
import type { DockerPortEndpoint } from "./dockerPortEndpoint";
import type { DockerVolume } from "./dockerVolume";
import type { GenericHttpEndpoint } from "./genericHttpEndpoint";
import type { GeneratedPassword } from "./generatedPassword";
import type { LocalPath } from "./localPath";
import type { MysqlDatabase } from "./mysqlDatabase";
import type { PostgresqlDatabase } from "./postgresqlDatabase";
import type { RedisDatabase } from "./redisDatabase";
import type { ResolvedDomain } from "./resolvedDomain";
import type { ResolvedParameter } from "./resolvedParameter";
import type { TraefikHttpEndpoint } from "./traefikHttpEndpoint";

export type ApplicationAllOfResourcesItem =
  | Application
  | DockerAttachedNetwork
  | DockerContainer
  | DockerNetwork
  | DockerPortEndpoint
  | DockerVolume
  | GenericHttpEndpoint
  | GeneratedPassword
  | LocalPath
  | MysqlDatabase
  | PostgresqlDatabase
  | RedisDatabase
  | ResolvedDomain
  | ResolvedParameter
  | TraefikHttpEndpoint;
