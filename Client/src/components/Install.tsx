import useAxios from "axios-hooks";

type Props = {
    packageId: string;
}

export default function Install({packageId}: Props) {
    const [{loading}, send] = useAxios(
        {
            url: `/api/v1/packages/${packageId}` ,
            method: 'POST',
        },
        {manual: true}
    );

    const install = () => {
        send().catch(() => {
            console.log('Failed to install the package');
        });
    }

    return (
        <button
            disabled={loading}
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
            onClick={install}
        >
            Install
        </button>
    );
}