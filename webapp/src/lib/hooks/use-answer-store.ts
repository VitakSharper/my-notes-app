import { Answer } from "@/lib/types";
import { create } from "zustand";

type AnswerStore = {
  answer: Answer | null;
  setAnswer: (answer: Answer) => void;
  clearAnswer: () => void;
};

// The answer being edited, held outside the tree on purpose: the edit button lives in the answer
// footer and the form it fills is a sibling, with two server components (the question page and the
// answer content) in between - so there is no shared client ancestor to hold useState.
export const useAnswerStore = create<AnswerStore>((set) => ({
  answer: null,
  setAnswer: (answer) => set({ answer }),
  clearAnswer: () => set({ answer: null }),
}));
