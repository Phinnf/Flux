'use client';

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Loader } from "lucide-react";
import { useGetOrCreateDirectChannel } from "@/features/channels/api/use-get-or-create-direct-channel";
import { toast } from "sonner";

const MemberPage = () => {
    const params = useParams();
    const router = useRouter();
    const workspaceId = params.workspaceId as string;
    const memberId = params.memberId as string;

    const { mutate, isPending } = useGetOrCreateDirectChannel();

    useEffect(() => {
        if (workspaceId && memberId) {
            mutate({ workspaceId, targetUserId: memberId }, {
                onSuccess: (data: any) => {
                    const channelId = data.value._id;
                    // Redirect to the direct channel
                    router.replace(`/workspace/${workspaceId}/channel/${channelId}`);
                },
                onError: () => {
                    toast.error("Failed to start conversation");
                    router.replace(`/workspace/${workspaceId}`);
                }
            });
        }
    }, [workspaceId, memberId, mutate, router]);

    return (
        <div className="h-full flex items-center justify-center">
            <Loader className="size-6 animate-spin text-muted-foreground" />
        </div>
    );
};

export default MemberPage;
