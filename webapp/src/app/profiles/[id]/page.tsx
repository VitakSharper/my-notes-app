import { getProfileById } from "@/lib/actions/profile-actions";
import { fuzzyTimeAgo } from "@/lib/util";
import { Avatar } from "@heroui/avatar";
import { notFound } from "next/navigation";

type Params = Promise<{ id: string }>;

export default async function ProfilePage({ params }: { params: Params }) {
  const { id } = await params;
  const { data: profile, error } = await getProfileById(id);

  if (error) throw new Error(error.message);
  // fetchClient already routes a 404 here; this only satisfies the type narrowing.
  if (!profile) return notFound();

  return (
    <div className="w-full p-6 flex flex-col gap-8">
      <div className="flex items-center gap-4">
        <Avatar
          className="h-20 w-20 text-2xl"
          color="secondary"
          name={profile.displayName.charAt(0)}
        />
        <div className="flex flex-col gap-1">
          <div className="text-3xl font-semibold">{profile.displayName}</div>
          <div className="text-sm font-extralight">
            joined {fuzzyTimeAgo(profile.joinedAt)}
          </div>
        </div>
      </div>

      <div className="flex flex-col gap-1 bg-primary/10 px-4 py-3 rounded w-fit">
        <span className="text-sm font-extralight">reputation</span>
        <span className="text-2xl font-semibold">{profile.reputation}</span>
      </div>

      <div className="flex flex-col gap-2">
        <div className="text-xl font-semibold">About</div>
        {/* The description is the one field a user owns, and nothing can write it yet - the Edit
            profile item in the user menu is still inert. */}
        <p>
          {profile.description ??
            "This user has not written anything about themselves yet."}
        </p>
      </div>
    </div>
  );
}
