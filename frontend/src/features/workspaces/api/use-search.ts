import { useQuery } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

interface UseSearchProps {
  workspaceId: string;
  query: string;
}

export const useSearch = ({ workspaceId, query }: UseSearchProps) => {
  const { data, isLoading } = useQuery({
    queryKey: ['search', workspaceId, query],
    queryFn: async () => {
      const response = await fetchApi(`/api/workspaces/${workspaceId}/search?q=${encodeURIComponent(query)}`);
      return response;
    },
    enabled: !!workspaceId && query.length > 1,
  });

  return { data, isLoading };
};
