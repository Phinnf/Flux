'use client';

import { Loader } from 'lucide-react';
import type { PropsWithChildren } from 'react';
import { useParams } from 'next/navigation';

import { ResizableHandle, ResizablePanel, ResizablePanelGroup } from '@/components/ui/resizable';
import { Sidebar } from './sidebar';
import { Toolbar } from './toolbar';
import { WorkspaceSidebar } from './workspace-sidebar';

import { useParentMessageId } from '@/features/messages/store/use-parent-message-id';
import { useProfileMemberId } from '@/features/members/store/use-profile-member-id';
import { Thread } from '@/features/messages/components/thread';
import { Profile } from '@/features/members/components/profile';
import { useMounted } from '@/hooks/use-mounted';

const WorkspaceIdLayout = ({ children }: Readonly<PropsWithChildren>) => {
  const params = useParams();
  const mounted = useMounted();
  const [parentMessageId, setParentMessageId] = useParentMessageId();
  const [profileMemberId, setProfileMemberId] = useProfileMemberId();

  const channelId = params.channelId as string;
  const showPanel = !!parentMessageId || !!profileMemberId;

  if (!mounted) return null;

  return (
    <div className="h-full">
      <Toolbar />

      <div className="flex h-[calc(100vh_-_40px)]">
        <Sidebar />

        <ResizablePanelGroup direction="horizontal" autoSaveId="flux-workspace-layout">
          <ResizablePanel defaultSize={20} minSize={11} className="bg-[#5E2C5F]">
            <WorkspaceSidebar />
          </ResizablePanel>

          <ResizableHandle withHandle />

          <ResizablePanel defaultSize={80} minSize={20}>
            <ResizablePanelGroup direction="horizontal">
              <ResizablePanel defaultSize={showPanel ? 70 : 100} minSize={20}>
                {children}
              </ResizablePanel>

              {showPanel && (
                <>
                  <ResizableHandle withHandle />
                  <ResizablePanel defaultSize={30} minSize={25}>
                    {parentMessageId ? (
                        <Thread
                            parentMessageId={parentMessageId as string}
                            onClose={() => setParentMessageId(null)}
                            channelId={channelId}
                        />
                    ) : profileMemberId ? (
                        <Profile 
                            memberId={profileMemberId as string}
                            onClose={() => setProfileMemberId(null)}
                        />
                    ) : (
                        <div className="flex h-full items-center justify-center">
                            <Loader className="size-5 animate-spin text-muted-foreground" />
                        </div>
                    )}
                  </ResizablePanel>
                </>
              )}
            </ResizablePanelGroup>
          </ResizablePanel>
        </ResizablePanelGroup>
      </div>
    </div>
  );
};

export default WorkspaceIdLayout;
