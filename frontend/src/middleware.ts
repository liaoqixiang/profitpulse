export { auth as middleware } from "@/lib/auth";

export const config = {
  matcher: [
    "/dashboard/:path*",
    "/menu/:path*",
    "/staff/:path*",
    "/trends/:path*",
    "/insights/:path*",
  ],
};
