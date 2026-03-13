'use client';

import { useEffect, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { AuthScreen } from '@/features/auth/components/auth-screen';
import { setAuthToken } from '@/lib/api';
import { Loader } from 'lucide-react';

const AuthContent = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get('token');

  useEffect(() => {
    if (token) {
      setAuthToken(token);
      router.replace('/');
    }
  }, [token, router]);

  if (token) {
    return (
      <div className="flex h-full items-center justify-center bg-[#5C3B58]">
        <Loader className="size-6 animate-spin text-white" />
      </div>
    );
  }

  return <AuthScreen />;
};

const AuthPage = () => {
  return (
    <Suspense fallback={<div className="flex h-full items-center justify-center bg-[#5C3B58]"><Loader className="size-6 animate-spin text-white" /></div>}>
      <AuthContent />
    </Suspense>
  );
};

export default AuthPage;
