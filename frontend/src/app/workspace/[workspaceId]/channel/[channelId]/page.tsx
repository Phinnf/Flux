'use client';

import { Loader, TriangleAlert } from 'lucide-react';

import { useGetChannel } from '@/features/channels/api/use-get-channel';
import { useGetMessages } from '@/features/messages/api/use-get-messages';
import { MessageList } from '@/features/messages/components/message-list';
import { useChatSocket } from '@/features/messages/hooks/use-chat-socket';
import { useChannelId } from '@/hooks/use-channel-id';
import { useWorkspaceId } from '@/hooks/use-workspace-id';

import { ChannelHeader } from './header';
import { ChatInput } from './chat-input';

const ChannelIdPage = () => {
  const channelId = useChannelId();
  const workspaceId = useWorkspaceId();

  useChatSocket({ channelId });

  const { data: channelData, isLoading: channelLoading } = useGetChannel({ workspaceId, id: channelId });
  const { data: messagesData, isLoading: messagesLoading } = useGetMessages({ channelId });

  const channel = channelData?.value ? { ...channelData.value, id: channelData.value._id || channelData.value.id } : null;
  const messages = messagesData?.value;

  if (channelLoading || messagesLoading) {
    return (
      <div className="flex h-full flex-1 items-center justify-center">
        <Loader className="size-5 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!channel) {
    return (
      <div className="flex h-full flex-1 flex-col items-center justify-center gap-y-2">
        <TriangleAlert className="size-5 text-muted-foreground" />
        <p className="text-sm text-muted-foreground">Channel not found.</p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col">
      <ChannelHeader title={channel.name} />

      <MessageList
        channelName={channel.name}
        channelCreationTime={channel.createdAt}
        data={messages}
        loadMore={() => {}}
        isLoadingMore={false}
        canLoadMore={false}
        isLoading={messagesLoading}
      />

      <ChatInput placeholder={`Message # ${channel.name}`} />
    </div>
  );
};

export default ChannelIdPage;
