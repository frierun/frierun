/**
 * Generated by orval v7.5.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import type { Contract } from "./contract";
import type { SubstituteAllOf } from "./substituteAllOf";
import type { SubstituteType } from "./substituteType";

export type Substitute = Contract &
  SubstituteAllOf & {
    Type: SubstituteType;
  } & Required<
    Pick<
      Contract &
        SubstituteAllOf & {
          Type: SubstituteType;
        },
      "Type" | "originalId"
    >
  >;
