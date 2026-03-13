import { useQuery } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

interface UseGetChannelsProps {
  workspaceId: string;
}

export const useGetChannels = ({ workspaceId }: UseGetChannelsProps) => {
  const { data, isLoading } = useQuery({
    queryKey: ['channels', workspaceId],
    queryFn: async () => {
      const response = await fetchApi(`/api/workspaces/${workspaceId}/channels`);
      return response;
    },
    enabled: !!workspaceId,
  });

  return { data, isLoading };
};
