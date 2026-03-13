import { useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

interface RequestType {
  workspaceId: string;
  memberId: string;
}

export const useRemoveMember = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: RequestType) => {
      const response = await fetchApi(`/api/workspaces/${data.workspaceId}/members/${data.memberId}`, {
        method: 'DELETE',
      });
      return response;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['members', variables.workspaceId] });
    },
  });
};
