import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

const isPublicPage = (url: string) => url.startsWith('/auth');

export function middleware(req: NextRequest) {
  const token = req.cookies.get('token')?.value;
  const isPublic = isPublicPage(req.nextUrl.pathname);

  if (!isPublic && !token) {
    return NextResponse.redirect(new URL('/auth', req.url));
  }

  if (isPublic && token) {
    return NextResponse.redirect(new URL('/', req.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!.*\\..*|_next).*)', '/', '/(api|trpc)(.*)'],
};
