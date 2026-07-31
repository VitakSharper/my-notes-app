# Standalone build of the Next.js app, following the official multi-stage example. next.config.ts
# sets output: "standalone", so the build emits a server.js plus only the traced files it needs -
# which is what keeps the runtime stage from carrying the whole node_modules of the build.
FROM node:24-alpine AS base

FROM base AS deps
WORKDIR /app
# Some native prebuilds (sharp among them) expect glibc symbols that musl does not provide.
RUN apk add --no-cache libc6-compat
COPY package.json package-lock.json ./
RUN npm ci

FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

FROM base AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# Not root: the server only ever needs to read what was copied in.
RUN addgroup --system --gid 1001 nodejs && adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
# server.js serves neither public/ nor .next/static unless they sit next to it - the standalone
# output leaves them out on the assumption a CDN handles them.
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

CMD ["node", "server.js"]
