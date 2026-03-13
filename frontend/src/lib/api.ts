import { User } from "lucide-react"; // Just an import placeholder

export const API_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7274';

export const getAuthToken = () => {
    if (typeof window !== 'undefined') {
        // Read from cookie (document.cookie)
        const match = document.cookie.match(new RegExp('(^| )token=([^;]+)'));
        if (match) return match[2];
    }
    return null;
};

export const setAuthToken = (token: string) => {
    if (typeof window !== 'undefined') {
        document.cookie = `token=${token}; path=/; max-age=${60 * 60 * 24 * 7}; samesite=lax`;
    }
};

export const removeAuthToken = () => {
    if (typeof window !== 'undefined') {
        document.cookie = `token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`;
    }
};

export const fetchApi = async (endpoint: string, options: RequestInit = {}) => {
    const token = getAuthToken();
    
    const headers = new Headers(options.headers);
    if (token) {
        headers.set('Authorization', `Bearer ${token}`);
    }
    if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
        headers.set('Content-Type', 'application/json');
    }

    const response = await fetch(`${API_URL}${endpoint}`, {
        ...options,
        headers,
    });

    if (!response.ok) {
        let errorData;
        try {
            errorData = await response.json();
        } catch {
            errorData = { message: 'Something went wrong' };
        }
        throw new Error(errorData.Error || errorData.message || 'API Error');
    }

    // Sometimes APIs return 204 No Content
    if (response.status === 204) return null;

    try {
        const data = await response.json();
        // If data doesn't have a 'value' property, wrap it to match frontend expectations
        if (data !== null && typeof data === 'object' && !('value' in data)) {
            return { value: data };
        }
        return data;
    } catch {
        return null;
    }
};
