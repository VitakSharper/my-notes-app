# Profile Enrichment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the ProfileService the source of truth for the display name and reputation shown beside every question and answer in the client.

**Architecture:** Each page resolves all the user ids it is about to render in a single
`GET /profiles/batch` call, server-side, and hands each component an `Author` with the fallback
already applied. This mirrors the existing path for server-only data reaching a client component:
`AnswerContent` awaits the session and passes `currentUser` down to `AnswerFooter`. The resolver
never throws, so a ProfileService failure degrades to the denormalised names the questions already
carry instead of taking the page down.

**Tech Stack:** Next.js 16 App Router, React server components, TypeScript, HeroUI, Tailwind v4.

## Global Constraints

- **Next.js 16 is not the Next.js you know.** Read the relevant guide in
  `webapp/node_modules/next/dist/docs/` before writing code (`webapp/AGENTS.md`).
- `src/lib/fetch-client.ts` is the only place that calls `fetch`. Every API call goes through a
  `"use server"` action in `src/lib/actions/`.
- Server pages throw so `app/error.tsx` catches; client components call an action inside
  `useTransition` and toast with `handleError`.
- Ownership comparisons use ids, never display names (`answer-footer.tsx:48`).
- **No test framework exists in `webapp`.** The gates are `npx tsc --noEmit`, `npm run lint` and
  `npm run build`, run from `webapp/`. Task 5 adds the manual pass.
- 2-space indentation, Prettier formatting, `@/` path alias.
- Work on `main`. Commit per task.

---

### Task 1: The resolver

The data layer, with no rendering attached: the type the components will consume, the action that
calls the batch endpoint, and the non-throwing resolver.

**Files:**
- Modify: `webapp/src/lib/types/index.ts` (append after `ProfileSummary`, line 56)
- Create: `webapp/src/lib/actions/profile-actions.ts`
- Create: `webapp/src/lib/profiles.ts`

**Interfaces:**
- Consumes: `ProfileSummary` from `@/lib/types` (`{ userId, displayName, reputation }`), `fetchClient`
  from `@/lib/fetch-client`.
- Produces:
  - `type Author = { id: string; displayName: string; reputation: number | null }`
  - `getProfiles(ids: string[]): Promise<ApiResponse<ProfileSummary[]>>`
  - `resolveAuthors(ids: string[]): Promise<Map<string, ProfileSummary>>`
  - `toAuthor(id: string, fallbackName: string, profiles: Map<string, ProfileSummary>): Author`

- [ ] **Step 1: Add the `Author` type**

Append to `webapp/src/lib/types/index.ts`:

```ts
/**
 * What the author components render. The fallback is applied before it reaches them, so no
 * component carries fallback logic of its own. A null reputation means the profile did not
 * resolve - the footers then show nothing rather than a 0, which would be as untrue as the
 * hardcoded 42 it replaces.
 */
export type Author = {
  id: string;
  displayName: string;
  reputation: number | null;
};
```

- [ ] **Step 2: Add the batch action**

Create `webapp/src/lib/actions/profile-actions.ts`:

```ts
"use server";

import { fetchClient } from "@/lib/fetch-client";
import { ProfileSummary } from "@/lib/types";

/**
 * The display name and reputation for a set of user ids, in one request.
 *
 * The endpoint is anonymous on the service side on purpose: questions are readable without signing
 * in, so their authors have to resolve for those readers too.
 */
export async function getProfiles(ids: string[]) {
  // The endpoint would answer 200 with an empty array, but there is no point in the round trip.
  if (ids.length === 0) return { data: [] as ProfileSummary[] };

  return fetchClient<ProfileSummary[]>(
    `/profiles/batch?ids=${ids.map(encodeURIComponent).join(",")}`,
    "GET",
  );
}
```

- [ ] **Step 3: Add the resolver**

Create `webapp/src/lib/profiles.ts`. Not a `"use server"` module on purpose: this is server-only
plumbing, not something the client may call, and every export of an action module becomes a
callable endpoint.

