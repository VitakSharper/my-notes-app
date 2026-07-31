import { getQuestions } from "@/lib/actions/question-actions";
import QuestionCard from "@/app/questions/question-card";
import QuestionsHeader from "@/app/questions/questions-header";

type Props = {
  searchParams?: Promise<{ tag?: string }>;
};

export default async function QuestionsPage({ searchParams }: Props) {
  const params = await searchParams;
  const { data: questions, error } = await getQuestions(params?.tag);

  // A server page has nowhere to show a toast, so a failure goes to the error boundary.
  if (error) throw new Error(error.message);

  return (
    <>
      <QuestionsHeader total={questions?.length ?? 0} tag={params?.tag} />
      {questions?.map((question) => (
        <div key={question.id} className="py-4 not-last:border-b w-full flex">
          <QuestionCard question={question} />
        </div>
      ))}
    </>
  );
}
