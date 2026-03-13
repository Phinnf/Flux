'use client';

import { format, isToday, isYesterday } from 'date-fns';
import dynamic from 'next/dynamic';
import { toast } from 'sonner';

import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Hint } from '@/components/hint';
import { Thumbnail } from '@/components/thumbnail';
import { Toolbar } from '@/components/toolbar';
import { ThreadBar } from '@/components/thread-bar';
import { Reactions } from './reactions';
import { cn } from '@/lib/utils';

import { useToggleReaction } from '../api/use-toggle-reaction';
import { useDeleteMessage } from '../api/use-delete-message';
import { useCurrentUser } from '@/features/auth/api/use-current-user';
import { useParentMessageId } from '../store/use-parent-message-id';

// Dynamic import for the renderer (Quill based)
const Renderer = dynamic(() => import('@/components/renderer'), { ssr: false });

interface MessageProps {
  id: string;
  memberId: string;
  authorImage?: string;
  authorName?: string;
  isAuthor: boolean;
  body: string;
  image?: string | null;
  createdAt: string;
  updatedAt?: string;
  isEditing: boolean;
  isCompact?: boolean;
  setEditingId: (id: string | null) => void;
  hideThreadButton?: boolean;
  threadCount?: number;
  threadImage?: string;
  threadTimestamp?: string;
  reactions: {
    emoji: string;
    count: number;
    userIds: string[];
  }[];
}

const formatFullTime = (date: Date) => {
  return `${isToday(date) ? 'Today' : isYesterday(date) ? 'Yesterday' : format(date, 'MMM d, yyyy')} at ${format(date, 'h:mm:ss a')}`;
};

export const Message = ({
  id,
  isAuthor,
  memberId,
  authorImage,
  authorName = 'Member',
  body,
  image,
  createdAt,
  updatedAt,
  isEditing,
  isCompact,
  setEditingId,
  hideThreadButton,
  threadCount,
  threadImage,
  threadTimestamp,
  reactions,
}: MessageProps) => {
  const { data: user } = useCurrentUser();
  const [parentMessageId, setParentMessageId] = useParentMessageId();
  
  const { mutate: toggleReaction, isPending: isTogglingReaction } = useToggleReaction();
  const { mutate: deleteMessage, isPending: isDeletingMessage } = useDeleteMessage();

  const handleReaction = (emoji: string) => {
    if (!user) return;
    toggleReaction({ messageId: id, emoji });
  };

  const handleDelete = () => {
    if (!user) return;
    
    const ok = confirm("Are you sure you want to delete this message?");
    if (!ok) return;

    deleteMessage({ messageId: id, userId: user.id }, {
        onSuccess: () => {
            toast.success("Message deleted");
        },
        onError: () => {
            toast.error("Failed to delete message");
        }
    });
  };

  const avatarFallback = authorName.charAt(0).toUpperCase();
  const fullImageUrl = image ? (image.startsWith('http') ? image : `${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'}${image}`) : null;

  if (isCompact) {
    return (
      <div className={cn(
        "group relative flex flex-col gap-2 p-1.5 px-5 hover:bg-gray-100/60",
        isEditing && "bg-[#f2c74433] hover:bg-[#f2c74433]"
      )}>
        <div className="flex items-start gap-2">
          <Hint label={formatFullTime(new Date(createdAt))}>
            <button className="opacity-0 group-hover:opacity-100 w-[40px] text-[10px] text-muted-foreground text-center leading-[22px]">
              {format(new Date(createdAt), "hh:mm")}
            </button>
          </Hint>

          <div className="flex flex-col w-full">
            <Renderer value={body} />
            {fullImageUrl && <Thumbnail url={fullImageUrl} />}
            {updatedAt && <span className="text-xs text-muted-foreground">(edited)</span>}
            <Reactions data={reactions} onChange={handleReaction} />
            <ThreadBar
              count={threadCount}
              image={threadImage}
              name={authorName}
              timestamp={threadTimestamp ? new Date(threadTimestamp).getTime() : undefined}
              onClick={() => setParentMessageId(id)}
            />
          </div>
        </div>

        {!isEditing && (
          <Toolbar
            isAuthor={isAuthor}
            isPending={isTogglingReaction || isDeletingMessage}
            handleEdit={() => setEditingId(id)}
            handleThread={() => setParentMessageId(id)}
            handleDelete={handleDelete}
            handleReaction={handleReaction}
            hideThreadButton={hideThreadButton}
          />
        )}
      </div>
    );
  }

  return (
    <div className={cn(
      "group relative flex flex-col gap-2 p-1.5 px-5 hover:bg-gray-100/60",
      isEditing && "bg-[#f2c74433] hover:bg-[#f2c74433]"
    )}>
      <div className="flex items-start gap-2">
        <button>
          <Avatar className="rounded-md">
            <AvatarImage src={authorImage} className="rounded-md" />
            <AvatarFallback className="rounded-md bg-sky-500 text-white text-xs">
              {avatarFallback}
            </AvatarFallback>
          </Avatar>
        </button>

        <div className="flex flex-col w-full overflow-hidden">
          <div className="text-sm">
            <button className="font-bold text-primary hover:underline">
              {authorName}
            </button>
            <span>&nbsp;&nbsp;</span>
            <Hint label={formatFullTime(new Date(createdAt))}>
              <button className="text-xs text-muted-foreground hover:underline">
                {format(new Date(createdAt), "h:mm a")}
              </button>
            </Hint>
          </div>

          <Renderer value={body} />
          {fullImageUrl && <Thumbnail url={fullImageUrl} />}
          {updatedAt && <span className="text-xs text-muted-foreground">(edited)</span>}
          <Reactions data={reactions} onChange={handleReaction} />
          <ThreadBar
            count={threadCount}
            image={threadImage}
            name={authorName}
            timestamp={threadTimestamp ? new Date(threadTimestamp).getTime() : undefined}
            onClick={() => setParentMessageId(id)}
          />
        </div>
      </div>

      {!isEditing && (
          <Toolbar
            isAuthor={isAuthor}
            isPending={isTogglingReaction || isDeletingMessage}
            handleEdit={() => setEditingId(id)}
            handleThread={() => setParentMessageId(id)}
            handleDelete={handleDelete}
            handleReaction={handleReaction}
            hideThreadButton={hideThreadButton}
          />
        )}
    </div>
  );
};
