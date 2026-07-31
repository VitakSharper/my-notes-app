import LinkComponent from "@/components/link-component";
import { Button } from "@heroui/button";

export default function NotFound() {
  return (
    <div className="h-full flex items-center justify-center">
      <div className="text-center space-y-6">
        <h1 className="text-5xl font-bold">404 page not found</h1>
        <p className="text-lg text-foreground-500">
          Sorry, the page you are looking for doesn&apos;t exist.
        </p>
        <Button as={LinkComponent} href="/" color="primary">
          Go home
        </Button>
      </div>
    </div>
  );
}
