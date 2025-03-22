/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { ConnectExternalContainer } from "./connectExternalContainer";
import type { Container } from "./container";
import type { Dependency } from "./dependency";
import type { File } from "./file";
import type { HttpEndpoint } from "./httpEndpoint";
import type { Mount } from "./mount";
import type { Mysql } from "./mysql";
import type { Network } from "./network";
import type { Package } from "./package";
import type { Parameter } from "./parameter";
import type { Password } from "./password";
import type { PortEndpoint } from "./portEndpoint";
import type { Postgresql } from "./postgresql";
import type { Redis } from "./redis";
import type { Selector } from "./selector";
import type { Substitute } from "./substitute";
import type { Volume } from "./volume";

export type PackageAllOfContractsItem =
  | ConnectExternalContainer
  | Container
  | Dependency
  | File
  | HttpEndpoint
  | Mount
  | Mysql
  | Network
  | Package
  | Parameter
  | Password
  | PortEndpoint
  | Postgresql
  | Redis
  | Selector
  | Substitute
  | Volume;
