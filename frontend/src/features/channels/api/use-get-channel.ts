import { useQuery } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

interface UseGetChannelProps {
  workspaceId: string;
  id: string;
}

export const useGetChannel = ({ workspaceId, id }: UseGetChannelProps) => {
  const { data, isLoading } = useQuery({
    queryKey: ['channel', id],
    queryFn: async () => {
      const response = await fetchApi(`/api/workspaces/${workspaceId}/channels/${id}`);
      return response;
    },
    enabled: !!workspaceId && !!id,
  });

  return { data, isLoading };
};
