import {PropsWithChildren, useState} from "react";

export default function Debug({children}:PropsWithChildren)
{
    const [visible, setVisible] = useState(false);

    return <>
        <a onClick={()=> { setVisible((old) => !old); }}>Debug info {!visible ? '↓' : '↑'}</a>
        {visible && <div className={"card mt-2"}>{children}</div>}
    </>
}