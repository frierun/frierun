import {useGetDomains} from "@/api/endpoints/domains.ts";

export type Domain = {
    typeName: string;
    applicationName?: string | null;
    domain?: string | null;
}

type Props = {
    domain: Domain;
    setDomain: (domain: Domain) => void;
}

export default function DomainForm({domain, setDomain}: Props) {
    const {data} = useGetDomains();
    const subdomain = domain.domain?.split('.')[0] ?? '';
    const domainName = domain.domain?.split('.').slice(1).join('.') ?? '';
    return (
        <div>
            <label className={"inline-block w-48"}>
                Domain:
            </label>
            <input value={subdomain} onChange={e => {
                setDomain({
                    ...domain,
                    domain: `${e.target.value}.${domainName}`
                });
            }}/>

            {typeof data !== 'undefined' && (
                <select
                    value={data.data.findIndex(
                        handler => handler.typeName === domain.typeName
                            && handler.applicationName === domain.applicationName
                    )}
                    onChange={e => {
                        const index = parseInt(e.target.value);
                        setDomain({
                            ...domain,
                            typeName: data.data[index].typeName,
                            applicationName: data.data[index].applicationName,
                            domain: `${subdomain}.${data.data[index].domainName ?? ""}`
                        });
                    }}
                >
                    {data.data.map((domain, index) => (
                        <option key={index} value={index}>{domain.domainName}</option>
                    ))}
                </select>
            )}
        </div>
    );
}