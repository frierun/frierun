/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { IHandlerLazy } from "./iHandlerLazy";

export interface Contract {
  type: string;
  name: string;
  handler?: IHandlerLazy;
  installed?: boolean;
}
