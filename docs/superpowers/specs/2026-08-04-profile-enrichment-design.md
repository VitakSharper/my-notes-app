# Profile enrichment on questions and answers

The ProfileService owns what the application knows about a user, but nothing in the client asks it
about anyone other than the signed-in visitor. This step makes it the source of truth for the names
and reputations shown next to questions and answers.

## Why

`GET /profiles/batch` was written, routed through the gateway (`AppHost.cs:119`) and typed in the
client (`lib/types/index.ts:51`, "used to enrich questions and answers") — and then called by
nobody. Meanwhile the client shows:

- `question.askerDisplayName` and `answer.authorDisplayName`, denormalised by QuestionService when
  the row was written, so a user who renames themselves keeps the old name on everything they have
  already posted;
- a hardcoded `42` where a reputation belongs, in `question-footer.tsx:40` and
  `answer-footer.tsx:93`.

## Decisions

**The profile replaces the denormalised name.** What `/profiles/batch` returns is what renders. The
name the question carries stays as a fallback, so a missing profile degrades to the old behaviour
rather than to a blank.

**One batch call per page, resolved server-side, handed down as a prop.** This is already the
established path for server-only data reaching a client component: `AnswerContent` awaits the
session and passes `currentUser` to `AnswerFooter`, "the footer needs client hooks, so it cannot
await the session itself" (`answer-content.tsx:11-13`). A Zustand store like `use-tag-store` was
considered and rejected — tags are a small global set cached for an hour, profiles are per-page and
unbounded, and a store would force `question-card` and `question-footer` to become client
components for no gain. A fetch per component was rejected outright: the N+1 is what the batch
endpoint exists to avoid.

**Enrichment failures are silent.** See error handling below.

## Data flow

```
questions/page.tsx            [q.askerId, …]  ──► resolveAuthors ──► Map<id, ProfileSummary>
  └─ QuestionCard             author ◄── toAuthor(askerId, askerDisplayName, map)

questions/[id]/page.tsx       [askerId, …answers.authorId]  ──► resolveAuthors
  ├─ QuestionContent   → QuestionFooter   author
  └─ AnswerContent[]   → AnswerFooter     author
```

`Author` is what the components consume — the fallback is already applied by the time it reaches
them, so no component carries fallback logic:

```ts
type Author = { id: string; displayName: string; reputation: number | null };
```

`reputation: null` means the profile could not be resolved. The components then render no
reputation at all rather than a `0`, which would be as untrue as the `42` it replaces.

## Components

| File | Change |
| --- | --- |
| `lib/types/index.ts` | add `Author` |
| `lib/actions/profile-actions.ts` | new — `getProfiles(ids)` over `/profiles/batch?ids=…`, no round trip on an empty list |
| `lib/profiles.ts` | new — `resolveAuthors(ids)` returning a `Map`, and the pure `toAuthor(id, fallbackName, map)` |
| `app/questions/page.tsx` | collect the asker ids, resolve, pass `author` |
| `app/questions/[id]/page.tsx` | collect asker + answer authors deduplicated, resolve, pass `author` down both branches |
| `app/questions/question-card.tsx` | render `author.displayName` |
| `app/questions/[id]/question-content.tsx` | thread `author` through to the footer |
| `app/questions/[id]/question-footer.tsx` | `author.displayName` + the real reputation |
| `app/questions/[id]/answer-content.tsx` | thread `author` through to the footer |
| `app/questions/[id]/answer-footer.tsx` | `author.displayName` + the real reputation |

`resolveAuthors` lives outside the `"use server"` file on purpose: it is server-only plumbing, not
an action the client may call, and every export of an action module becomes a callable endpoint.

## Error handling

`fetchClient` calls `notFound()` on a 404 and throws on a 500, which is right for the two pages
that cannot render without their question. It is wrong for an enrichment: a ProfileService in
trouble would send the whole question list to the error boundary over a display name. So
`resolveAuthors` wraps the call and treats any failure — thrown or returned — as an empty map.

Three cases, one behaviour:

| Case | Result |
| --- | --- |
| ProfileService or the gateway fails | empty map, every author falls back, no reputation shown |
| The batch answers 200 without a given id | that author falls back, the others resolve normally |
| A user has no profile row yet (never signed in since the service shipped) | same per-author fallback |

The question fetch itself is untouched: it still throws to the error boundary, since there is no
page without it.

No `force-cache` on the batch call. Reputation is meant to move, and freezing it for an hour the
way the tags are cached would defeat the point.

## Out of scope

- Search results — `SearchQuestion` carries no asker at all.
- The `/profiles/[id]` page. `question-card.tsx:74` already links to it and will still 404 when this
  step lands; it is the next step, and it needs a `GET /profiles/{id}` endpoint that does not exist
  yet (the service has only `/me` and `/batch`).
- The `42` on the tag cards. Different known issue, and no endpoint exposes an answer count per tag.

## Verification

There is no test setup in `webapp`, so the same gates the previous lessons used:

- `npx tsc --noEmit`, `npm run lint`, `npm run build` all clean.
- Manually, with the compose stack up and a **fresh login** — which finally exercises
  `/profiles/me`, untested since it was written: the display name on a list card, a real reputation
  in both footers, and exactly one `/profiles/batch` request per page in the network tab.
- One negative check: with `profile-svc` stopped, `/questions` still renders, showing the
  denormalised names and no reputation.
