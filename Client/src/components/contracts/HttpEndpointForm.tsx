import {useEffect, useState} from "react";
import {ExecutionPlanContractsItem, HttpEndpoint, Package} from "@/api/schemas";
import DomainForm, {Domain} from "@/components/contracts/DomainForm.tsx";

const defaultPort = 1080;

type Props = {
    contract: HttpEndpoint;
    contracts: ExecutionPlanContractsItem[];
    updateContracts: (contracts: Package['contracts']) => void;
}

const findDomain = (contract: HttpEndpoint, contracts: ExecutionPlanContractsItem[]) => {
    const domainContract = contracts
        .filter(contract => contract.type === 'Domain')
        .find(domain => domain.name === contract.domain);

    if (!domainContract) {
        return {
            typeName: '',
        };
    }

    return {
        typeName: domainContract.handler?.typeName ?? '',
        applicationName: domainContract.handler?.applicationName,
        domain: domainContract.value
    }
}

export default function HttpEndpointForm({contract, contracts, updateContracts}: Props) {
    const [domain, setDomain] = useState<Domain>({typeName: ''});
    const [port, setPort] = useState(0);
    const hasCloudflare = contract.handler?.typeName === 'CloudflareHttpEndpointHandler';
    const hasTraefik = contract.handler?.typeName === 'TraefikHttpEndpointHandler';
    const traefikApplication = hasTraefik ? contract.handler?.applicationName : null;
    const [handlerType, setHandlerType] = useState('');

    useEffect(() => {
        setDomain(findDomain(contract, contracts));

        setPort(Object.values(contracts)
            .filter(contract => contract.type === 'PortEndpoint')
            .find(port => port.container === contract.container && port.port === contract.port)
            ?.externalPort ?? defaultPort)
        setHandlerType(contract.handler?.typeName ?? 'PortHttpEndpointHandler');
    }, [contract, contracts]);

    const updateHandlerType = (handlerType: string) => {
        setHandlerType(handlerType);
        if (handlerType === 'TraefikHttpEndpointHandler') {
            updateDomain(findDomain(contract, contracts));
        } else {
            updatePort(port.toString());
        }
    }

    const updateDomain = (domain: Domain) => {
        setDomain(domain);
        updateContracts([
            {
                ...contract,
                handler: {
                    typeName: 'TraefikHttpEndpointHandler',
                    applicationName: traefikApplication
                }
            },
            {
                type: 'Domain',
                name: contract.name,
                value: domain.domain,
                handler: {
                    typeName: domain.typeName,
                    applicationName: domain.applicationName
                }
            }
        ]);
    }

    const updatePort = (portValue: string) => {
        let port = parseInt(portValue);
        if (port < 1 || port > 65535 || isNaN(port)) {
            port = defaultPort;
        }
        setPort(port);
        updateContracts([
            {
                ...contract,
                handler: {
                    typeName: 'PortHttpEndpointHandler'
                },
            },
            {
                type: 'PortEndpoint',
                name: `${contract.container}:${contract.port.toString()}/tcp`,
                protocol: 'Tcp',
                container: contract.container,
                port: contract.port,
                externalPort: port,
            }
        ]);
    }

    return (
        <div>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>Http endpoint to port </label>{contract.port}
                {contract.container && ` in container ${contract.container}`}
            </div>
            {(hasTraefik || hasCloudflare) && (
                <fieldset className={"flex gap-4"}>
                    {hasTraefik && (
                        <div>
                            <input
                                type="radio"
                                id={"TraefikHttpEndpointHandlerRadio"}
                                value="TraefikHttpEndpointHandler"
                                checked={handlerType === "TraefikHttpEndpointHandler"}
                                onChange={e => {
                                    updateHandlerType(e.target.value);
                                }}
                            >
                            </input>
                            <label htmlFor={"TraefikHttpEndpointHandlerRadio"}>Traefik
                            </label>
                        </div>
                    )}
                    {hasCloudflare && (
                        <div>
                            <input
                                type="radio"
                                id={"CloudflareHttpEndpointHandlerRadio"}
                                value="CloudflareHttpEndpointHandler"
                                checked={handlerType === "CloudflareHttpEndpointHandler"}
                                onChange={e => {
                                    updateHandlerType(e.target.value);
                                }}
                            />
                            <label htmlFor={"CloudflareHttpEndpointHandlerRadio"}>
                                Cloudflare
                            </label>
                        </div>
                    )}
                    <div>
                        <input
                            type="radio"
                            id={"PortHttpEndpointHandlerRadio"}
                            value="PortHttpEndpointHandler"
                            checked={handlerType === "PortHttpEndpointHandler"}
                            onChange={e => {
                                updateHandlerType(e.target.value);
                            }}
                        />
                        <label htmlFor={"PortHttpEndpointHandlerRadio"}>
                            Port
                        </label>
                    </div>
                </fieldset>
            )}
            {handlerType === 'TraefikHttpEndpointHandler' && (
                <DomainForm domain={domain} setDomain={updateDomain}/>
            )}
            {handlerType === 'PortHttpEndpointHandler' && (
                <div>
                    <label className={"inline-block w-48"}>
                        Port:
                    </label>
                    <input value={port} onChange={e => {
                        updatePort(e.target.value);
                    }}/>
                </div>
            )}
        </div>
    );
}