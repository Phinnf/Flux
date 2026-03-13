import { useQuery } from '@tanstack/react-query';
import { fetchApi, removeAuthToken } from '@/lib/api';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';

export const useCurrentUser = () => {
  const router = useRouter();
  
  const query = useQuery({
    queryKey: ['current-user'],
    queryFn: async () => {
      try {
        return await fetchApi('/api/auth/current');
      } catch (error: any) {
        // Only redirect if NOT already on the auth page
        if (typeof window !== 'undefined' && !window.location.pathname.startsWith('/auth')) {
          removeAuthToken();
          window.location.href = '/auth';
        }
        throw error;
      }
    },
    retry: false,
  });

  return query;
};
