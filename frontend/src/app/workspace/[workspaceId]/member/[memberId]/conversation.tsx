'use client';

import { Loader } from 'lucide-react';
import { MessageList } from '@/features/messages/components/message-list';
import { useGetMessages } from '@/features/messages/api/use-get-messages';
import { ChatInput } from '../../channel/[channelId]/chat-input'; // Reuse chat input
import { Header } from './header'; // We will create this
import { useGetMember } from '@/features/members/api/use-get-member';
import { useWorkspaceId } from '@/hooks/use-workspace-id';

interface ConversationProps {
  memberId: string;
  channelId: string;
}

export const Conversation = ({ memberId, channelId }: ConversationProps) => {
  const workspaceId = useWorkspaceId();
  const { data: memberData, isLoading: memberLoading } = useGetMember({ workspaceId, memberId });
  const { data: messagesData, isLoading: messagesLoading } = useGetMessages({ channelId });

  const member = memberData?.value;
  const messages = messagesData?.value || [];

  if (memberLoading || messagesLoading) {
    return (
      <div className="h-full flex items-center justify-center">
        <Loader className="size-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      <Header 
        memberName={member?.user.name} 
        memberImage={member?.user.image} 
      />
      <MessageList
        data={messages}
        variant="conversation"
        memberName={member?.user.name}
        memberImage={member?.user.image}
      />
      <ChatInput 
        placeholder={`Message ${member?.user.name}`} 
        channelId={channelId}
      />
    </div>
  );
};
