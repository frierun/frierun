import Cookies from 'js-cookie'

export const customFetch = async <T>(
    url: string,
    options: RequestInit,
): Promise<T> => {
    const res = await fetch(url, {
        ...options,
        headers: {
            // eslint-disable-next-line @typescript-eslint/no-misused-spread
            ...options.headers,
            'X-XSRF-TOKEN': Cookies.get('XSRF-TOKEN') ?? '',
        }
    });
    const contentType = res.headers.get('content-type');
    
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    const data = (contentType && contentType.includes('application/json')) ? await res.json() : await res.text();

    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    return {status: res.status, data} as T;
};