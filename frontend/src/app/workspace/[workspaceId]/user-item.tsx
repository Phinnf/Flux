import { LucideIcon } from 'lucide-react';
import Link from 'next/link';
import { cva, type VariantProps } from 'class-variance-authority';

import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import { useWorkspaceId } from '@/hooks/use-workspace-id';
import { cn } from '@/lib/utils';

const userItemVariants = cva(
  'flex items-center gap-1.5 justify-start font-normal h-7 px-4 text-sm overflow-hidden',
  {
    variants: {
      variant: {
        default: 'text-[#f9edffcc]',
        active: 'text-[#481349] bg-white/90 hover:bg-white/90',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  },
);

interface UserItemProps {
  id: string;
  label?: string;
  image?: string;
  variant?: VariantProps<typeof userItemVariants>['variant'];
  isOnline?: boolean;
}

export const UserItem = ({ id, label = 'Member', image, variant, isOnline }: UserItemProps) => {
  const workspaceId = useWorkspaceId();
  const avatarFallback = label.charAt(0).toUpperCase();

  return (
    <Button asChild variant="transparent" size="sm" className={cn(userItemVariants({ variant }))}>
      <Link href={`/workspace/${workspaceId}/member/${id}`}>
        <div className="relative mr-1">
          <Avatar className="size-5 rounded-md">
            <AvatarImage className="rounded-md" src={image} />
            <AvatarFallback className="rounded-md bg-sky-500 text-xs text-white">
              {avatarFallback}
            </AvatarFallback>
          </Avatar>
          <span className={cn(
            "absolute -bottom-0.5 -right-0.5 size-2 rounded-full border-2 border-[#5E2C5F]",
            isOnline ? "bg-green-500" : "bg-gray-500",
            variant === 'active' && "border-white"
          )} />
        </div>

        <span className="truncate text-sm">{label}</span>
      </Link>
    </Button>
  );
};
