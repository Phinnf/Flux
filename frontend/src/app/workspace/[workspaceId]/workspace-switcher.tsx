'use client';

import { Loader, Plus } from 'lucide-react';
import { useRouter } from 'next/navigation';

import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useGetWorkspaceInfo } from '@/features/workspaces/api/use-get-workspace-info';
import { useGetWorkspaces } from '@/features/workspaces/api/use-get-workspaces';
import { useCreateWorkspaceModal } from '@/features/workspaces/store/use-create-workspace-modal';
import { useWorkspaceId } from '@/hooks/use-workspace-id';
import { useMounted } from '@/hooks/use-mounted';

export const WorkspaceSwitcher = () => {
  const router = useRouter();
  const mounted = useMounted();
  const workspaceId = useWorkspaceId();
  const [_open, setOpen] = useCreateWorkspaceModal();

  const { data: workspacesData, isLoading: workspacesLoading } = useGetWorkspaces();
  const { data: workspaceData, isLoading: workspaceLoading } = useGetWorkspaceInfo({ id: workspaceId });

  const workspaces = (workspacesData?.value || []).map((w: any) => ({ ...w, id: w._id || w.id }));
  const workspace = workspaceData?.value ? { ...workspaceData.value, id: workspaceData.value._id || workspaceData.value.id } : null;

  const filteredWorkspaces = workspaces?.filter((workspace: any) => workspace?.id !== workspaceId);

  if (!mounted) return null;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button className="relative size-9 overflow-hidden bg-[#ABABAB] text-lg font-semibold text-slate-800 hover:bg-[#ABABAB]/80">
          {workspaceLoading ? <Loader className="size-5 shrink-0 animate-spin" /> : workspace?.name?.charAt(0).toUpperCase()}
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent side="bottom" align="start" className="w-64">
        <DropdownMenuItem
          onClick={() => router.push(`/workspace/${workspaceId}`)}
          className="cursor-pointer flex-col items-start justify-start capitalize"
        >
          {workspace?.name}

          <span className="text-xs text-muted-foreground">Active workspace</span>
        </DropdownMenuItem>

        {filteredWorkspaces?.map((workspace: any) => (
          <DropdownMenuItem
            key={workspace.id}
            className="cursor-pointer overflow-hidden capitalize"
            onClick={() => router.push(`/workspace/${workspace.id}`)}
          >
            <div className="relative mr-2 flex size-9 shrink-0 items-center justify-center overflow-hidden rounded-md bg-[#616061] text-xl font-semibold text-white">
              {workspace.name.charAt(0).toUpperCase()}
            </div>
            <p className="truncate">{workspace.name}</p>
          </DropdownMenuItem>
        ))}

        <DropdownMenuItem className="cursor-pointer" onClick={() => setOpen(true)}>
          <div className="relative mr-2 flex size-9 items-center justify-center overflow-hidden rounded-md bg-[#F2F2F2] text-xl font-semibold text-slate-800">
            <Plus />
          </div>
          Create a new workspace
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
