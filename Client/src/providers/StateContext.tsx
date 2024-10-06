import React, {createContext} from "react";
import {useGetState} from "../api/endpoints/state.ts";

type Context = {
    ready: boolean;
    waitForReady: () => Promise<void>;
}

const StateContext = createContext<Context>({ready: true, waitForReady: async () => {}});
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
        while (true)
        {
            const {data} = await refetch();
            if (data?.data.ready) {
                return;
            }
            
            await new Promise((resolve) => setTimeout(resolve, 1000));
        }
    }
    

    return (
        <StateContext.Provider value={{ready: data.data.ready, waitForReady: waitForReady}}>
            {!data.data.ready && <p>Waiting for server to be ready...</p>}
            {children}
        </StateContext.Provider>
    );
}