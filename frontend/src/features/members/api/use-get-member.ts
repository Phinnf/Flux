import { useQuery } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

interface UseGetMemberProps {
  workspaceId: string;
  memberId: string;
}

export const useGetMember = ({ workspaceId, memberId }: UseGetMemberProps) => {
  const { data, isLoading } = useQuery({
    queryKey: ['member', workspaceId, memberId],
    queryFn: async () => {
      const response = await fetchApi(`/api/workspaces/${workspaceId}/members/${memberId}`);
      return response;
    },
    enabled: !!workspaceId && !!memberId,
  });

  return { data, isLoading };
};
