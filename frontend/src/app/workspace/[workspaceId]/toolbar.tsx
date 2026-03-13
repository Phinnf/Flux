'use client';

import { Search, Loader } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';

import { Button } from '@/components/ui/button';
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from '@/components/ui/command';
import { useGetWorkspaceInfo } from '@/features/workspaces/api/use-get-workspace-info';
import { useWorkspaceId } from '@/hooks/use-workspace-id';
import { useProfileMemberId } from '@/features/members/store/use-profile-member-id';
import { useSearch } from '@/features/workspaces/api/use-search';
import { useDebounce } from 'react-use';

export const Toolbar = () => {
  const workspaceId = useWorkspaceId();
  const router = useRouter();
  const [_, setProfileMemberId] = useProfileMemberId();

  const { data: workspaceData } = useGetWorkspaceInfo({ id: workspaceId });
  const workspace = workspaceData?.value;

  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  useDebounce(() => setDebouncedSearch(search), 500, [search]);

  const { data: searchResults, isLoading: isSearching } = useSearch({ 
    workspaceId, 
    query: debouncedSearch 
  });

  const results = searchResults?.value || { channels: [], members: [], messages: [] };

  useEffect(() => {
    const down = (e: KeyboardEvent) => {
      if (e.key === 'k' && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        setOpen((open) => !open);
      }
    };
    document.addEventListener('keydown', down);
    return () => document.removeEventListener('keydown', down);
  }, []);

  const onChannelClick = (channelId: string) => {
    setOpen(false);
    router.push(`/workspace/${workspaceId}/channel/${channelId}`);
  };

  const onMemberClick = (memberId: string) => {
    setOpen(false);
    setProfileMemberId(memberId);
  };

  const onMessageClick = (channelId: string, messageId: string) => {
    setOpen(false);
    router.push(`/workspace/${workspaceId}/channel/${channelId}`);
    // You might want to scroll to the message in the future
  };

  return (
    <nav className="flex h-10 items-center justify-between bg-[#481349] p-1.5">
      <div className="flex-1" aria-hidden />

      <div className="min-w-[280px] max-w-[642px] shrink grow-[2]">
        <Button onClick={() => setOpen(true)} size="sm" className="h-7 w-full justify-start bg-accent/25 px-2 hover:bg-accent/25">
          <Search className="mr-2 size-4 text-white" />
          <span className="text-xs text-white">Search {workspace?.name ?? 'workspace'}...</span>

          <kbd className="pointer-events-none ml-auto inline-flex h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium text-muted-foreground opacity-90">
            <span className="text-xs">⌘</span>K
          </kbd>
        </Button>

        <CommandDialog open={open} onOpenChange={setOpen}>
          <CommandInput 
            placeholder={`Search ${workspace?.name ?? 'workspace'}...`} 
            onValueChange={setSearch}
          />
          <CommandList>
            {isSearching && (
                <div className="p-4 flex items-center justify-center">
                    <Loader className="size-4 animate-spin text-muted-foreground" />
                </div>
            )}
            <CommandEmpty>No results found.</CommandEmpty>
            
            {results.channels.length > 0 && (
                <CommandGroup heading="Channels">
                {results.channels.map((channel: any) => (
                    <CommandItem key={channel._id} onSelect={() => onChannelClick(channel._id)}>
                    {channel.name}
                    </CommandItem>
                ))}
                </CommandGroup>
            )}

            {results.members.length > 0 && (
                <>
                    <CommandSeparator />
                    <CommandGroup heading="Members">
                    {results.members.map((member: any) => (
                        <CommandItem key={member._id} onSelect={() => onMemberClick(member._id)}>
                        {member.name}
                        </CommandItem>
                    ))}
                    </CommandGroup>
                </>
            )}

            {results.messages.length > 0 && (
                <>
                    <CommandSeparator />
                    <CommandGroup heading="Messages">
                    {results.messages.map((message: any) => (
                        <CommandItem 
                            key={message._id} 
                            onSelect={() => onMessageClick(message.channelId, message._id)}
                        >
                            <div className="flex flex-col">
                                <span className="text-xs text-muted-foreground">{message.username}</span>
                                <span className="line-clamp-1">{message.content}</span>
                            </div>
                        </CommandItem>
                    ))}
                    </CommandGroup>
                </>
            )}
          </CommandList>
        </CommandDialog>
      </div>

      <div className="ml-auto flex flex-1 items-center justify-end">
      </div>
    </nav>
  );
};
