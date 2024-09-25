import Root from "./Root";
import {createBrowserRouter, RouterProvider} from "react-router-dom";


const router = createBrowserRouter([
    {
        path: "/",
        element: <Root />,
    },
]);

export default function Router()
{
    return <RouterProvider router={router} />;
}