```ts
import { getProfiles } from "@/lib/actions/profile-actions";
import { Author, ProfileSummary } from "@/lib/types";

/**
 * The profiles for a set of user ids, keyed by id.
 *
 * Never throws and never navigates. fetchClient sends a 404 to the not-found page and a 500 to the
 * error boundary, which is right for a question but wrong for the name beside it: a ProfileService
 * in trouble must not take a question list down. Any failure comes back as an empty map and every
 * author falls back to the name the question already carries.
 */
export async function resolveAuthors(ids: string[]) {
  // The endpoint deduplicates too, but the same user holding several answers should not pad the
  // query string.
  const unique = [...new Set(ids)];

  try {
    const { data } = await getProfiles(unique);

    return new Map((data ?? []).map((profile) => [profile.userId, profile] as const));
  } catch {
    return new Map<string, ProfileSummary>();
  }
}

/** Applies the fallback once, so no component has to. */
export function toAuthor(
  id: string,
  fallbackName: string,
  profiles: Map<string, ProfileSummary>,
): Author {
  const profile = profiles.get(id);

  return {
    id,
    displayName: profile?.displayName ?? fallbackName,
    reputation: profile?.reputation ?? null,
  };
}
```

- [ ] **Step 4: Verify it compiles and lints**

Run from `webapp/`:

```bash
npx tsc --noEmit && npm run lint
```

Expected: no output from `tsc`, and `eslint` clean. Nothing consumes the new module yet, so a
failure here is a type error in the resolver itself — most likely the `as const` on the Map entries,
which is what makes the tuple infer as `[string, ProfileSummary]` rather than
`(string | ProfileSummary)[]`.

- [ ] **Step 5: Commit**

```bash
git add webapp/src/lib/types/index.ts webapp/src/lib/actions/profile-actions.ts webapp/src/lib/profiles.ts
git commit -m "feat(webapp): resolve author profiles in one batch call"
```

---

### Task 2: The question list

The list card is the simplest consumer — one author per card, no reputation shown — so it proves the
resolver end to end before the detail page threads it through two branches.

**Files:**
- Modify: `webapp/src/app/questions/page.tsx`
- Modify: `webapp/src/app/questions/question-card.tsx:10-12` (props), `:69-76` (the author block)

**Interfaces:**
- Consumes: `resolveAuthors`, `toAuthor` from `@/lib/profiles`; `Author` from `@/lib/types`.
- Produces: `QuestionCard` now requires an `author: Author` prop.

- [ ] **Step 1: Resolve the askers in the page**

In `webapp/src/app/questions/page.tsx`, add the import and resolve after the error check:

```tsx
import { resolveAuthors, toAuthor } from "@/lib/profiles";
```

```tsx
  // A server page has nowhere to show a toast, so a failure goes to the error boundary.
  if (error) throw new Error(error.message);

  // One request for every asker on the page, not one per card.
  const profiles = await resolveAuthors(
    questions?.map((question) => question.askerId) ?? [],
  );
```

- [ ] **Step 2: Pass the author to the card**

Replace the `<QuestionCard question={question} />` call:

```tsx
          <QuestionCard
            question={question}
            author={toAuthor(
              question.askerId,
              question.askerDisplayName,
              profiles,
            )}
          />
```

- [ ] **Step 3: Render the resolved author**

In `webapp/src/app/questions/question-card.tsx`, extend the props:

```tsx
import { Author, Question } from "@/lib/types";

type Props = {
  question: Question;
  // Resolved by the page: the profile service is the source of truth for the name, and the name
  // the question carries is only the fallback.
  author: Author;
};

export default function QuestionCard({ question, author }: Props) {
```

Then replace the author block (currently `:69-76`):

```tsx
            <Avatar
              className="h-6 w-6"
              color="secondary"
              name={author.displayName.charAt(0)}
            />
            <Link href={`/profiles/${author.id}`}>{author.displayName}</Link>
```

- [ ] **Step 4: Verify**

Run from `webapp/`:

```bash
npx tsc --noEmit && npm run lint
```

Expected: clean. A type error naming `author` as missing means a `QuestionCard` call site was
missed — `git grep -n "QuestionCard"` should show only `page.tsx` and the component itself.

- [ ] **Step 5: Commit**

```bash
git add webapp/src/app/questions/page.tsx webapp/src/app/questions/question-card.tsx
git commit -m "feat(webapp): show the profile display name on question cards"
```

---

### Task 3: The question detail, asker side

Same resolution on the detail page, threaded through `QuestionContent` to the footer — and the
footer is where the first hardcoded `42` dies.

**Files:**
- Modify: `webapp/src/app/questions/[id]/page.tsx`
- Modify: `webapp/src/app/questions/[id]/question-content.tsx`
- Modify: `webapp/src/app/questions/[id]/question-footer.tsx:7-11` (props), `:32-42` (the author block)

