'use client';

import { X, Mail, Loader, AlertTriangle, ChevronDown } from 'lucide-react';
import { useGetMember } from '../api/use-get-member';
import { useUpdateMember } from '../api/use-update-member';
import { useRemoveMember } from '../api/use-remove-member';
import { useGetMembers } from '../api/use-get-members';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { useWorkspaceId } from '@/hooks/use-workspace-id';
import { useCurrentUser } from '@/features/auth/api/use-current-user';
import { toast } from 'sonner';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

interface ProfileProps {
  memberId: string;
  onClose: () => void;
}

export const Profile = ({ memberId, onClose }: ProfileProps) => {
  const workspaceId = useWorkspaceId();
  const { data: userData } = useCurrentUser();
  const currentUser = userData?._id;

  const { data: memberData, isLoading } = useGetMember({ workspaceId, memberId });
  const { data: membersData } = useGetMembers({ workspaceId });
  
  const { mutate: updateMember, isPending: isUpdating } = useUpdateMember();
  const { mutate: removeMember, isPending: isRemoving } = useRemoveMember();

  const member = memberData?.value;
  const members = membersData?.value || [];
  const currentMember = members.find((m: any) => m._id === currentUser);
  const isAdmin = currentMember?.role === 'admin';

  const onUpdateRole = (role: string) => {
    updateMember({ workspaceId, memberId, role }, {
        onSuccess: () => toast.success("Role updated"),
        onError: () => toast.error("Failed to update role")
    });
  };

  const onRemove = () => {
    const ok = confirm("Are you sure you want to remove this member?");
    if (!ok) return;

    removeMember({ workspaceId, memberId }, {
        onSuccess: () => {
            toast.success("Member removed");
            onClose();
        },
        onError: () => toast.error("Failed to remove member")
    });
  };

  if (isLoading) {
    return (
      <div className="h-full flex flex-col">
        <div className="flex justify-between items-center px-4 h-[49px] border-b">
            <p className="text-lg font-bold">Profile</p>
            <Button onClick={onClose} variant="ghost" size="iconSm">
                <X className="size-5 stroke-[1.5]" />
            </Button>
        </div>
        <div className="flex h-full items-center justify-center">
            <Loader className="size-5 animate-spin text-muted-foreground" />
        </div>
      </div>
    );
  }

  if (!member) {
    return (
      <div className="h-full flex flex-col">
        <div className="flex justify-between items-center px-4 h-[49px] border-b">
            <p className="text-lg font-bold">Profile</p>
            <Button onClick={onClose} variant="ghost" size="iconSm">
                <X className="size-5 stroke-[1.5]" />
            </Button>
        </div>
        <div className="flex flex-col items-center justify-center h-full gap-y-2">
          <AlertTriangle className="size-5 text-muted-foreground" />
          <p className="text-sm text-muted-foreground">Profile not found</p>
        </div>
      </div>
    );
  }

  const avatarFallback = member.user.name.charAt(0).toUpperCase();

  return (
    <div className="h-full flex flex-col bg-white border-l">
      <div className="flex justify-between items-center px-4 h-[49px] border-b">
        <p className="text-lg font-bold">Profile</p>
        <Button onClick={onClose} variant="ghost" size="iconSm">
          <X className="size-5 stroke-[1.5]" />
        </Button>
      </div>
      <div className="flex flex-col items-center justify-center p-4">
        <Avatar className="max-w-[256px] max-h-[256px] size-full">
          <AvatarImage src={member.user.image} />
          <AvatarFallback className="aspect-square text-6xl">
            {avatarFallback}
          </AvatarFallback>
        </Avatar>
      </div>
      <div className="flex flex-col p-4">
        <p className="text-xl font-bold">{member.user.name}</p>
        
        {isAdmin && member._id !== currentUser ? (
            <div className="flex items-center gap-2 mt-4">
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="outline" className="w-full capitalize" disabled={isUpdating}>
                            {member.role} <ChevronDown className="size-4 ml-2" />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent className="w-full">
                        <DropdownMenuRadioGroup 
                            value={member.role}
                            onValueChange={onUpdateRole}
                        >
                            <DropdownMenuRadioItem value="admin">Admin</DropdownMenuRadioItem>
                            <DropdownMenuRadioItem value="member">Member</DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
                <Button 
                    variant="outline" 
                    className="w-full text-rose-600 hover:text-rose-600"
                    onClick={onRemove}
                    disabled={isRemoving}
                >
                    Remove
                </Button>
            </div>
        ) : member._id === currentUser && member.role !== 'admin' ? (
            <div className="mt-4">
                <Button variant="outline" className="w-full" onClick={onRemove} disabled={isRemoving}>
                    Leave Workspace
                </Button>
            </div>
        ) : (
            <p className="text-sm text-muted-foreground capitalize">{member.role}</p>
        )}
      </div>
      <Separator />
      <div className="flex flex-col p-4">
        <p className="text-sm font-bold mb-4">Contact information</p>
        <div className="flex items-center gap-2">
          <div className="size-9 rounded-md bg-muted flex items-center justify-center">
            <Mail className="size-4" />
          </div>
          <div className="flex flex-col">
            <p className="text-[13px] font-semibold text-muted-foreground">Email Address</p>
            <a href={`mailto:${member.user.email}`} className="text-sm text-[#1264a3] hover:underline">
                {member.user.email}
            </a>
          </div>
        </div>
      </div>
    </div>
  );
};
