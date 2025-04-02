import {Link} from "react-router-dom";
import Button from "./Button";
import {Package as PackageType} from "@/api/schemas/package";

type Props = {
    pkg: PackageType;
}

export default function Package({pkg}: Props) {
    const colors: { [tagName: string]: string } = {
        default: '#ddd',
        internet: '#a7aadd',
        network: '#89c1ff',
        storage: '#b5e3ca',
        dev: '#b3c422',
        provider: '#b3c422',
        dashboard: '#e3b5d4',
    }
    return (
        <div className={"bg-gray p-2 lg:p-3 rounded-md flex justify-between items-center"}>
            <div className={"flex gap-3"}>
                <div className={"h-12 w-12 rounded flex-shrink-0"}>
                    <img src={`/packages/${pkg.name}.png`} className={"rounded"} alt={pkg.name}/>
                </div>
                <div>
                    <div className={"text-bold text-md font-bold"}>{pkg.name}</div>
                    <div className={"my-2"}>
                        {pkg.shortDescription ?? ''}
                    </div>
                    <div className={"mt-1 flex gap-1"}>
                        {pkg.tags.map(tag => (
                            <div
                                key={tag}
                                className={`rounded-full px-3 py-0.5 text-sm font-bold`}
                                style={{backgroundColor: colors[tag] ?? colors.default}}
                            >
                                {tag}
                            </div>
                        ))}
                    </div>
                </div>
            </div>
            <div>
                <Link to={`/packages/${pkg.name}`}>
                    <Button type={"primary"}>
                        Install
                    </Button>
                </Link>
            </div>
        </div>
    )
}

