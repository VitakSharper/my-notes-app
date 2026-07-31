import LinkComponent from "@/components/link-component";
import { getCurrentUser } from "@/lib/actions/auth-actions";
import { Question } from "@/lib/types";
import { fuzzyTimeAgo } from "@/lib/util";
import { Button } from "@heroui/button";

type Props = {
  question: Question;
};

export default async function QuestionDetailedHeader({ question }: Props) {
  const currentUser = await getCurrentUser();

  return (
    <div className="flex flex-col w-full border-b gap-4 pb-4 px-6">
      <div className="flex justify-between gap-4">
        <div className="text-3xl font-semibold first-letter:uppercase">
          {question.title}
        </div>
        <Button
          as={LinkComponent}
          href="/questions/ask"
          color="secondary"
          className="w-[20%]"
        >
          ask question
        </Button>
      </div>
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-6">
          <div className="flex items-center gap-3">
            <span className="text-foreground-500">asked</span>
            <span>{fuzzyTimeAgo(question.createdAt)}</span>
          </div>
          {question.updatedAt && (
            <div className="flex items-center gap-3">
              <span className="text-foreground-500">modified</span>
              <span>{fuzzyTimeAgo(question.updatedAt)}</span>
            </div>
          )}
          <div className="flex items-center gap-3">
            <span className="text-foreground-500">viewed</span>
            {/* No +1 here: GetQuestionById increments ViewCount before returning, so the
                value we receive is already the post-increment count. */}
            <span>{question.viewCount} times</span>
          </div>
        </div>
        {/* The id comes from the Keycloak profile through the session; comparing display names
            would be unreliable since nothing stops two users sharing one. */}
        {currentUser?.id === question.askerId && (
          <div className="flex items-center gap-3">
            <Button
              as={LinkComponent}
              href={`/questions/${question.id}/edit`}
              size="sm"
              variant="faded"
              color="primary"
            >
              edit
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
