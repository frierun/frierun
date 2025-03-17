import {useEffect, useState} from "react";
import {Volume} from "@/api/schemas";
import {useGetVolumes} from "@/api/endpoints/volumes.ts";

type Props = {
    contract: Volume;
    updateContract: (contract: Volume) => void;
}

export default function VolumeForm({contract, updateContract}: Props) {
    const [value, setValue] = useState('');
    const [installer, setInstaller] = useState<'NewVolume' | 'ExistingVolume' | 'LocalPath'>('NewVolume');
    const {data: getVolumesResponse} = useGetVolumes();

    useEffect(() => {
        switch (installer)
        {
            case 'ExistingVolume':
                setValue(getVolumesResponse?.data[0].name ?? '');
                break;
            case 'LocalPath':
                setValue(contract.path ?? '/data');
                break;
        }
    }, [contract, installer]);

    useEffect(() => {
        switch (installer) {
            case 'NewVolume':
                updateContract({
                    ...contract,
                    volumeName: null,
                    path: null,
                });
                break;
            case 'ExistingVolume':
                updateContract({
                    ...contract,
                    volumeName: value,
                    path: null,
                });
                break;
            case 'LocalPath':
                updateContract({
                    ...contract,
                    path: value,
                    volumeName: null,
                });
                break;
        }
    }, [value, installer]);

    return (
        <>
            <div>
                <div className={"my-1.5"}>
                    <label className={"inline-block w-48"}>
                        Volume
                    </label>
                    {contract.name}
                </div>
                <fieldset className={"flex gap-4"}>
                    <div>
                        <input
                            id={"NewVolume" + value}
                            type="radio"
                            checked={installer === 'NewVolume'}
                            onChange={() => {
                                setInstaller('NewVolume');
                            }}
                        />
                        <label htmlFor={"NewVolume" + value}>
                            New volume
                        </label>
                    </div>
                    {!!getVolumesResponse?.data?.length && (
                        <div>
                            <input
                                id={"ExistingVolume" + value}
                                type="radio"
                                checked={installer === 'ExistingVolume'}
                                onChange={() => {
                                    setInstaller('ExistingVolume');
                                }}
                            />
                            <label htmlFor={"ExistingVolume" + value}>
                                Existing volume
                            </label>
                        </div>
                    )}
                    <div>
                        <input
                            id={"LocalPath" + value}
                            type="radio"
                            checked={installer === 'LocalPath'}
                            onChange={() => {
                                setInstaller('LocalPath');
                            }}
                        />
                        <label htmlFor={"LocalPath" + value}>
                            Local directory
                        </label>
                    </div>
                </fieldset>
                {installer == 'ExistingVolume' &&
                    (
                        <div>
                            <label className={"inline-block w-48"}>
                                Existing volume name:
                            </label>

                            <select
                                value={value}
                                onChange={e => setValue(e.target.value)}
                            >
                                {getVolumesResponse?.data.map(volume => (
                                    <option key={volume.name} value={volume.name}>{volume.name}</option>
                                ))}
                            </select>

                        </div>
                    )
                }
                {installer == 'LocalPath' &&
                    (
                        <div>
                            <label className={"inline-block w-48"}>
                                Local directory:
                            </label>
                            <input
                                type="text"
                                value={value}
                                onChange={e => setValue(e.target.value)}
                            />
                        </div>
                    )
                }
            </div>
        </>
    );
}