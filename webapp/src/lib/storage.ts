import {
  CreateBucketCommand,
  DeleteObjectCommand,
  HeadBucketCommand,
  PutBucketPolicyCommand,
  PutObjectCommand,
  S3Client,
} from "@aws-sdk/client-s3";

/**
 * Image storage on MinIO, the S3-compatible container the AppHost starts. This stands in for the
 * course's Cloudinary account and keeps the same contract: an upload returns a public URL plus a
 * key ("publicId") the delete path can use later.
 */
const bucket = process.env.MINIO_BUCKET ?? "overflow";
const publicBaseUrl =
  process.env.NEXT_PUBLIC_IMAGE_BASE_URL ?? `http://localhost:9000/${bucket}`;

// forcePathStyle is required: MinIO serves buckets as /bucket/key, not as a subdomain.
const client = new S3Client({
  endpoint: process.env.MINIO_ENDPOINT ?? "http://localhost:9000",
  region: "us-east-1",
  forcePathStyle: true,
  credentials: {
    accessKeyId: process.env.MINIO_ACCESS_KEY ?? "",
    secretAccessKey: process.env.MINIO_SECRET_KEY ?? "",
  },
});

let bucketReady = false;

/**
 * Creates the bucket on first use and opens it for anonymous reads, because the browser loads
 * the images straight from MinIO. Only GetObject is public: writing still needs credentials.
 */
async function ensureBucket() {
  if (bucketReady) return;

  try {
    await client.send(new HeadBucketCommand({ Bucket: bucket }));
  } catch {
    await client.send(new CreateBucketCommand({ Bucket: bucket }));
    await client.send(
      new PutBucketPolicyCommand({
        Bucket: bucket,
        Policy: JSON.stringify({
          Version: "2012-10-17",
          Statement: [
            {
              Effect: "Allow",
              Principal: { AWS: ["*"] },
              Action: ["s3:GetObject"],
              Resource: [`arn:aws:s3:::${bucket}/*`],
            },
          ],
        }),
      }),
    );
  }

  bucketReady = true;
}

export async function uploadImage(file: File) {
  await ensureBucket();

  // The key doubles as the publicId: unguessable, and it keeps the original extension so the
  // browser gets a sensible URL.
  const extension = file.name.includes(".")
    ? file.name.slice(file.name.lastIndexOf("."))
    : "";
  const key = `images/${crypto.randomUUID()}${extension}`;

  await client.send(
    new PutObjectCommand({
      Bucket: bucket,
      Key: key,
      Body: Buffer.from(await file.arrayBuffer()),
      ContentType: file.type,
    }),
  );

  return { url: `${publicBaseUrl}/${key}`, publicId: key };
}

export async function deleteImage(publicId: string) {
  await client.send(
    new DeleteObjectCommand({ Bucket: bucket, Key: publicId }),
  );
}
