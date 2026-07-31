import { Answer } from "@/lib/types";
import { timeAgo } from "@/lib/util";
import { Avatar } from "@heroui/avatar";

type Props = {
  answer: Answer;
};

export default function AnswerFooter({ answer }: Props) {
  return (
    <div className="flex justify-end mt-4">
      <div className="flex flex-col basis-2/5 bg-primary/10 px-3 p-2 gap-2 rounded">
        {/* "answered" rather than "asked": the block is copied from the question footer. */}
        <span className="text-sm font-extralight">
          answered {timeAgo(answer.createdAt)}
        </span>
        <div className="flex items-center gap-3">
          {/* The API exposes authorDisplayName, not userDisplayName. */}
          <Avatar
            className="h-6 w-6"
            color="secondary"
            name={answer.authorDisplayName.charAt(0)}
          />
          <div className="flex flex-col items-center">
            <span>{answer.authorDisplayName}</span>
            <span className="self-start text-sm font-semibold">42</span>
          </div>
        </div>
      </div>
    </div>
  );
}
