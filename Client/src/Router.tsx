import {createBrowserRouter, RouterProvider} from "react-router-dom";
import Root from "./pages/Root.tsx";
import Package from "./pages/Package.tsx";


const router = createBrowserRouter([
    {
        path: "/",
        element: <Root />,
    },
    {
        path: "/packages/:name",
        Component: Package,
    }
]);

export default function Router()
{
    return <RouterProvider router={router} />;
}