"use client";

import RichTextEditor from "@/components/editor/rich-text-editor";
import { postAnswer } from "@/lib/actions/question-actions";
import {
  AnswerSchema,
  AnswerSchemaInput,
  answerSchema,
} from "@/lib/schemas/answer-schema";
import { handleError } from "@/lib/util";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@heroui/button";
import { useTransition } from "react";
import { Controller, useForm } from "react-hook-form";

type Props = {
  questionId: string;
};

export default function AnswerForm({ questionId }: Props) {
  const [pending, startTransition] = useTransition();

  const {
    control,
    handleSubmit,
    reset,
    formState: { isValid },
  } = useForm<AnswerSchemaInput, unknown, AnswerSchema>({
    resolver: zodResolver(answerSchema),
    mode: "onTouched",
    defaultValues: { content: "" },
  });

  const onSubmit = (data: AnswerSchema) => {
    startTransition(async () => {
      const { error } = await postAnswer(data, questionId);

      if (error) {
        handleError(error);
        return;
      }

      // No redirect here: the user stays on the question, so the form is emptied instead.
      reset({ content: "" });
    });
  };

  return (
    <div className="flex flex-col gap-3 items-start my-4 w-full p-6">
      <h3 className="text-2xl">Your answer</h3>
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="w-full flex flex-col gap-3"
      >
        <Controller
          control={control}
          name="content"
          render={({ field: { onChange, onBlur, value }, fieldState }) => (
            <>
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
        <Button
          type="submit"
          color="primary"
          className="w-fit"
          isLoading={pending}
          isDisabled={!isValid || pending}
        >
          post your answer
        </Button>
      </form>
    </div>
  );
}
