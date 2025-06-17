import {useState} from "react";
import {Contract} from "@/components/contracts/ContractForm.tsx";

type Props = {
    contracts: Contract[];
}

export default function Debug({contracts}: Props) {
    const [visible, setVisible] = useState(false);

    return <>
        <a onClick={() => {
            setVisible((old) => !old);
        }}>Debug info {!visible ? '↓' : '↑'}</a>
        {visible && (
            <div className={"card mt-2"}>
                {contracts.map(contract => (
                    <div key={`${contract.type}:${contract.name}`}>
                        <p>{contract.type}</p>
                        <pre>{JSON.stringify(contract, null, 2)}</pre>
                    </div>
                ))}
            </div>
        )}
    </>
}