/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Resource } from "./resource";
import type { FileAllOf } from "./fileAllOf";
import type { FileType } from "./fileType";

export type File = Resource &
  FileAllOf & {
    Type: FileType;
  };
