import LinkComponent from "@/components/link-component";
import { Tag } from "@/lib/types";
import { Card, CardBody, CardFooter, CardHeader } from "@heroui/card";
import { Chip } from "@heroui/chip";

type Props = {
  tag: Tag;
};

export default function TagCard({ tag }: Props) {
  return (
    // LinkComponent rather than next/link: a function prop cannot cross the RSC boundary
    // from this server component into the client-side Card.
    <Card
      as={LinkComponent}
      href={`/questions?tag=${tag.slug}`}
      isHoverable
      isPressable
    >
      <CardHeader>
        <Chip variant="bordered">{tag.slug}</Chip>
      </CardHeader>
      <CardBody>
        <p className="line-clamp-3">{tag.description}</p>
      </CardBody>
      <CardFooter>42 questions</CardFooter>
    </Card>
  );
}
