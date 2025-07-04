﻿import {useEffect, useState} from "react";
import {Volume} from "@/api/schemas";
import {useGetVolumes} from "@/api/endpoints/volumes.ts";

type Props = {
    contract: Volume;
    updateContract: (contract: Volume) => void;
}

export default function VolumeForm({contract, updateContract}: Props) {
    const [value, setValue] = useState('');
    const [installer, setinstaller] = useState<'NewVolume' | 'ExistingVolume' | 'LocalPath'>('NewVolume');
    const {data: getVolumesResponse} = useGetVolumes();

    useEffect(() => {
        switch (installer)
        {
            case 'ExistingVolume':
                setValue(getVolumesResponse?.data[0] ?? '');
                break;
            case 'LocalPath':
                setValue(contract.localPath ?? '/data');
                break;
        }
    }, [contract, getVolumesResponse, installer]);

    useEffect(() => {
        switch (installer) {
            case 'NewVolume':
                updateContract({
                    ...contract,
                    volumeName: null,
                    localPath: null,
                    handler: undefined
                });
                break;
            case 'ExistingVolume':
                updateContract({
                    ...contract,
                    volumeName: value,
                    localPath: null,
                    handler: undefined
                });
                break;
            case 'LocalPath':
                updateContract({
                    ...contract,
                    localPath: value,
                    volumeName: null,
                    handler: undefined
                });
                break;
        }
    }, [value, installer, updateContract, contract]);

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
                                setinstaller('NewVolume');
                            }}
                        />
                        <label htmlFor={"NewVolume" + value}>
                            New volume
                        </label>
                    </div>
                    {!!getVolumesResponse?.data.length && (
                        <div>
                            <input
                                id={"ExistingVolume" + value}
                                type="radio"
                                checked={installer === 'ExistingVolume'}
                                onChange={() => {
                                    setinstaller('ExistingVolume');
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
                                setinstaller('LocalPath');
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
                                onChange={e => { setValue(e.target.value); }}
                            >
                                {getVolumesResponse?.data.map(volume => (
                                    <option key={volume} value={volume}>{volume}</option>
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
                                onChange={e => { setValue(e.target.value); }}
                            />
                        </div>
                    )
                }
            </div>
        </>
    );
}