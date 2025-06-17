import React, {createContext} from "react";
import {useGetState} from "../api/endpoints/state.ts";
import {State} from "@/api/schemas";

type Context = {
    ready: boolean;
    waitForReady: () => Promise<State>;
}

const StateContext = createContext<Context>({ready: true, waitForReady: () => Promise.resolve({ready: true} as State)});
export default StateContext;

export function StateContextProvider({children}: { children: React.ReactNode }) {
    const {data, isPending, isError, refetch} = useGetState();

    if (isPending) {
        return <p>Loading...</p>;
    }

    if (isError) {
        return <p>Error!</p>;
    }

    const waitForReady = async () => {
        while (true) {
            const {data} = await refetch();
            if (data?.data.ready) {
                return data.data;
            }

            await new Promise((resolve) => setTimeout(resolve, 1000));
        }
    }

    return (
        <StateContext.Provider value={{ready: data.data.ready, waitForReady: waitForReady}}>
            {children}
        </StateContext.Provider>
    );
}