/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Application } from "./application";
import type { Container } from "./container";
import type { Network } from "./network";
import type { File } from "./file";
import type { HttpEndpoint } from "./httpEndpoint";
import type { Mount } from "./mount";
import type { PortHttpEndpoint } from "./portHttpEndpoint";
import type { TraefikHttpEndpoint } from "./traefikHttpEndpoint";
import type { Volume } from "./volume";
import type { Resource } from "./resource";

export type ApplicationResponseResourcesItem =
  | Application
  | Container
  | Network
  | File
  | HttpEndpoint
  | Mount
  | PortHttpEndpoint
  | TraefikHttpEndpoint
  | Volume
  | Resource;
