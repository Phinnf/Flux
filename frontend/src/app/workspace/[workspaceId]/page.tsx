'use client';

import { Loader, TriangleAlert } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useEffect, useMemo } from 'react';

import { useGetChannels } from '@/features/channels/api/use-get-channels';
import { useGetWorkspaceInfo } from '@/features/workspaces/api/use-get-workspace-info';
import { useWorkspaceId } from '@/hooks/use-workspace-id';

const WorkspaceIdPage = () => {
  const router = useRouter();
  const workspaceId = useWorkspaceId();

  const { data: workspaceData, isLoading: workspaceLoading } = useGetWorkspaceInfo({ id: workspaceId });
  const { data: channelsData, isLoading: channelsLoading } = useGetChannels({ workspaceId });

  const workspace = workspaceData?.value;
  const channels = useMemo(() => channelsData?.value || [], [channelsData]);
  const channelId = useMemo(() => channels[0]?._id, [channels]);

  useEffect(() => {
    if (workspaceLoading || channelsLoading || !workspace) return;

    if (channelId) {
      router.replace(`/workspace/${workspaceId}/channel/${channelId}`);
    }
    // Logic automatically opening create channel modal is removed as requested.
  }, [channelId, workspaceLoading, channelsLoading, workspace, router, workspaceId]);

  if (workspaceLoading || channelsLoading) {
    return (
      <div className="flex h-full flex-1 flex-col items-center justify-center gap-2 bg-[#5E2C5F]/95 text-white">
        <Loader className="size-5 animate-spin" />
      </div>
    );
  }

  if (!workspaceId || !workspace) {
    return (
      <div className="flex h-full flex-1 flex-col items-center justify-center gap-2 bg-[#5E2C5F]/95 text-white">
        <TriangleAlert className="size-5" />
        <span className="text-sm">Workspace not found.</span>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-1 flex-col items-center justify-center gap-2 bg-[#5E2C5F]/95 text-white">
      <TriangleAlert className="size-5" />
      <span className="text-sm">No Channel(s) found.</span>
    </div>
  );
};

export default WorkspaceIdPage;
