import useAxios from "axios-hooks";

type Props = {
    applicationId: string;
}

export default function Uninstall({applicationId}: Props) {
    const [{loading}, send] = useAxios(
        {
            url: `/api/v1/applications/${applicationId}` ,
            method: 'DELETE',
        },
        {manual: true}
    );

    const uninstall = () => {
        send().catch(() => {
            console.log('Failed to uninstall the application');
        });
    }

    return (
        <button
            disabled={loading}
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
            onClick={uninstall}
        >
            Uninstall
        </button>
    );
}