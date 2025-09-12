import type {AlternativeContract, ExecutionPlanContracts} from "@/api/schemas";

export type ContractList = ExecutionPlanContracts;
export type Contract = ContractList[keyof ContractList];
export type Alternative = {
    contractId: string;
    contract: AlternativeContract;
}