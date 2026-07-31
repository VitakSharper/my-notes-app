import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  // Logs every server-side fetch with its URL and whether it was a data-cache hit.
  logging: {
    fetches: {
      fullUrl: true,
    },
  },
};

export default nextConfig;
