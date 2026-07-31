import AnswerContent from "@/app/questions/[id]/answer-content";
import AnswersHeader from "@/app/questions/[id]/answers-header";
import QuestionContent from "@/app/questions/[id]/question-content";
import QuestionDetailedHeader from "@/app/questions/[id]/question-detailed-header";
import { getQuestionById } from "@/lib/actions/question-actions";
import { notFound } from "next/navigation";

type Params = Promise<{ id: string }>;

export default async function QuestionDetailedPage({
  params,
}: {
  params: Params;
}) {
  const { id } = await params;
  const { data: question, error } = await getQuestionById(id);

  if (error) throw new Error(error.message);
  if (!question) return notFound();

  return (
    <div className="w-full">
      <QuestionDetailedHeader question={question} />
      <QuestionContent question={question} />
      {question.answers.length > 0 && (
        <AnswersHeader answerCount={question.answers.length} />
      )}
      {question.answers.map((answer) => (
        <AnswerContent answer={answer} key={answer.id} />
      ))}
    </div>
  );
}
