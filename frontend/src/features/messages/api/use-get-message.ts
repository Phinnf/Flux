import { useQuery } from '@tanstack/react-query';

import { fetchApi } from '@/lib/api';

export const useGetMessage = (messageId: string) => {
  const { data, isLoading } = useQuery({
    queryKey: ['message', messageId],
    queryFn: async () => {
      const response = await fetchApi(`/api/messages/${messageId}`);
      return response;
    },
    enabled: !!messageId,
  });

  return { data, isLoading };
};
