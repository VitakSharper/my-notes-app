"use client";

import RichTextEditor from "@/components/editor/rich-text-editor";
import {
  postQuestion,
  updateQuestion,
} from "@/lib/actions/question-actions";
import { useTagStore } from "@/lib/hooks/use-tag-store";
import { Question } from "@/lib/types";
import {
  QuestionSchema,
  QuestionSchemaInput,
  questionSchema,
} from "@/lib/schemas/question-schema";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@heroui/button";
import { Form } from "@heroui/form";
import { Input } from "@heroui/input";
import { Select, SelectItem } from "@heroui/select";
import { handleError } from "@/lib/util";
import clsx from "clsx";
import { useRouter } from "next/navigation";
import { useEffect, useTransition } from "react";
import { Controller, useForm } from "react-hook-form";

type Props = {
  // Present when the form is used to edit; absent when asking a new question.
  questionToUpdate?: Question;
};

export default function QuestionForm({ questionToUpdate }: Props) {
  const tags = useTagStore((state) => state.tags);
  const router = useRouter();
  // isSubmitting alone lags behind the redirect, so the transition drives the button state too.
  const [pending, startTransition] = useTransition();

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { isSubmitting, isValid, errors },
  } = useForm<QuestionSchemaInput, unknown, QuestionSchema>({
    resolver: zodResolver(questionSchema),
    // onTouched validates on blur, so errors appear before the user hits submit.
    mode: "onTouched",
    // Without a default, tags starts undefined and zod reports "expected array, received
    // undefined" instead of the message we wrote.
    defaultValues: { title: "", content: "", tags: [] },
  });

  useEffect(() => {
    if (questionToUpdate) {
      // The API calls them tagSlugs, the form calls them tags.
      reset({ ...questionToUpdate, tags: questionToUpdate.tagSlugs });
    }
  }, [questionToUpdate, reset]);

  const onSubmit = (data: QuestionSchema) => {
    // The callback has to resolve to void, so handleError is called as a statement here.
    startTransition(async () => {
      if (questionToUpdate) {
        const { error } = await updateQuestion(data, questionToUpdate.id);

        if (error) {
          handleError(error);
          return;
        }

        router.push(`/questions/${questionToUpdate.id}`);
      } else {
        const { data: question, error } = await postQuestion(data);

        if (error) {
          handleError(error);
          return;
        }

        if (question) router.push(`/questions/${question.id}`);
      }
    });
  };

  return (
    <Form
      onSubmit={handleSubmit(onSubmit)}
      className="flex flex-col gap-3 p-6 shadow-xl bg-white dark:bg-black"
    >
      <div className="flex flex-col gap-3 w-full">
        <div className="text-2xl font-semibold">Title</div>
        <Input
          {...register("title")}
          type="text"
          className="w-full"
          label="Be specific and imagine you are asking a question to another person"
          labelPlacement="outside-top"
          placeholder="e.g. How would you truncate text in tailwind?"
          isInvalid={!!errors.title}
          errorMessage={errors.title?.message}
        />
      </div>
      <div className="flex flex-col gap-3 w-full">
        <div className="text-2xl font-semibold">Body</div>
        {/* The editor is controlled too: tiptap owns its own state, so the form gets the HTML
            through onUpdate rather than through register. */}
        <Controller
          control={control}
          name="content"
          render={({ field: { onChange, onBlur, value }, fieldState }) => (
            <>
              <p
                className={clsx("text-sm", {
                  "text-danger": fieldState.error?.message,
                })}
              >
                Include all the information someone would need to answer your
                question
              </p>
              <RichTextEditor
                onChange={onChange}
                onBlur={onBlur}
                value={value ?? ""}
                errorMessage={fieldState.error?.message}
              />
              {fieldState.error?.message && (
                <span className="text-xs text-danger -mt-1">
                  {fieldState.error.message}
                </span>
              )}
            </>
          )}
        />
      </div>
      <div className="flex flex-col gap-3 w-full">
        <div className="text-2xl font-semibold">Tags</div>
        <p className="text-sm">
          Add up to five tags to describe what your question is about
        </p>
        {/* An uncontrolled select hands back comma separated values, which cannot be validated
            as an array - hence the Controller. */}
        <Controller
          control={control}
          name="tags"
          render={({ field, fieldState }) => (
            <Select
              className="w-full"
              label="Select 1 to 5 tags"
              selectionMode="multiple"
              isClearable
              disallowEmptySelection
              items={tags}
              selectedKeys={field.value ?? []}
              onSelectionChange={(keys) => field.onChange(Array.from(keys))}
              onBlur={field.onBlur}
              isInvalid={fieldState.invalid}
              errorMessage={fieldState.error?.message}
            >
              {/* key is the slug, not the id: the API validates tags by slug. */}
              {(tag) => <SelectItem key={tag.slug}>{tag.name}</SelectItem>}
            </Select>
          )}
        />
      </div>
      <Button
        type="submit"
        color="primary"
        className="w-fit"
        isLoading={isSubmitting || pending}
        isDisabled={!isValid || pending}
      >
        {questionToUpdate ? "update" : "post"} your question
      </Button>
    </Form>
  );
}
