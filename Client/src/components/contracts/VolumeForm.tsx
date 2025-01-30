import {useEffect, useState} from "react";
import {Volume} from "@/api/schemas";
import {useGetVolumes} from "@/api/endpoints/volumes.ts";

type Props = {
    contract: Volume;
    updateContract: (contract: Volume) => void;
}

export default function VolumeForm({contract, updateContract}: Props) {
    const [volumeName, setVolumeName] = useState('');
    const [createNew, setCreateNew] = useState(true);
    const {data: getVolumesResponse} = useGetVolumes();

    useEffect(() => {
        setVolumeName(contract.volumeName ?? '');
    }, [contract]);

    const updateVolumeName = (volumeName: string) => {
        setVolumeName(volumeName);
        updateContract({
            ...contract,
            volumeName
        });
    }

    return (
        <>
            <div>
                <div className={"my-1.5"}>
                    <label className={"inline-block w-48"}>
                        Volume
                    </label>
                    {contract.name}
                </div>
                {!!getVolumesResponse?.data?.length && (
                    <div>
                        <label>
                            <input
                                type="radio"
                                checked={createNew}
                                onChange={() => {
                                    setCreateNew(true);
                                    updateVolumeName(contract.volumeName ?? '');
                                }}
                                className="mr-2"
                            />
                            New
                        </label>
                        <label>
                            <input
                                type="radio"
                                checked={!createNew}
                                onChange={() => {
                                    setCreateNew(false);
                                    updateVolumeName(getVolumesResponse?.data[0].name ?? '');
                                }}
                                className="mr-2"
                            />
                            Existing
                        </label>
                    </div>
                )}
                {createNew
                    ? (
                        <div>
                            <label className={"inline-block w-48"}>
                                New volume name:
                            </label>
                            <input
                                value={volumeName}
                                onChange={e => updateVolumeName(e.target.value)}
                            />
                        </div>
                    ) : (
                        <div>
                            <label className={"inline-block w-48"}>
                                Existing volume name:
                            </label>
                            <select
                                value={volumeName}
                                onChange={e => updateVolumeName(e.target.value)}
                            >
                                {getVolumesResponse?.data.map(volume => (
                                    <option key={volume.name} value={volume.name}>{volume.name}</option>
                                ))}
                            </select>
                        </div>
                    )
                }
            </div>
        </>
    );
}