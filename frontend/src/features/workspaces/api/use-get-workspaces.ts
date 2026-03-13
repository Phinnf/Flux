import { useQuery } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

export const useGetWorkspaces = () => {
  const { data, isLoading, error } = useQuery({
    queryKey: ['workspaces'],
    queryFn: () => fetchApi(`/api/workspaces`),
  });

  return { data, isLoading, error };
};
