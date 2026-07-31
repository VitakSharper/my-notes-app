import QuestionForm from "@/app/questions/ask/question-form";
import { getQuestionById } from "@/lib/actions/question-actions";
import { notFound } from "next/navigation";

type Params = Promise<{ id: string }>;

export default async function EditQuestionPage({
  params,
}: {
  params: Params;
}) {
  const { id } = await params;
  const { data: question, error } = await getQuestionById(id);

  if (error) throw new Error(error.message);
  if (!question) return notFound();

  return (
    <div className="flex flex-col gap-4 px-6">
      <h3 className="text-3xl font-semibold">Edit your question</h3>
      <QuestionForm questionToUpdate={question} />
    </div>
  );
}
