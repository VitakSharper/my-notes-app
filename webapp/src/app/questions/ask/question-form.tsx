"use client";

import { useTagStore } from "@/lib/hooks/use-tag-store";
import {
  QuestionSchema,
  questionSchema,
} from "@/lib/schemas/question-schema";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@heroui/button";
import { Form } from "@heroui/form";
import { Input, Textarea } from "@heroui/input";
import { Select, SelectItem } from "@heroui/select";
import { Controller, useForm } from "react-hook-form";

export default function QuestionForm() {
  const tags = useTagStore((state) => state.tags);

  const {
    register,
    handleSubmit,
    control,
    formState: { isSubmitting, isValid, errors },
  } = useForm<QuestionSchema>({
    resolver: zodResolver(questionSchema),
    // onTouched validates on blur, so errors appear before the user hits submit.
    mode: "onTouched",
    // Without a default, tags starts undefined and zod reports "expected array, received
    // undefined" instead of the message we wrote.
    defaultValues: { title: "", content: "", tags: [] },
  });

  const onSubmit = (data: QuestionSchema) => {
    console.log(data);
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
        {/* Replaced by a rich text editor further into the section. */}
        <Textarea
          {...register("content")}
          className="w-full"
          label="Include all the information someone would need to answer your question"
          labelPlacement="outside-top"
          minRows={12}
          isInvalid={!!errors.content}
          errorMessage={errors.content?.message}
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
        isLoading={isSubmitting}
        isDisabled={!isValid}
      >
        post your question
      </Button>
    </Form>
  );
}
