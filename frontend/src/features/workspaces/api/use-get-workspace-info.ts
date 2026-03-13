import { useQuery } from '@tanstack/react-query';
import { fetchApi } from '@/lib/api';

interface useGetWorkspaceInfoProps {
  id: string;
}

export const useGetWorkspaceInfo = ({ id }: useGetWorkspaceInfoProps) => {
  const { data, isLoading, error } = useQuery({
    queryKey: ['workspace-info', id],
    queryFn: () => fetchApi(`/api/workspaces/${id}/info`),
    enabled: !!id,
  });

  return { data, isLoading, error };
};
