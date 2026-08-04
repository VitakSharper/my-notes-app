import ProfileForm from "@/app/profiles/[id]/profile-form";
import { getCurrentUser } from "@/lib/actions/auth-actions";
import { getProfileById } from "@/lib/actions/profile-actions";
import { notFound } from "next/navigation";

type Params = Promise<{ id: string }>;

export default async function EditProfilePage({ params }: { params: Params }) {
  const { id } = await params;

  const [{ data: profile, error }, currentUser] = await Promise.all([
    getProfileById(id),
    getCurrentUser(),
  ]);

  if (error) throw new Error(error.message);
  if (!profile) return notFound();

  // Compared by id, never by display name - the same rule the question and answer footers follow.
  // The service writes whoever the token says regardless, so this guard is only about not offering
  // a form whose save could never land.
  if (currentUser?.id !== profile.id) return notFound();

  return (
    <div className="flex flex-col gap-4 px-6">
      <h3 className="text-3xl font-semibold">Edit your profile</h3>
      <ProfileForm profile={profile} />
    </div>
  );
}
