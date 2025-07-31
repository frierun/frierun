import {Container} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function ContainerForm({contract, variants, updateContract}: ContractProps<Container>) {
    return (
        <BaseForm
            contract={contract}
            variants={variants}
            updateContract={updateContract}
        />
    );
}