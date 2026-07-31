"use client";

import { deleteQuestion } from "@/lib/actions/question-actions";
import { handleError } from "@/lib/util";
import { Button } from "@heroui/button";
import { useRouter } from "next/navigation";
import { useTransition } from "react";

type Props = {
  questionId: string;
};

/**
 * Its own component purely so the question header can stay a server component: this needs a
 * click handler, a transition and the router.
 */
export default function DeleteQuestionButton({ questionId }: Props) {
  const [pending, startTransition] = useTransition();
  const router = useRouter();

  const handleDelete = () => {
    startTransition(async () => {
      const { error } = await deleteQuestion(questionId);

      if (error) {
        handleError(error);
        return;
      }

      router.push("/questions");
    });
  };

  return (
    <Button
      type="button"
      size="sm"
      variant="faded"
      color="danger"
      isLoading={pending}
      onPress={handleDelete}
    >
      delete
    </Button>
  );
}
