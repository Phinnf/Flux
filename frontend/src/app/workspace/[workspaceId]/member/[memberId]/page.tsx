'use client';

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { Loader, AlertTriangle } from "lucide-react";
import { useGetOrCreateDirectChannel } from "@/features/channels/api/use-get-or-create-direct-channel";
import { Conversation } from "./conversation";

const MemberPage = () => {
    const params = useParams();
    const workspaceId = params.workspaceId as string;
    const memberId = params.memberId as string;

    const [channelId, setChannelId] = useState<string | null>(null);
    const { mutate, isPending } = useGetOrCreateDirectChannel();

    useEffect(() => {
        if (workspaceId && memberId) {
            mutate({ workspaceId, targetUserId: memberId }, {
                onSuccess: (data: any) => {
                    setChannelId(data.value._id);
                }
            });
        }
    }, [workspaceId, memberId, mutate]);

    if (isPending) {
        return (
            <div className="h-full flex items-center justify-center">
                <Loader className="size-6 animate-spin text-muted-foreground" />
            </div>
        );
    }

    if (!channelId) {
        return (
            <div className="h-full flex flex-col gap-y-2 items-center justify-center">
                <AlertTriangle className="size-6 text-muted-foreground" />
                <p className="text-sm text-muted-foreground">Conversation not found</p>
            </div>
        );
    }

    return <Conversation memberId={memberId} channelId={channelId} />;
};

export default MemberPage;
