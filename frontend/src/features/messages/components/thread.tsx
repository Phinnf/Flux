'use client';

import { AlertTriangle, Loader, XIcon } from 'lucide-react';
import { useRef, useState } from 'react';
import dynamic from 'next/dynamic';
import Quill from 'quill';

import { Button } from '@/components/ui/button';
import { useGetMessage } from '../api/use-get-message';
import { useGetMessages } from '../api/use-get-messages';
import { useSendMessage } from '../api/use-send-message';
import { useCurrentUser } from '@/features/auth/api/use-current-user';
import { Message } from './message';
import { useChatSocket } from '../hooks/use-chat-socket';

const Editor = dynamic(() => import('@/components/editor'), { ssr: false });

interface ThreadProps {
  parentMessageId: string;
  onClose: () => void;
  channelId: string;
}

export const Thread = ({ parentMessageId, onClose, channelId }: ThreadProps) => {
  const { data: user } = useCurrentUser();
  const { data: parentMessageData, isLoading: loadingParent } = useGetMessage(parentMessageId);
  const { data: repliesData, isLoading: loadingReplies } = useGetMessages({ channelId, parentMessageId });
  const { mutate: sendMessage } = useSendMessage();

  const parentMessage = parentMessageData?.value;
  const replies = repliesData?.value || [];

  const [editorKey, setEditorKey] = useState(0);
  const [isPending, setIsPending] = useState(false);
  const editorRef = useRef<Quill | null>(null);

  // Sync replies in real-time
  useChatSocket({ channelId, queryKey: 'messages' });

  const onSubmit = async ({ body, image }: { body: string; image: File | null }) => {
    try {
      setIsPending(true);
      if (!user) return;

      await sendMessage({
        content: body,
        channelId,
        parentMessageId,
        image,
      });

      setEditorKey((prev) => prev + 1);
    } catch (error) {
      console.error(error);
    } finally {
      setIsPending(false);
    }
  };

  const loading = loadingParent || loadingReplies;

  if (loading) {
    return (
      <div className="flex h-full flex-col">
        <div className="flex h-[49px] items-center justify-between border-b px-4">
          <p className="text-lg font-bold">Thread</p>
          <Button onClick={onClose} variant="ghost" size="iconSm">
            <XIcon className="size-5 stroke-[1.5]" />
          </Button>
        </div>
        <div className="flex h-full items-center justify-center">
          <Loader className="size-5 animate-spin text-muted-foreground" />
        </div>
      </div>
    );
  }

  if (!parentMessage) {
    return (
      <div className="flex h-full flex-col">
        <div className="flex h-[49px] items-center justify-between border-b px-4">
          <p className="text-lg font-bold">Thread</p>
          <Button onClick={onClose} variant="ghost" size="iconSm">
            <XIcon className="size-5 stroke-[1.5]" />
          </Button>
        </div>
        <div className="flex flex-col items-center justify-center h-full">
          <AlertTriangle className="size-5 text-muted-foreground" />
          <p className="text-sm text-muted-foreground">Message not found</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col">
      <div className="flex h-[49px] items-center justify-between border-b px-4">
        <p className="text-lg font-bold">Thread</p>
        <Button onClick={onClose} variant="ghost" size="iconSm">
          <XIcon className="size-5 stroke-[1.5]" />
        </Button>
      </div>

      <div className="flex flex-1 flex-col overflow-y-auto pb-4">
        <Message
          id={parentMessage.id}
          memberId={parentMessage.userId}
          authorName={parentMessage.username}
          isAuthor={parentMessage.userId === user?.id}
          body={parentMessage.content}
          image={parentMessage.imageUrl}
          createdAt={parentMessage.createdAt}
          updatedAt={parentMessage.updatedAt}
          reactions={parentMessage.reactions || []}
          isEditing={false}
          setEditingId={() => {}}
          hideThreadButton
        />

        <div className="relative my-2 text-center">
          <hr className="absolute left-0 right-0 top-[50%] border-t border-gray-300" />
          <span className="relative inline-block bg-white px-4 text-xs font-semibold text-muted-foreground">
            {replies.length} {replies.length === 1 ? 'reply' : 'replies'}
          </span>
        </div>

        {replies.map((message: any) => (
          <Message
            key={message.id}
            id={message.id}
            memberId={message.userId}
            authorName={message.username}
            isAuthor={message.userId === user?.id}
            body={message.content}
            image={message.imageUrl}
            createdAt={message.createdAt}
            updatedAt={message.updatedAt}
            reactions={message.reactions || []}
            isEditing={false}
            setEditingId={() => {}}
            hideThreadButton
          />
        ))}
      </div>

      <div className="px-4 pb-4">
        <Editor
          key={editorKey}
          onSubmit={onSubmit}
          innerRef={editorRef}
          disabled={isPending}
          placeholder="Reply..."
        />
      </div>
    </div>
  );
};
