/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Resource } from "./resource";
import type { LocalPathAllOf } from "./localPathAllOf";
import type { LocalPathType } from "./localPathType";

export type LocalPath = Resource &
  LocalPathAllOf & {
    Type: LocalPathType;
  };
