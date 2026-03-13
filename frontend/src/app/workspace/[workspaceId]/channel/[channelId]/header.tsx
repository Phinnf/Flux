'use client';

import { ChevronDown, Loader, Trash } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { useWorkspaceId } from '@/hooks/use-workspace-id';

interface ChannelHeaderProps {
  title: string;
}

export const ChannelHeader = ({ title }: ChannelHeaderProps) => {
  const workspaceId = useWorkspaceId();
  const [value, setValue] = useState(title);
  const [editOpen, setEditOpen] = useState(false);

  const handleEditOpen = () => {
    // Check if user is admin
    setEditOpen(true);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value.replace(/\s+/g, '-').toLowerCase();
    setValue(value);
  };

  return (
    <div className="flex h-[49px] items-center overflow-hidden border-b bg-white px-4">
      <Dialog open={editOpen} onOpenChange={setEditOpen}>
        <DialogTrigger asChild>
          <Button
            variant="ghost"
            className="w-auto overflow-hidden px-2 text-lg font-semibold"
            size="sm"
            onClick={handleEditOpen}
          >
            <span className="truncate"># {title}</span>
            <ChevronDown className="ml-2 size-2.5 shrink-0" />
          </Button>
        </DialogTrigger>

        <DialogContent className="overflow-hidden bg-gray-50 p-0">
          <DialogHeader className="border-b bg-white p-4">
            <DialogTitle># {title}</DialogTitle>
          </DialogHeader>

          <div className="flex flex-col gap-y-2 pb-4">
            <div className="cursor-pointer rounded-lg bg-white px-5 py-4 hover:bg-gray-50">
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold">Channel name</p>
                <p className="text-sm font-semibold text-[#1264a3] hover:underline">Edit</p>
              </div>

              <p className="text-sm"># {title}</p>
            </div>

            <button
              className="flex cursor-pointer items-center gap-x-2 rounded-lg bg-white px-5 py-4 text-rose-600 hover:bg-gray-50"
              onClick={() => {}}
            >
              <Trash className="size-4" />
              <p className="text-sm font-semibold">Delete channel</p>
            </button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
};
