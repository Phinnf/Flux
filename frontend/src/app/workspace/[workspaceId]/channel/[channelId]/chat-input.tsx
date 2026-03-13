'use client';

import dynamic from 'next/dynamic';
import { useRef, useState } from 'react';
import type Quill from 'quill';
import { toast } from 'sonner';

import { useSendMessage } from '@/features/messages/api/use-send-message';
import { useChannelId } from '@/hooks/use-channel-id';
import { useWorkspaceId } from '@/hooks/use-workspace-id';

// Dynamic import to avoid SSR issues with Quill
const Editor = dynamic(() => import('@/components/editor'), { ssr: false });

interface ChatInputProps {
  placeholder: string;
}

export const ChatInput = ({ placeholder }: ChatInputProps) => {
  const [editorKey, setEditorKey] = useState(0);
  const [isPending, setIsPending] = useState(false);

  const channelId = useChannelId();
  const workspaceId = useWorkspaceId();

  const editorRef = useRef<Quill | null>(null);

  const { mutate: sendMessage } = useSendMessage();

  const handleSubmit = async ({ body, image }: { body: string; image: File | null }) => {
    try {
      setIsPending(true);
      editorRef.current?.enable(false);

      sendMessage(
        {
          channelId,
          content: body,
          image,
        },
        {
          onSuccess: () => {
            toast.success('Message sent!');
            setEditorKey((prev) => prev + 1);
          },
          onError: () => {
            toast.error('Failed to send message.');
          },
          onSettled: () => {
            setIsPending(false);
            editorRef.current?.enable(true);
          },
        },
      );
    } catch (error) {
      toast.error('Something went wrong!');
    } finally {
      setIsPending(false);
      editorRef.current?.enable(true);
    }
  };

  return (
    <div className="w-full px-5 pb-5">
      <Editor
        key={editorKey}
        placeholder={placeholder}
        onSubmit={handleSubmit}
        disabled={isPending}
        innerRef={editorRef}
      />
    </div>
  );
};
