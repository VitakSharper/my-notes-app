import AnswerFooter from "@/app/questions/[id]/answer-footer";
import VotingButtons from "@/app/questions/[id]/voting-buttons";
import { getCurrentUser } from "@/lib/actions/auth-actions";
import { Answer } from "@/lib/types";

type Props = {
  answer: Answer;
};

export default async function AnswerContent({ answer }: Props) {
  // Read here rather than in the footer: the footer needs client hooks, so it cannot await the
  // session itself.
  const currentUser = await getCurrentUser();

  return (
    <div className="flex border-b pb-3 px-6">
      {/* The API exposes isAccepted, not accepted. */}
      <VotingButtons accepted={answer.isAccepted} />
      <div className="flex flex-col flex-1">
        <div
          className="mt-4 ml-6 prose dark:prose-invert max-w-none"
          dangerouslySetInnerHTML={{ __html: answer.content }}
        />
        <AnswerFooter answer={answer} currentUser={currentUser} />
      </div>
    </div>
  );
}
