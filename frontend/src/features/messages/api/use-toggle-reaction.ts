import { useMutation, useQueryClient } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

type ToggleReactionRequest = {
  messageId: string;
  emoji: string;
};

export const useToggleReaction = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: ToggleReactionRequest) => {
      const response = await fetchApi(`/api/messages/${data.messageId}/reactions`, {
        method: 'POST',
        body: JSON.stringify({
          emoji: data.emoji,
        }),
      });
      return response;
    },
    onSuccess: () => {
        // Query invalidation is handled by SignalR listener in useChatSocket
    },
  });
};
