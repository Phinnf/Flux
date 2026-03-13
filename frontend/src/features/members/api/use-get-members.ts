import { useQuery } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

interface UseGetMembersProps {
  workspaceId: string;
}

export const useGetMembers = ({ workspaceId }: UseGetMembersProps) => {
  const { data, isLoading } = useQuery({
    queryKey: ['members', workspaceId],
    queryFn: async () => {
      const response = await fetchApi(`/api/workspaces/${workspaceId}/members`);
      return response;
    },
    enabled: !!workspaceId,
  });

  return { data, isLoading };
};
