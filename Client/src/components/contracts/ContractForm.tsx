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
import DockerApiConnectionForm from "@/components/contracts/DockerApiConnectionForm.tsx";
import CloudflareApiConnectionForm from "@/components/contracts/CloudflareApiConnectionForm.tsx";
import {Alternative, Contract, ContractList} from "@/types.ts";

export type ContractProps<TContract extends Contract> = {
    contractId: string;
    contract: TContract;
    variants: TContract[];
    allContracts: Contract[];
    updateContract: (contractId: string, contract: Contract|null, isRefetch?: boolean) => void;
}

type Props = {
    contractId: string;
    alternatives: Alternative[];
    updateContract: (contractId: string, contract: Contract|null, isRefetch?: boolean) => void;
    allContracts: ContractList;
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
    CloudflareApiConnection: CloudflareApiConnectionForm,
    Container: ContainerForm,
    DockerApiConnection: DockerApiConnectionForm,
    Domain: DomainForm,
    HttpEndpoint: HttpEndpointForm,
    Optional: OptionalForm,
    Parameter: ParameterForm,
    PortEndpoint: PortEndpointForm,
    Selector: SelectorForm,
    SshConnection: SshConnectionForm,
    Volume: VolumeForm,
}

export default function ContractForm({contractId, alternatives, updateContract, allContracts}: Props) {
    const [variants, setVariants] = useState<Contract[]>([]);
    const contract = allContracts[contractId];

    useEffect(() => {
        setVariants(variants => {
                const filteredAlternatives = alternatives
                    .filter(alt => alt.contractId == contractId)
                    .map(alt => alt.contract);

                const refreshVariants = variants.length === 0 || !sameContract(variants[0], contract) || filteredAlternatives.length > 0;
                if (!refreshVariants) {
                    return variants;
                }

                return [contract, ...filteredAlternatives]
            }
        );
    }, [contract, contractId, alternatives]);
    
    if (variants.length == 0) {
        return <></>
    }

    const ContractForm = contractForms[contract.type];
    if (!ContractForm) {
        return <></>;
    }

    return (
        <ContractForm
            contractId={contractId}
            // @ts-expect-error contract is typed as never
            contract={contract}
            // @ts-expect-error variants are typed as never[]
            variants={variants}
            updateContract={updateContract}
            allContracts={Object.entries(allContracts).map(entry => entry[1])}
        />
    )
}