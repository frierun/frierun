import {Container} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function ContainerForm({contractId, contract, variants, updateContract}: ContractProps<Container>) {
    return (
        <BaseForm
            contractId={contractId}
            contract={contract}
            variants={variants}
            updateContract={updateContract}
        />
    );
}