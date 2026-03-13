import { useMutation } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

interface RequestType {
  workspaceId: string;
  targetUserId: string;
}

export const useGetOrCreateDirectChannel = () => {
  return useMutation({
    mutationFn: async (data: RequestType) => {
      // Note: Backend endpoint uses POST api/workspaces/{workspaceId}/channels/direct
      // and expects { targetUserId, currentUserId } 
      // But currentUserId is already in JWT token claims in my backend logic.
      // Let's check the controller again to be sure.
      const response = await fetchApi(`/api/workspaces/${data.workspaceId}/channels/direct`, {
        method: 'POST',
        body: JSON.stringify({ targetUserId: data.targetUserId }),
      });
      return response;
    },
  });
};
