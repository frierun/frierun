/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Package } from "./package";

export type ApplicationAllOf = {
  name: string;
  package?: Package;
  /** @nullable */
  url?: string | null;
  /** @nullable */
  description?: string | null;
};
