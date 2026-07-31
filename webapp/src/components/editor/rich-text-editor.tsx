import MenuBar from "@/components/editor/menu-bar";
import Image from "@tiptap/extension-image";
import StarterKit from "@tiptap/starter-kit";
import { EditorContent, useEditor } from "@tiptap/react";
import { extractPublicIdsFromHtml } from "@/lib/util";
import clsx from "clsx";
import { useEffect, useRef } from "react";

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
  // The keys present at the previous update, to spot the ones the user just removed.
  const previousPublicIds = useRef<string[]>([]);

  const editor = useEditor({
    // StarterKit has no image support. This extension only renders <img> tags; getting a file
    // into storage and a URL back is our job (see the upload button in the menu bar).
    extensions: [StarterKit, Image],
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
    onUpdate: ({ editor }) => {
      const html = editor.getHTML();

      onChange(html);

      // An image dropped from the editor would otherwise stay in storage forever. Diffing the
      // keys on each update is cheap (getHTML is), but it is not exhaustive: an upload followed
      // by closing the tab still leaves an orphan behind.
      const currentPublicIds = extractPublicIdsFromHtml(html);
      const removed = previousPublicIds.current.filter(
        (publicId) => !currentPublicIds.includes(publicId),
      );

      removed.forEach((publicId) => {
        void fetch("/api/images", {
          method: "DELETE",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ publicId }),
        });
      });

      previousPublicIds.current = currentPublicIds;
    },
  });

  // `content` is only read when the editor is created, so an edit form - which resets its values
  // in an effect, after the editor already exists - would show an empty body. Comparing with the
  // current HTML keeps this a no-op while typing, and emitUpdate: false avoids feeding the value
  // straight back into the form.
  useEffect(() => {
    if (editor && value && editor.getHTML() !== value) {
      editor.commands.setContent(value, { emitUpdate: false });
      // Seed the baseline as well, or the first image removed from a question being edited would
      // not be seen as removed - setContent deliberately emits no update.
      previousPublicIds.current = extractPublicIdsFromHtml(value);
    }
  }, [editor, value]);

  return (
    <div>
      <MenuBar editor={editor} />
      <EditorContent editor={editor} />
    </div>
  );
}
