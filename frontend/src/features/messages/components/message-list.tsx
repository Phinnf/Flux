'use client';

import { differenceInMinutes, format, isSameDay } from 'date-fns';
import { Loader } from 'lucide-react';
import { useState } from 'react';

import { useCurrentUser } from '@/features/auth/api/use-current-user';
import { API_URL } from '@/lib/api';

import { Message } from './message';

interface MessageListProps {
  memberName?: string;
  memberImage?: string;
  channelName?: string;
  channelCreationTime?: string;
  data: any[] | undefined;
  loadMore: () => void;
  isLoadingMore: boolean;
  canLoadMore: boolean;
  isLoading: boolean;
}

const TIME_THRESHOLD = 5;

export const MessageList = ({
  channelName,
  channelCreationTime,
  data,
  loadMore,
  isLoadingMore,
  canLoadMore,
  isLoading,
}: MessageListProps) => {
  const [editingId, setEditingId] = useState<string | null>(null);
  const { data: user } = useCurrentUser();

  if (isLoading) {
    return (
      <div className="flex h-full flex-1 items-center justify-center">
        <Loader className="size-5 animate-spin text-muted-foreground" />
      </div>
    );
  }

  const groupedMessages = data?.reduce((groups: any, message: any) => {
    const date = new Date(message.createdAt);
    const dateKey = format(date, 'yyyy-MM-dd');
    if (!groups[dateKey]) {
      groups[dateKey] = [];
    }
    groups[dateKey].push(message);
    return groups;
  }, {});

  return (
    <div className="flex flex-1 flex-col-reverse overflow-y-auto pb-4">
      {Object.entries(groupedMessages || {}).map(([dateKey, messages]: [string, any]) => (
        <div key={dateKey}>
          <div className="relative my-2 text-center">
            <hr className="absolute left-0 right-0 top-[50%] border-t border-gray-300" />
            <span className="relative inline-block bg-white px-4 text-xs font-semibold text-muted-foreground">
              {format(new Date(dateKey), 'EEEE, MMMM do')}
            </span>
          </div>

          {messages.map((message: any, index: number) => {
            const prevMessage = messages[index - 1];
            const isCompact =
              prevMessage &&
              prevMessage.userId === message.userId &&
              differenceInMinutes(new Date(message.createdAt), new Date(prevMessage.createdAt)) < TIME_THRESHOLD;

            return (
              <Message
                key={message.id}
                id={message.id}
                memberId={message.userId}
                authorImage={message.avatarUrl}
                authorName={message.username}
                isAuthor={message.userId === user?.id}
                body={message.content}
                image={message.imageUrl ? `${API_URL}${message.imageUrl}` : null}
                reactions={message.reactions}
                createdAt={message.createdAt}
                updatedAt={message.updatedAt}
                isEditing={editingId === message.id}
                setEditingId={setEditingId}
                isCompact={isCompact}
                threadCount={message.threadCount}
                threadImage={message.threadImage}
                threadName={message.threadName}
                threadTimestamp={message.threadLastReplyTime}
              />
            );
          })}
        </div>
      ))}

      {isLoadingMore && (
        <div className="relative my-2 text-center">
          <hr className="absolute left-0 right-0 top-[50%] border-t border-gray-300" />
          <span className="relative inline-block bg-white px-4 text-xs font-semibold text-muted-foreground">
            <Loader className="size-4 animate-spin" />
          </span>
        </div>
      )}
    </div>
  );
};
