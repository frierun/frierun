import {useState} from "react";
import {ContractList} from "@/types.ts";

type Props = {
    contracts: ContractList;
}

export default function Debug({contracts}: Props) {
    const [visible, setVisible] = useState(false);

    return <>
        <a onClick={() => {
            setVisible((old) => !old);
        }}>Debug info {!visible ? '↓' : '↑'}</a>
        {visible && (
            <div className={"card mt-2"}>
                {Object.entries(contracts).map(pair => (
                    <div key={pair[0]}>
                        <p>{pair[0]}</p>
                        <pre>{JSON.stringify(pair[1], null, 2)}</pre>
                    </div>
                ))}
            </div>
        )}
    </>
}