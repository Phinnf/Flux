import { useMutation, useQueryClient } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

type CreateChannelRequest = {
  name: string;
  description?: string;
  type?: number; // 0 = Public, 1 = Private, 2 = Direct
  workspaceId: string;
};

export const useCreateChannel = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateChannelRequest) => {
      // Endpoint expects: api/workspaces/{workspaceId}/channels
      const { workspaceId, ...payload } = data;
      const response = await fetchApi(`/api/workspaces/${workspaceId}/channels`, {
        method: 'POST',
        body: JSON.stringify({ ...payload, type: payload.type || 0 }),
      });
      return response;
    },
    onSuccess: (_, variables) => {
      // Invalidate both workspaces and channels lists
      queryClient.invalidateQueries({ queryKey: ['channels', variables.workspaceId] });
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
    },
  });
};
