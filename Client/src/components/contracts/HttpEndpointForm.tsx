import {useEffect, useState} from "react";
import {GetPackagesIdPlan200Item, HttpEndpoint, Package} from "@/api/schemas";

const defaultPort = 1080;

type Props = {
    contract: HttpEndpoint;
    contracts: GetPackagesIdPlan200Item[];
    updateContracts: (contracts: Package['contracts']) => void;
}

export default function HttpEndpointForm({contract, contracts, updateContracts}: Props) {

    const [domain, setDomain] = useState('');
    const [port, setPort] = useState(0);
    const hasTraefik = contract.installer?.typeName === 'TraefikHttpEndpointInstaller';
    const [installerType, setInstallerType] = useState('');

    useEffect(() => {
        setDomain(contract.domainName ?? '');
        setPort(contracts
            .filter(contract => contract.Type === 'PortEndpoint')
            .find(port => port.containerName === contract.containerName && port.port === contract.port)
            ?.destinationPort ?? defaultPort)
        setInstallerType(contract.installer?.typeName ?? 'PortHttpEndpointInstaller');
    }, [contract, contracts]);

    const updateInstallerType = (installerType: string) => {
        setInstallerType(installerType);
        if (installerType === 'TraefikHttpEndpointInstaller') {
            updateDomain(domain);
        } else {
            updatePort(port.toString());
        }
    }

    const updateDomain = (domainName: string) => {
        setDomain(domainName);
        updateContracts([{
            ...contract,
            installer: {
                typeName: 'TraefikHttpEndpointInstaller'
            },
            domainName
        }]);
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
                installer: {
                    typeName: 'PortHttpEndpointInstaller'
                },
                domainName: null
            },
            {
                Type: 'PortEndpoint',
                name: `${contract.containerName}:${contract.port}/tcp`,
                protocol: 'Tcp',
                containerName: contract.containerName,
                port: contract.port,
                destinationPort: port,
                dependsOn: [],
                dependencyOf: []
            }
        ]);
    }

    return (
        <>
            <div>
                <div className={"my-1.5"}>
                    <label className={"inline-block w-48"}>Http endpoint to port </label>{contract.port}
                    {contract.containerName && ` in container ${contract.containerName}`}
                </div>
                {hasTraefik && (
                    <fieldset className={"flex gap-4"}>
                        <div>
                            <input
                                type="radio"
                                id={"TraefikHttpEndpointInstallerRadio"}
                                value="TraefikHttpEndpointInstaller"
                                checked={installerType === "TraefikHttpEndpointInstaller"}
                                onChange={e => updateInstallerType(e.target.value)}
                            >
                            </input>
                            <label htmlFor={"TraefikHttpEndpointInstallerRadio"}>Traefik
                            </label>
                        </div>
                        <div>
                            <input
                                type="radio"
                                id={"PortHttpEndpointInstallerRadio"}
                                value="PortHttpEndpointInstaller"
                                checked={installerType === "PortHttpEndpointInstaller"}
                                onChange={e => updateInstallerType(e.target.value)}
                            />
                            <label htmlFor={"PortHttpEndpointInstallerRadio"}>
                                Port
                            </label>
                        </div>
                    </fieldset>
                )}
                {installerType === 'TraefikHttpEndpointInstaller' && (
                    <div>
                        <label className={"inline-block w-48"}>
                            Domain:
                        </label>
                        <input value={domain} onChange={e => updateDomain(e.target.value)}/>
                    </div>
                )}
                {installerType === 'PortHttpEndpointInstaller' && (
                    <div>
                        <label className={"inline-block w-48"}>
                            Port:
                        </label>
                        <input value={port} onChange={e => updatePort(e.target.value)}/>
                    </div>
                )}
            </div>
        </>
    );
}