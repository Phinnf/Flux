import { useMutation, useQueryClient } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

type SendMessageRequest = {
  content: string;
  channelId: string;
  parentMessageId?: string;
  image?: File | null;
};

export const useSendMessage = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: SendMessageRequest) => {
      const formData = new FormData();
      formData.append('content', data.content);
      formData.append('channelId', data.channelId);
      
      if (data.parentMessageId) {
        formData.append('parentMessageId', data.parentMessageId);
      }
      
      if (data.image) {
        formData.append('image', data.image);
      }

      const response = await fetchApi('/api/messages', {
        method: 'POST',
        body: formData,
      });
      return response;
    },
    onSuccess: (_, variables) => {
      // Invalidate messages for this channel
      queryClient.invalidateQueries({ queryKey: ['messages', variables.channelId] });
    },
  });
};
