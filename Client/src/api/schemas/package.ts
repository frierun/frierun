/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Volume } from "./volume";

export interface Package {
  imageName: string;
  name: string;
  port: number;
  requireDocker: boolean;
  /** @nullable */
  volumes?: Volume[] | null;
}
