export type Question = {
  id: string;
  title: string;
  content: string;
  askerId: string;
  askerDisplayName: string;
  createdAt: string;
  updatedAt: string | null;
  viewCount: number;
  tagSlugs: string[];
  hasAcceptedAnswer: boolean;
  votes: number;
  answerCount: number;
  // Only populated by GET /questions/{id} - the list endpoint never includes answers.
  answers: Answer[];
};

export type Answer = {
  id: string;
  content: string;
  questionId: string;
  authorId: string;
  authorDisplayName: string;
  createdAt: string;
  updatedAt: string | null;
  isAccepted: boolean;
  votes: number;
};

// What the SearchService returns from Typesense: a flattened document, not a full Question
// (no asker, no votes, and createdAt is an epoch in seconds).
export type SearchQuestion = {
  id: string;
  title: string;
  content: string;
  tags: string[];
  createdAt: number;
  hasAcceptedAnswer: boolean;
  answerCount: number;
};

/** What GET /profiles/me returns: the profile service's view of a user. */
export type UserProfile = {
  id: string;
  displayName: string;
  description: string | null;
  joinedAt: string;
  reputation: number;
};

/** The slice GET /profiles/batch returns, used to enrich questions and answers. */
export type ProfileSummary = {
  userId: string;
  displayName: string;
  reputation: number;
};

export type Tag = {
  id: string;
  name: string;
  slug: string;
  description: string;
};

export type ApiError = {
  message: string;
  status: number;
};

// A server action cannot throw an error across the server/client boundary, so the fetch client
// hands the failure back as data and the caller picks between a toast and the error boundary.
export type ApiResponse<T> = {
  data: T | null;
  error?: ApiError;
};
