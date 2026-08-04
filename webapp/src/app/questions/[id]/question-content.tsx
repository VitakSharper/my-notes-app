import QuestionFooter from "@/app/questions/[id]/question-footer";
import VotingButtons from "@/app/questions/[id]/voting-buttons";
import { Author, Question } from "@/lib/types";

type Props = {
  question: Question;
  author: Author;
};

export default function QuestionContent({ question, author }: Props) {
  return (
    <div className="flex border-b pb-3 px-6">
      <VotingButtons />
      <div className="flex flex-col flex-1">
        <div
          className="mt-4 ml-6 prose dark:prose-invert max-w-none"
          dangerouslySetInnerHTML={{ __html: question.content }}
        />
        <QuestionFooter question={question} author={author} />
      </div>
    </div>
  );
}
