'use client';

import { MdAddReaction } from 'react-icons/md';
import { useCurrentUser } from '@/features/auth/api/use-current-user';
import { Hint } from '@/components/hint';
import { EmojiPopover } from '@/components/emoji-popover';
import { cn } from '@/lib/utils';

interface ReactionsProps {
  data: {
    emoji: string;
    count: number;
    userIds: string[];
  }[];
  onChange: (emoji: string) => void;
}

export const Reactions = ({ data, onChange }: ReactionsProps) => {
  const { data: user } = useCurrentUser();

  if (data.length === 0) return null;

  return (
    <div className="flex items-center gap-1 mt-1 mb-1">
      {data.map((reaction) => {
        const isReacted = reaction.userIds.includes(user?.id || '');

        return (
          <Hint 
            key={reaction.emoji} 
            label={`${reaction.count} members reacted with ${reaction.emoji}`}
          >
            <button
              onClick={() => onChange(reaction.emoji)}
              className={cn(
                "h-6 px-2 rounded-full bg-slate-200/70 border border-transparent text-slate-800 flex items-center gap-x-1",
                isReacted && "bg-blue-100/70 border-blue-500 text-blue-700"
              )}
            >
              <span className="text-sm">{reaction.emoji}</span>
              <span className={cn(
                "text-xs font-semibold text-muted-foreground",
                isReacted && "text-blue-500"
              )}>
                {reaction.count}
              </span>
            </button>
          </Hint>
        );
      })}
      <EmojiPopover
        hint="Add reaction"
        onEmojiSelect={(emoji) => onChange(emoji)}
      >
        <button className="h-6 px-3 rounded-full bg-slate-200/70 border border-transparent text-slate-800 hover:border-slate-500 flex items-center gap-x-1">
          <MdAddReaction className="size-4" />
        </button>
      </EmojiPopover>
    </div>
  );
};
