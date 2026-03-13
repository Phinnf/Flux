'use client';

import { useParams } from 'next/navigation';

type WorkspaceIdParams = {
  workspaceId: string;
};

export const useWorkspaceId = () => {
  const params = useParams<WorkspaceIdParams>();

  return params.workspaceId;
};
