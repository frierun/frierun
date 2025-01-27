import Cookies from 'js-cookie'

export const customFetch = async <T>(
    url: string,
    options: RequestInit,
): Promise<T> => {
    const res = await fetch(url, {
        ...options,
        headers: {
            ...options.headers,
            'X-XSRF-TOKEN': Cookies.get('XSRF-TOKEN') ?? '',
        }
    });
    console.log(Cookies.get('XSRF-TOKEN'));
    const contentType = res.headers.get('content-type');
    
    const data = (contentType && contentType.includes('application/json')) ? await res.json() : await res.text();
    
    return {status: res.status, data} as T;
};