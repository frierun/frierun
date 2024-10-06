import {createBrowserRouter, RouterProvider} from "react-router-dom";
import Root from "./pages/Root.tsx";
import Package from "./pages/Package.tsx";
import {StateContextProvider} from "./providers/StateContext.tsx";
import {QueryClient, QueryClientProvider} from "@tanstack/react-query";


const router = createBrowserRouter([
    {
        path: "/",
        element: <Root/>,
    },
    {
        path: "/packages/:name",
        Component: Package,
    }
]);

const queryClient = new QueryClient();

export default function App() {
    return (
        <QueryClientProvider client={queryClient}>
            <StateContextProvider>
                <RouterProvider router={router}/>
            </StateContextProvider>
        </QueryClientProvider>
    );
}