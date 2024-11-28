import {PropsWithChildren, useState} from "react";


type Props = {
}
export default function Debug({children}:PropsWithChildren<Props>)
{
    const [visible, setVisible] = useState(false);

    return <>
        <a onClick={()=> setVisible((old) => !old)}>Debug info {!visible ? '↓' : '↑'}</a>
        {visible && <div className={"card mt-2"}>{children}</div>}
    </>
}