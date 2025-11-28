import {Contract} from "@/types.ts";
import {useState} from "react";

type Props = {
    contract: Contract;
}

export default function InstalledContract({contract}: Props) {
    const [opened, setOpened] = useState(false);

    return (
        <div className={"bg-gray p-2 lg:p-3 rounded-md"}>
            <div>{contract.type}</div>
            <div>{contract.id}</div>
            {opened ? (
                <>
                    <div onClick={() => {
                        setOpened(false)
                    }}>
                        Close
                    </div>
                    <pre>
                        {JSON.stringify(contract, null, 2)}
                    </pre>
                </>
            ) : (
                <>
                    <div onClick={() => {
                        setOpened(true)
                    }}>
                        Information ↓
                    </div>
                </>
            )}
        </div>
    );
}