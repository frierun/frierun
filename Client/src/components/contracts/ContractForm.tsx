import {Domain, ExecutionPlanContractsItem, HttpEndpoint, Optional, Selector, Volume} from "@/api/schemas";
import {useEffect, useState} from "react";
import HttpEndpointForm from "@/components/contracts/HttpEndpointForm.tsx";
import DomainForm from "@/components/contracts/DomainForm.tsx";
import VolumeForm from "@/components/contracts/VolumeForm.tsx";
import SelectorForm from "@/components/contracts/SelectorForm.tsx";
import OptionalForm from "@/components/contracts/OptionalForm.tsx";

export type Contract = ExecutionPlanContractsItem;

type Props = {
    contract: Contract;
    alternatives: Contract[];
    updateContract: (contract: Contract, isRefetch?: boolean) => void;
    allContracts: Contract[];
}

const sameContract = (a: Contract, b: Contract) => {
    return a.type === b.type && a.name === b.name;
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

    switch (contract.type) {
        case 'HttpEndpoint':
            return (
                <HttpEndpointForm
                    contract={contract}
                    variants={variants as HttpEndpoint[]}
                    updateContract={updateContract}
                    allContracts={allContracts}
                />
            );
        case 'Domain':
            return (
                <DomainForm
                    contract={contract}
                    variants={variants as Domain[]}
                    updateContract={updateContract}
                />
            );
        case 'Volume':
            return (
                <VolumeForm
                    contract={contract}
                    variants={variants as Volume[]}
                    updateContract={updateContract}
                />
            );
        case 'Selector':
            return (
                <SelectorForm
                    contract={contract}
                    variants={variants as Selector[]}
                    updateContract={updateContract}
                />
            );
        case 'Optional':
            return (
                <OptionalForm
                    contract={contract}
                    variants={variants as Optional[]}
                    updateContract={updateContract}
                />
            );
        default:
            return <></>
    }
}