import MenuBar from "@/components/editor/menu-bar";
import StarterKit from "@tiptap/starter-kit";
import { EditorContent, useEditor } from "@tiptap/react";
import clsx from "clsx";
import { useEffect } from "react";

type Props = {
  onChange: (body: string) => void;
  onBlur: () => void;
  value: string;
  errorMessage?: string;
};

export default function RichTextEditor({
  onChange,
  onBlur,
  value,
  errorMessage,
}: Props) {
  const editor = useEditor({
    extensions: [StarterKit],
    content: value,
    // Required in the app router: rendering on the first pass causes a hydration mismatch.
    immediatelyRender: false,
    editorProps: {
      attributes: {
        // The background is picked with a ternary rather than an extra class, so there is no
        // bg-default-100 / bg-red-50 conflict left for CSS order to settle.
        class: clsx(
          "w-full p-3 rounded-xl min-h-60 prose dark:prose-invert max-w-none dark:prose-pre:bg-primary-100",
          errorMessage ? "bg-red-50 dark:bg-red-900/30" : "bg-default-100",
        ),
      },
    },
    onBlur: () => onBlur(),
    onUpdate: ({ editor }) => onChange(editor.getHTML()),
  });

  // `content` is only read when the editor is created, so an edit form - which resets its values
  // in an effect, after the editor already exists - would show an empty body. Comparing with the
  // current HTML keeps this a no-op while typing, and emitUpdate: false avoids feeding the value
  // straight back into the form.
  useEffect(() => {
    if (editor && value && editor.getHTML() !== value) {
      editor.commands.setContent(value, { emitUpdate: false });
    }
  }, [editor, value]);

  return (
    <div>
      <MenuBar editor={editor} />
      <EditorContent editor={editor} />
    </div>
  );
}
