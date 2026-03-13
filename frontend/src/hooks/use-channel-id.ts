'use client';

import { useParams } from 'next/navigation';

type ChannelIdParams = {
  channelId: string;
};

export const useChannelId = () => {
  const params = useParams<ChannelIdParams>();

  return params.channelId;
};