**Interfaces:**
- Consumes: `resolveAuthors`, `toAuthor`, `Author`.
- Produces: `QuestionContent` and `QuestionFooter` both require an `author: Author` prop. The
  `profiles` map built in `page.tsx` is reused by Task 4 for the answer authors.

- [ ] **Step 1: Resolve everyone the page renders**

In `webapp/src/app/questions/[id]/page.tsx`, add the import and resolve after the guards. The answer
authors are included now even though Task 4 is what consumes them — one call covers the page:

```tsx
import { resolveAuthors, toAuthor } from "@/lib/profiles";
```

```tsx
  if (error) throw new Error(error.message);
  if (!question) return notFound();

  // The asker and every answer author, in one request.
  const profiles = await resolveAuthors([
    question.askerId,
    ...question.answers.map((answer) => answer.authorId),
  ]);
```

- [ ] **Step 2: Pass the asker down**

Replace the `<QuestionContent question={question} />` call:

```tsx
      <QuestionContent
        question={question}
        author={toAuthor(
          question.askerId,
          question.askerDisplayName,
          profiles,
        )}
      />
```

- [ ] **Step 3: Thread it through the content**

In `webapp/src/app/questions/[id]/question-content.tsx`:

```tsx
import { Author, Question } from "@/lib/types";

type Props = {
  question: Question;
  author: Author;
};

export default function QuestionContent({ question, author }: Props) {
```

and pass it on:

```tsx
        <QuestionFooter question={question} author={author} />
```

- [ ] **Step 4: Render the name and the real reputation**

In `webapp/src/app/questions/[id]/question-footer.tsx`:

```tsx
import { Author, Question } from "@/lib/types";

type Props = {
  question: Question;
  author: Author;
};

export default function QuestionFooter({ question, author }: Props) {
```

Replace the author block (currently `:32-42`):

```tsx
        <div className="flex items-center gap-3">
          <Avatar
            className="h-6 w-6"
            color="secondary"
            name={author.displayName.charAt(0)}
          />
          <div className="flex flex-col items-center">
            <span>{author.displayName}</span>
            {/* Nothing rather than a 0 when the profile did not resolve. */}
            {author.reputation !== null && (
              <span className="self-start text-sm font-semibold">
                {author.reputation}
              </span>
            )}
          </div>
        </div>
```

- [ ] **Step 5: Verify**

Run from `webapp/`:

```bash
npx tsc --noEmit && npm run lint
```

Then confirm the first `42` is gone:

```bash
git grep -n "42" -- webapp/src/app/questions
```

Expected: `answer-footer.tsx` still matches (Task 4 removes it), `question-footer.tsx` no longer
does.

- [ ] **Step 6: Commit**

```bash
git add webapp/src/app/questions/[id]/page.tsx webapp/src/app/questions/[id]/question-content.tsx webapp/src/app/questions/[id]/question-footer.tsx
git commit -m "feat(webapp): real asker name and reputation on the question footer"
```

---

### Task 4: The question detail, answer side

The answer footer is a client component, so its author arrives the way its `currentUser` already
does: resolved by the server component above it and handed down.

**Files:**
- Modify: `webapp/src/app/questions/[id]/page.tsx` (the `AnswerContent` call only)
- Modify: `webapp/src/app/questions/[id]/answer-content.tsx`
- Modify: `webapp/src/app/questions/[id]/answer-footer.tsx:12-19` (props), `:84-95` (the author block)

**Interfaces:**
- Consumes: the `profiles` map already built in `page.tsx` by Task 3, plus `toAuthor` and `Author`.
- Produces: `AnswerContent` and `AnswerFooter` both require an `author: Author` prop.

- [ ] **Step 1: Pass each answer's author down**

In `webapp/src/app/questions/[id]/page.tsx`, replace the `AnswerContent` call:

```tsx
      {question.answers.map((answer) => (
        <AnswerContent
          answer={answer}
          key={answer.id}
          author={toAuthor(answer.authorId, answer.authorDisplayName, profiles)}
        />
      ))}
```

- [ ] **Step 2: Thread it through the content**

In `webapp/src/app/questions/[id]/answer-content.tsx`:

```tsx
import { Answer, Author } from "@/lib/types";

type Props = {
  answer: Answer;
  author: Author;
};

export default async function AnswerContent({ answer, author }: Props) {
```

and pass it on, next to the session it already resolves:

```tsx
        <AnswerFooter answer={answer} currentUser={currentUser} author={author} />
```

- [ ] **Step 3: Render the name and the real reputation**

In `webapp/src/app/questions/[id]/answer-footer.tsx`, extend the props:

