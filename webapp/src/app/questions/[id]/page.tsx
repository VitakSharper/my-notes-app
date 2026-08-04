import AnswerContent from "@/app/questions/[id]/answer-content";
import AnswerForm from "@/app/questions/[id]/answer-form";
import AnswersHeader from "@/app/questions/[id]/answers-header";
import QuestionContent from "@/app/questions/[id]/question-content";
import QuestionDetailedHeader from "@/app/questions/[id]/question-detailed-header";
import { getQuestionById } from "@/lib/actions/question-actions";
import { resolveAuthors, toAuthor } from "@/lib/profiles";
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

  // The asker and every answer author, in one request.
  const profiles = await resolveAuthors([
    question.askerId,
    ...question.answers.map((answer) => answer.authorId),
  ]);

  return (
    <div className="w-full">
      <QuestionDetailedHeader question={question} />
      <QuestionContent
        question={question}
        author={toAuthor(question.askerId, question.askerDisplayName, profiles)}
      />
      {question.answers.length > 0 && (
        <AnswersHeader answerCount={question.answers.length} />
      )}
      {question.answers.map((answer) => (
        <AnswerContent answer={answer} key={answer.id} />
      ))}
      <AnswerForm questionId={question.id} />
    </div>
  );
}
