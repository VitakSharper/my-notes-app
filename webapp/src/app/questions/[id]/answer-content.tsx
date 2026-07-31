import AnswerFooter from "@/app/questions/[id]/answer-footer";
import VotingButtons from "@/app/questions/[id]/voting-buttons";
import { Answer } from "@/lib/types";

type Props = {
  answer: Answer;
};

export default function AnswerContent({ answer }: Props) {
  return (
    <div className="flex border-b pb-3 px-6">
      {/* The API exposes isAccepted, not accepted. */}
      <VotingButtons accepted={answer.isAccepted} />
      <div className="flex flex-col flex-1">
        <div
          className="mt-4 ml-6 prose dark:prose-invert max-w-none"
          dangerouslySetInnerHTML={{ __html: answer.content }}
        />
        <AnswerFooter answer={answer} />
      </div>
    </div>
  );
}
