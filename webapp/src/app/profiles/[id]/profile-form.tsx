"use client";

import { updateProfile } from "@/lib/actions/profile-actions";
import {
  ProfileSchema,
  ProfileSchemaInput,
  profileSchema,
} from "@/lib/schemas/profile-schema";
import { UserProfile } from "@/lib/types";
import { handleError, successToast } from "@/lib/util";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@heroui/button";
import { Form } from "@heroui/form";
import { Input, Textarea } from "@heroui/input";
import { useRouter } from "next/navigation";
import { useTransition } from "react";
import { useForm } from "react-hook-form";

type Props = {
  profile: UserProfile;
};

export default function ProfileForm({ profile }: Props) {
  const router = useRouter();
  // isSubmitting alone lags behind the redirect, so the transition drives the button state too.
  const [pending, startTransition] = useTransition();

  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isValid, errors },
  } = useForm<ProfileSchemaInput, unknown, ProfileSchema>({
    resolver: zodResolver(profileSchema),
    mode: "onTouched",
    // The profile is fetched by the page, so the values are there on first render - no reset in an
    // effect the way the question form needs one.
    defaultValues: {
      displayName: profile.displayName,
      description: profile.description ?? "",
    },
  });

  const onSubmit = (data: ProfileSchema) => {
    startTransition(async () => {
      const { error } = await updateProfile(data);

      if (error) {
        handleError(error);
        return;
      }

      successToast("Your profile has been updated");
      router.push(`/profiles/${profile.id}`);
    });
  };

  return (
    <Form
      onSubmit={handleSubmit(onSubmit)}
      className="flex flex-col gap-3 p-6 shadow-xl bg-white dark:bg-black"
    >
      <div className="flex flex-col gap-3 w-full">
        <div className="text-2xl font-semibold">Display name</div>
        <Input
          {...register("displayName")}
          type="text"
          className="w-full"
          label="The name shown on your questions and answers"
          labelPlacement="outside-top"
          isInvalid={!!errors.displayName}
          errorMessage={errors.displayName?.message}
        />
      </div>
      <div className="flex flex-col gap-3 w-full">
        <div className="text-2xl font-semibold">About</div>
        <Textarea
          {...register("description")}
          className="w-full"
          label="Anything you would like others to know about you"
          labelPlacement="outside-top"
          minRows={4}
          isInvalid={!!errors.description}
          errorMessage={errors.description?.message}
        />
      </div>
      <Button
        type="submit"
        color="primary"
        className="w-fit"
        isLoading={isSubmitting || pending}
        isDisabled={!isValid || pending}
      >
        update your profile
      </Button>
    </Form>
  );
}
