/**
 * Generated by orval v7.5.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { PackageAllOf } from "./packageAllOf";
import type { PackageType } from "./packageType";

export type Package = Contract &
  PackageAllOf & {
    Type: PackageType;
  } & Required<
    Pick<
      Contract &
        PackageAllOf & {
          Type: PackageType;
        },
      | "Type"
      | "contracts"
      | "tags"
      | "contracts"
      | "tags"
      | "contracts"
      | "tags"
    >
  >;
