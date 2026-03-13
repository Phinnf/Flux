'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { useWorkspaceId } from '@/hooks/use-workspace-id';

import { useCreateChannel } from '../api/use-create-channel';
import { useCreateChannelModal } from '../store/use-create-channel-modal';

export const CreateChannelModal = () => {
  const router = useRouter();
  const workspaceId = useWorkspaceId();

  const [name, setName] = useState('');
  const [open, setOpen] = useCreateChannelModal();
  const { isPending, mutate } = useCreateChannel();

  const handleClose = () => {
    setOpen(false);
    setName('');
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    mutate(
      { name, workspaceId },
      {
        onSuccess: (id) => {
          toast.success('Channel created!');
          router.push(`/workspace/${workspaceId}/channel/${id}`);

          handleClose();
        },
        onError: (error) => {
          console.error('[CREATE_CHANNEL]: ', error);
          toast.error('Failed to create channel.');
        },
      },
    );
  };

  return (
    <Dialog open={open || isPending} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add a channel</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            disabled={isPending}
            value={name}
            onChange={(e) => {
              const value = e.target.value.replace(/\s+/g, '-').toLowerCase();
              setName(value);
            }}
            required
            autoFocus
            minLength={3}
            maxLength={20}
            placeholder="e.g. plan-budget"
          />

          <div className="flex justify-end">
            <Button disabled={isPending}>Create</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
};
