import {useEffect, useState} from "react";
import {Volume} from "@/api/schemas";
import {useGetVolumes} from "@/api/endpoints/volumes.ts";

type Props = {
    contract: Volume;
    variants: Volume[];
    updateContract: (contract: Volume, isRefetch?: boolean) => void;
}

function VariantName(contract: Volume): string {
    return contract.handler?.typeName.replace("Handler", "") ?? 'Unknown';
}

export default function VolumeForm({contract, variants, updateContract}: Props) {
    const [value, setValue] = useState('');
    const {data: getVolumesResponse} = useGetVolumes();
    const [selected, setSelected] = useState<number>(0);

    useEffect(() => {
        setValue((contract.handler?.typeName === 'LocalPathHandler' ? contract.localPath : contract.volumeName) ?? '');
    }, [contract]);

    useEffect(() => {
        setSelected(0);
    }, [variants]);

    return (
        <div className="card">
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Volume
                </label>
                {contract.name}
            </div>
            {variants.length > 1 && (
                <fieldset className="flex gap-4">
                    {variants.map((variant, idx) => (
                        <label key={idx} className="flex items-center gap-2">
                            <input
                                type="radio"
                                value={idx}
                                checked={idx === selected}
                                onChange={() => {
                                    setSelected(idx);
                                    updateContract(variant, true);
                                }}
                            />
                            {VariantName(variant)}
                        </label>
                    ))}
                </fieldset>
            )}
            {contract.handler?.typeName == 'ExistingVolumeHandler' &&
                (
                    <div>
                        <label className={"inline-block w-48"}>
                            Existing volume name:
                        </label>

                        <select
                            value={value}
                            onChange={e => {
                                setValue(e.target.value);
                                updateContract({
                                    ...contract,
                                    volumeName: e.target.value
                                });
                            }}
                        >
                            {getVolumesResponse?.data.map(volume => (
                                <option key={volume} value={volume}>{volume}</option>
                            ))}
                        </select>

                    </div>
                )
            }
            {contract.handler?.typeName == 'LocalPathHandler' &&
                (
                    <div>
                        <label className={"inline-block w-48"}>
                            Local directory:
                        </label>
                        <input
                            type="text"
                            value={value}
                            onChange={e => {
                                setValue(e.target.value);
                                updateContract({
                                    ...contract,
                                    localPath: e.target.value
                                });
                            }}
                        />
                    </div>
                )
            }
        </div>
    );
}