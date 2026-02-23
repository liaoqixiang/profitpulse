import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  productionBrowserSourceMaps: false,
  webpack: (config, { isServer }) => {
    // Fix recharts "Unexpected end of JSON input" by disabling source maps for node_modules
    if (!isServer) {
      config.module.rules.push({
        test: /\.js$/,
        enforce: "pre" as const,
        include: /node_modules/,
        use: ["source-map-loader"],
        resolve: {
          fullySpecified: false,
        },
      });
      // Ignore source map warnings from node_modules
      config.ignoreWarnings = [
        ...(config.ignoreWarnings || []),
        /Failed to parse source map/,
      ];
    }
    return config;
  },
};

export default nextConfig;
