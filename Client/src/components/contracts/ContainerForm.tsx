import {Container} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function ContainerForm({contractRef, contract, variants, updateContract}: ContractProps<Container>) {
    return (
        <BaseForm
            contractRef={contractRef}
            contract={contract}
            variants={variants}
            updateContract={updateContract}
        />
    );
}