```tsx
import { Answer, Author } from "@/lib/types";
```

```tsx
type Props = {
  answer: Answer;
  // Resolved by the answer content, which is a server component: there is no session provider on
  // the client, the session is read server-side and handed down.
  currentUser?: User | null;
  // Same reason, same route: the profile service is the source of truth for the name.
  author: Author;
};

export default function AnswerFooter({ answer, currentUser, author }: Props) {
```

Replace the author block (currently `:84-95`), dropping the stale comment about
`authorDisplayName`:

```tsx
        <div className="flex items-center gap-3">
          <Avatar
            className="h-6 w-6"
            color="secondary"
            name={author.displayName.charAt(0)}
          />
          <div className="flex flex-col items-center">
            <span>{author.displayName}</span>
            {/* Nothing rather than a 0 when the profile did not resolve. */}
            {author.reputation !== null && (
              <span className="self-start text-sm font-semibold">
                {author.reputation}
              </span>
            )}
          </div>
        </div>
```

- [ ] **Step 4: Verify**

Run from `webapp/`:

```bash
npx tsc --noEmit && npm run lint
```

Then confirm both hardcoded reputations are gone:

```bash
git grep -n "42" -- webapp/src/app/questions
```

Expected: no matches.

- [ ] **Step 5: Commit**

```bash
git add webapp/src/app/questions/[id]/page.tsx webapp/src/app/questions/[id]/answer-content.tsx webapp/src/app/questions/[id]/answer-footer.tsx
git commit -m "feat(webapp): real author name and reputation on answers"
```

---

### Task 5: Verify against the running stack

The type checker cannot tell whether the batch call is actually made once, resolves, or degrades
correctly. This task is the manual gate the previous lessons used, plus the negative case the spec
asks for.

**Files:** none — verification only, then the README.

- [ ] **Step 1: Production build**

Run from `webapp/`:

```bash
npm run build
```

Expected: compiles. `output: "standalone"` is on, so this is also what the container build runs.

- [ ] **Step 2: Bring the stack up**

```powershell
.\scripts\overflow.ps1 start
```

Expected: the preflight warns about nothing, and the wait loop reports keycloak, minio and the
gateway answering. The four `*.overflow.local` names must be in the hosts file.

- [ ] **Step 3: Sign in fresh**

Open https://app.overflow.local, sign out if a session is already there, and sign in as `bob`.

This is the round trip lesson 138 never exercised: `auth.ts` calls `/profiles/me` at sign-in, the
service's middleware creates the row on a first visit, and the response becomes `session.user`.
Expected: the user menu shows a display name, not an empty label.

- [ ] **Step 4: Check the three surfaces**

| Where | Expected |
| --- | --- |
| `/questions` | each card shows a display name from the profile service |
| `/questions/{id}` footer | the asker's name and a real reputation, not 42 |
| an answer footer | the author's name and a real reputation, not 42 |

In the browser network tab, confirm **one** `/profiles/batch` request per page load — not one per
card or per answer.

- [ ] **Step 5: The negative case**

```powershell
docker stop overflow-profile-svc-1
```

Reload `/questions`. Expected: the page still renders, showing the denormalised names and no
reputation — it must not land on the error boundary. Then:

```powershell
docker start overflow-profile-svc-1
```

If the container name differs, `.\scripts\overflow.ps1 status` lists them.

- [ ] **Step 6: Update the README**

The client-app conventions list in `README.md` gains a line, since the batch call is now part of how
a page renders:

```markdown
- Display names and reputations come from the profile service, not from the question: each page
  resolves every author it renders in one `GET /profiles/batch` call and hands each component an
  `Author` with the denormalised name as a fallback (`src/lib/profiles.ts`). A profile-service
  failure degrades to those fallback names rather than to the error boundary.
```

Leave the Known issues section alone: the tag-card `42` and the missing ownership check on
`DELETE /questions/{id}` are both untouched by this work.

- [ ] **Step 7: Commit**

```bash
git add README.md
git commit -m "docs: record how author profiles are resolved"
```

---

## Verification summary

| Gate | Command | Task |
| --- | --- | --- |
| Types | `npx tsc --noEmit` | 1–4 |
| Lint | `npm run lint` | 1–4 |
| Build | `npm run build` | 5 |
| No hardcoded reputation left | `git grep -n "42" -- webapp/src/app/questions` | 4 |
| One batch call per page | browser network tab | 5 |
| Degrades without the service | `docker stop overflow-profile-svc-1` | 5 |
