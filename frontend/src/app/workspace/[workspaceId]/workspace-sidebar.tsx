'use client';

import {
  AlertTriangle,
  HashIcon,
  Loader,
  MessageSquareText,
  SendHorizonal,
} from 'lucide-react';
import { usePathname } from 'next/navigation';

import { useGetChannels } from '@/features/channels/api/use-get-channels';
import { useCreateChannelModal } from '@/features/channels/store/use-create-channel-modal';
import { useGetMembers } from '@/features/members/api/use-get-members';
import { useGetWorkspaceInfo } from '@/features/workspaces/api/use-get-workspace-info';
import { useWorkspaceId } from '@/hooks/use-workspace-id';

import { SidebarItem } from './sidebar-item';
import { UserItem } from './user-item';
import { WorkspaceHeader } from './workspace-header';
import { WorkspaceSection } from './workspace-section';

export const WorkspaceSidebar = () => {
  const pathname = usePathname();
  const workspaceId = useWorkspaceId();
  const [_open, setOpen] = useCreateChannelModal();

  const { data: workspaceData, isLoading: workspaceLoading } = useGetWorkspaceInfo({ id: workspaceId });
  const { data: channelsData, isLoading: channelsLoading } = useGetChannels({ workspaceId });
  const { data: membersData, isLoading: membersLoading } = useGetMembers({ workspaceId });

  const workspace = workspaceData?.value;
  const channels = channelsData?.value || [];
  const members = membersData?.value || [];

  if (workspaceLoading || channelsLoading || membersLoading) {
    return (
      <div className="flex h-full flex-col items-center justify-center bg-[#5E2C5F]">
        <Loader className="size-5 animate-spin text-white" />
      </div>
    );
  }

  if (!workspace) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-y-2 bg-[#5E2C5F]">
        <AlertTriangle className="size-5 text-white" />
        <p className="text-sm text-white">Workspace not found.</p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col bg-[#5E2C5F]">
      <WorkspaceHeader workspace={workspace} isAdmin={workspace.isMember && workspace.role === 'admin'} />

      <div className="mt-3 flex flex-col px-2">
        <SidebarItem label="Threads" icon={MessageSquareText} id="threads" />
        <SidebarItem label="Drafts & Sent" icon={SendHorizonal} id="drafts" />
      </div>

      <WorkspaceSection
        label="Channels"
        hint="New channel"
        onNew={workspace.role === 'admin' ? () => setOpen(true) : undefined}
      >
        {channels.map((item: any) => (
          <SidebarItem
            key={item._id}
            icon={HashIcon}
            label={item.name}
            id={item._id}
            variant={pathname.includes(item._id) ? 'active' : 'default'}
          />
        ))}
      </WorkspaceSection>

      <WorkspaceSection
        label="Direct Messages"
        hint="New direct message"
        onNew={() => {}}
      >
        {members.map((item: any) => (
          <UserItem
            key={item._id}
            label={item.user.name}
            image={item.user.image}
            id={item._id}
            variant={pathname.includes(item._id) ? 'active' : 'default'}
          />
        ))}
      </WorkspaceSection>
    </div>
  );
};
