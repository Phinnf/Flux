import { useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

interface RequestType {
  workspaceId: string;
  memberId: string;
  role: string;
}

export const useUpdateMember = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: RequestType) => {
      const response = await fetchApi(`/api/workspaces/${data.workspaceId}/members/${data.memberId}/role`, {
        method: 'PATCH',
        body: JSON.stringify({ role: data.role }),
      });
      return response;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['member', variables.workspaceId, variables.memberId] });
      queryClient.invalidateQueries({ queryKey: ['members', variables.workspaceId] });
    },
  });
};
