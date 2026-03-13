import { useQuery } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

interface UseGetMessagesProps {
  channelId: string;
  parentMessageId?: string;
}

export const useGetMessages = ({ channelId, parentMessageId }: UseGetMessagesProps) => {
  const { data, isLoading } = useQuery({
    queryKey: ['messages', channelId, parentMessageId],
    queryFn: async () => {
      let url = `/api/channels/${channelId}/messages`;
      if (parentMessageId) {
        url += `?parentMessageId=${parentMessageId}`;
      }
      const response = await fetchApi(url);
      return response; // Expecting { messages: [], nextCursor: ... }
    },
    enabled: !!channelId,
  });

  return { data, isLoading };
};
