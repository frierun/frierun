/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { ContainerContractAllOfEnv } from "./containerContractAllOfEnv";

export type ContainerContractAllOf = {
  command: string[];
  /** @nullable */
  containerName?: string | null;
  env: ContainerContractAllOfEnv;
  /** @nullable */
  imageName?: string | null;
  networkName: string;
  requireDocker: boolean;
};
