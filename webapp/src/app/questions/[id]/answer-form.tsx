"use client";

import RichTextEditor from "@/components/editor/rich-text-editor";
import { postAnswer, updateAnswer } from "@/lib/actions/question-actions";
import { useAnswerStore } from "@/lib/hooks/use-answer-store";
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
  // Filled by the edit button of an answer footer, through the store: the two are siblings.
  const editableAnswer = useAnswerStore((state) => state.answer);
  const clearAnswer = useAnswerStore((state) => state.clearAnswer);

  const {
    control,
    handleSubmit,
    reset,
    formState: { isValid },
  } = useForm<AnswerSchemaInput, unknown, AnswerSchema>({
    resolver: zodResolver(answerSchema),
    mode: "onTouched",
    defaultValues: { content: "" },
    // values, not an effect: the form follows the selected answer, so picking one fills it and
    // clearing it empties it again.
    values: { content: editableAnswer?.content ?? "" },
  });

  const onSubmit = (data: AnswerSchema) => {
    startTransition(async () => {
      const { error } = editableAnswer
        ? await updateAnswer(data, editableAnswer.questionId, editableAnswer.id)
        : await postAnswer(data, questionId);

      if (error) {
        handleError(error);
        return;
      }

      // No redirect here: the user stays on the question, so the form is emptied instead -
      // clearing the selection is what empties it when an answer was being edited.
      clearAnswer();
      reset({ content: "" });
    });
  };

  return (
    /* The id is what the edit button of an answer footer scrolls to. */
    <div
      id="answer-form"
      className="flex flex-col gap-3 items-start my-4 w-full p-6"
    >
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
        <div className="flex items-start gap-3">
          <Button
            type="submit"
            color="primary"
            className="w-fit"
            isLoading={pending}
            isDisabled={!isValid || pending}
          >
            {editableAnswer ? "update" : "post"} your answer
          </Button>
          <Button
            type="button"
            className="w-fit"
            isDisabled={!editableAnswer || pending}
            onPress={() => {
              clearAnswer();
              reset({ content: "" });
            }}
          >
            cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
