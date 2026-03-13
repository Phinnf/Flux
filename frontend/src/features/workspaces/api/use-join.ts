import { useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

export const useJoin = () => {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async ({ workspaceId, joinCode }: { workspaceId: string; joinCode: string }) => {
      return fetchApi(`/api/workspaces/${workspaceId}/join`, {
        method: 'POST',
        body: JSON.stringify({ joinCode }),
      });
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      queryClient.invalidateQueries({ queryKey: ['workspace-info', variables.workspaceId] });
    },
  });

  return {
    mutate: mutation.mutate,
    isPending: mutation.isPending,
    isSuccess: mutation.isSuccess,
    isError: mutation.isError,
    error: mutation.error,
  };
};
