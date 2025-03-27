import {useGetDomains} from "@/api/endpoints/domains.ts";

export type Domain = {
    typeName: string;
    applicationName?: string | null;
    subdomain?: string | null;
}

type Props = {
    domain: Domain;
    setDomain: (domain: Domain) => void;
}

export default function DomainForm({domain, setDomain}: Props) {
    const {data} = useGetDomains();
    return (
        <div>
            <label className={"inline-block w-48"}>
                Domain:
            </label>
            <input value={domain.subdomain ?? ''} onChange={e => {
                setDomain({
                    ...domain,
                    subdomain: e.target.value
                });
            }}/>

            {typeof data !== 'undefined' && (
                <select
                    value={data.data.findIndex(
                        installer => installer.typeName === domain.typeName
                            && installer.applicationName === domain.applicationName
                    )}
                    onChange={e => {
                        const index = parseInt(e.target.value);
                        setDomain({
                            ...domain,
                            typeName: data.data[index].typeName,
                            applicationName: data.data[index].applicationName
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