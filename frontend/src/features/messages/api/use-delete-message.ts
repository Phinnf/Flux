import { useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

type DeleteMessageRequest = {
  messageId: string;
  userId: string;
};

export const useDeleteMessage = () => {
  return useMutation({
    mutationFn: async (data: DeleteMessageRequest) => {
      const response = await fetchApi(`/api/messages/${data.messageId}?userId=${data.userId}`, {
        method: 'DELETE',
      });
      return response;
    },
  });
};
