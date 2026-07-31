import { Square3Stack3DIcon } from "@heroicons/react/24/solid";

export default function Home() {
  return (
    <div className="flex items-center h-[calc(100vh-160px)] justify-center">
      <div className="flex flex-col justify-center items-center gap-5 text-5xl text-secondary font-bold">
        <Square3Stack3DIcon className="h-96 w-96" />
        <div>Welcome to Cairn!</div>
      </div>
    </div>
  );
}
