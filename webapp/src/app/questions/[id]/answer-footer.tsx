"use client";

import { deleteAnswer } from "@/lib/actions/question-actions";
import { useAnswerStore } from "@/lib/hooks/use-answer-store";
import { Answer, Author } from "@/lib/types";
import { handleError, timeAgo } from "@/lib/util";
import { Avatar } from "@heroui/avatar";
import { Button } from "@heroui/button";
import type { User } from "next-auth";
import { useTransition } from "react";

type Props = {
  answer: Answer;
  // Resolved by the answer content, which is a server component: there is no session provider on
  // the client, the session is read server-side and handed down.
  currentUser?: User | null;
  // Same reason, same route: the profile service is the source of truth for the name.
  author: Author;
};

export default function AnswerFooter({ answer, currentUser, author }: Props) {
  const [pending, startTransition] = useTransition();
  const editableAnswer = useAnswerStore((state) => state.answer);
  const setAnswer = useAnswerStore((state) => state.setAnswer);

  const handleDelete = () => {
    startTransition(async () => {
      const { error } = await deleteAnswer(answer.questionId, answer.id);

      // The list is refreshed by the revalidation in the action; an accepted answer cannot be
      // deleted, and that 400 is what the toast reports.
      if (error) handleError(error);
    });
  };

  const handleEdit = () => {
    setAnswer(answer);

    // The form has to exist with its new content before it can be scrolled to.
    setTimeout(() => {
      document
        .getElementById("answer-form")
        ?.scrollIntoView({ behavior: "smooth" });
    }, 100);
  };

  return (
    <div className="flex justify-between mt-4">
      <div className="flex items-center mt-auto gap-1">
        {/* Same comparison as the question header: ids, never display names. */}
        {currentUser?.id === answer.authorId && (
          <>
            <Button
              type="button"
              size="sm"
              variant="light"
              color="primary"
              // One answer at a time in the single form, including this one: editing it again
              // while it sits in the form would be a no-op.
              isDisabled={!!editableAnswer}
              onPress={handleEdit}
            >
              edit
            </Button>
            <Button
              type="button"
              size="sm"
              variant="light"
              color="danger"
              // pending belongs to this footer, so only the button of the answer being deleted
              // spins - the course tracks a deleteTarget id for that, which a per-answer
              // component does not need.
              isLoading={pending}
              onPress={handleDelete}
            >
              delete
            </Button>
          </>
        )}
      </div>
      <div className="flex flex-col basis-2/5 bg-primary/10 px-3 p-2 gap-2 rounded">
        {/* "answered" rather than "asked": the block is copied from the question footer. */}
        <span className="text-sm font-extralight">
          answered {timeAgo(answer.createdAt)}
        </span>
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
      </div>
    </div>
  );
}
