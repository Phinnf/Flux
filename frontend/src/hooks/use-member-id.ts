'use client';

import { useParams } from 'next/navigation';

type MemberIdParams = {
  memberId: string;
};

export const useMemberId = () => {
  const params = useParams<MemberIdParams>();

  return params.memberId;
};
