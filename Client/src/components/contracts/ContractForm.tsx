import {ExecutionPlanContractsItem} from "@/api/schemas";
import React, {useEffect, useState} from "react";
import HttpEndpointForm from "@/components/contracts/HttpEndpointForm.tsx";
import DomainForm from "@/components/contracts/DomainForm.tsx";
import VolumeForm from "@/components/contracts/VolumeForm.tsx";
import SelectorForm from "@/components/contracts/SelectorForm.tsx";
import OptionalForm from "@/components/contracts/OptionalForm.tsx";
import SshConnectionForm from "@/components/contracts/SshConnectionForm.tsx";
import ContainerForm from "@/components/contracts/ContainerForm.tsx";
import ParameterForm from "@/components/contracts/ParameterForm.tsx";
import PortEndpointForm from "@/components/contracts/PortEndpointForm.tsx";

export type Contract = ExecutionPlanContractsItem;

export type ContractProps<TContract extends Contract> = {
    contract: TContract;
    variants: TContract[];
    allContracts: Contract[];
    updateContract: (contract: Contract, isRefetch?: boolean) => void;
}

type Props = {
    contract: Contract;
    alternatives: Contract[];
    updateContract: (contract: Contract, isRefetch?: boolean) => void;
    allContracts: Contract[];
}

const sameContract = (a: Contract, b: Contract) => {
    return a.type === b.type && a.name === b.name;
}

type ContractsByTypeName = {
    [P in Contract['type']]: Extract<Contract, { type: P }>
}
type FormsByTypeName = {
    [P in Contract['type']]: (props: ContractProps<ContractsByTypeName[P]>) => React.JSX.Element;
}
const contractForms: Partial<FormsByTypeName> = {
    Container: ContainerForm,
    Domain: DomainForm,
    HttpEndpoint: HttpEndpointForm,
    Optional: OptionalForm,
    Parameter: ParameterForm,
    PortEndpoint: PortEndpointForm,
    Selector: SelectorForm,
    SshConnection: SshConnectionForm,
    Volume: VolumeForm,
}

export default function ContractForm({contract, alternatives, updateContract, allContracts}: Props) {
    const [variants, setVariants] = useState<Contract[]>([]);

    useEffect(() => {
        setVariants(variants => {
                const filteredAlternatives = alternatives.filter(alt => sameContract(alt, contract));
                const refreshVariants = variants.length === 0 || !sameContract(variants[0], contract) || filteredAlternatives.length > 0;
                if (!refreshVariants) {
                    return variants;
                }

                return [contract, ...filteredAlternatives]
            }
        );
    }, [contract, alternatives]);

    if (variants.length == 0) {
        return <></>
    }

    const ContractForm = contractForms[contract.type];
    if (!ContractForm) {
        return <></>;
    }

    return (
        <ContractForm
            // @ts-expect-error contract is typed as never
            contract={contract}
            // @ts-expect-error variants are typed as never[]
            variants={variants}
            updateContract={updateContract}
            allContracts={allContracts}
        />
    )
}