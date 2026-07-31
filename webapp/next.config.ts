import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Emits .next/standalone with its own server.js, which is what the Dockerfile runs.
  output: "standalone",
  reactCompiler: true,
  // Logs every server-side fetch with its URL and whether it was a data-cache hit.
  logging: {
    fetches: {
      fullUrl: true,
    },
  },
};

export default nextConfig;
