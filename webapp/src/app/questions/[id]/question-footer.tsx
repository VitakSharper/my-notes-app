import LinkComponent from "@/components/link-component";
import { Author, Question } from "@/lib/types";
import { timeAgo } from "@/lib/util";
import { Avatar } from "@heroui/avatar";
import { Chip } from "@heroui/chip";

type Props = {
  question: Question;
  author: Author;
};

export default function QuestionFooter({ question, author }: Props) {
  return (
    <div className="flex justify-between mt-2">
      <div className="flex flex-col self-end">
        <div className="flex gap-2">
          {question.tagSlugs.map((tag) => (
            <Chip
              key={tag}
              as={LinkComponent}
              variant="bordered"
              href={`/questions?tag=${tag}`}
            >
              {tag}
            </Chip>
          ))}
        </div>
      </div>
      <div className="flex flex-col basis-2/5 bg-primary/10 px-3 p-2 gap-2 rounded">
        <span className="text-sm font-extralight">
          asked {timeAgo(question.createdAt)}
        </span>
        <div className="flex items-center gap-3">
          <Avatar
            className="h-6 w-6"
            color="secondary"
            name={author.displayName.charAt(0)}
          />
          <div className="flex flex-col items-center">
            <span>{author.displayName}</span>
            {/* Nothing rather than a 0 when the profile did not resolve. */}
            {author.reputation !== null && (
              <span className="self-start text-sm font-semibold">
                {author.reputation}
              </span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
