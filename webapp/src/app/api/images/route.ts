import { auth } from "@/auth";
import { deleteImage, uploadImage } from "@/lib/storage";

const maxBytes = 5 * 1024 * 1024;

/**
 * The course signs a direct browser upload to Cloudinary; with MinIO the file goes through this
 * route instead, which is simpler and means the session check cannot be skipped.
 */
export async function POST(request: Request) {
  const session = await auth();

  if (!session) {
    return Response.json({ message: "You must be logged in" }, { status: 401 });
  }

  const formData = await request.formData();
  const file = formData.get("file");

  if (!(file instanceof File)) {
    return Response.json({ message: "No file was sent" }, { status: 400 });
  }

  if (!file.type.startsWith("image/")) {
    return Response.json(
      { message: "Only images can be uploaded" },
      { status: 400 },
    );
  }

  if (file.size > maxBytes) {
    return Response.json(
      { message: "Images must be under 5MB" },
      { status: 400 },
    );
  }

  const image = await uploadImage(file);

  return Response.json(image);
}

export async function DELETE(request: Request) {
  const session = await auth();

  if (!session) {
    return Response.json({ message: "You must be logged in" }, { status: 401 });
  }

  const { publicId } = (await request.json()) as { publicId?: string };

  if (!publicId) {
    return Response.json({ message: "No publicId was sent" }, { status: 400 });
  }

  await deleteImage(publicId);

  return new Response(null, { status: 204 });
}
