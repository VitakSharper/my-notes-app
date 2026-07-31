import { errorToast } from "@/lib/util";
import {
  BoldIcon,
  CodeBracketIcon,
  ItalicIcon,
  PhotoIcon,
  StrikethroughIcon,
} from "@heroicons/react/20/solid";
import { Button } from "@heroui/button";
import { Editor, useEditorState } from "@tiptap/react";
import { ChangeEvent, useRef, useState } from "react";

type Props = {
  editor: Editor | null;
};

export default function MenuBar({ editor }: Props) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  // useEditorState is what makes React track the marks under the cursor, so the buttons can
  // show as pressed.
  const editorState = useEditorState({
    editor,
    selector: ({ editor }) => {
      if (!editor) return null;

      return {
        isBold: editor.isActive("bold"),
        isItalic: editor.isActive("italic"),
        isStrike: editor.isActive("strike"),
        isCodeBlock: editor.isActive("codeBlock"),
      };
    },
  });

  if (!editor) return null;

  // useEditorState builds its snapshot from the editor it saw on the first render - null here,
  // because immediatelyRender is false - and only refreshes it on the first transaction. Bailing
  // out on a null editorState would hide the whole toolbar until the user typed a character, so
  // nothing is active yet and false is the honest default.
  const state = editorState ?? {
    isBold: false,
    isItalic: false,
    isStrike: false,
    isCodeBlock: false,
  };

  const options = [
    {
      icon: <BoldIcon className="w-5 h-5" />,
      onClick: () => editor.chain().focus().toggleBold().run(),
      pressed: state.isBold,
    },
    {
      icon: <ItalicIcon className="w-5 h-5" />,
      onClick: () => editor.chain().focus().toggleItalic().run(),
      pressed: state.isItalic,
    },
    {
      icon: <StrikethroughIcon className="w-5 h-5" />,
      onClick: () => editor.chain().focus().toggleStrike().run(),
      pressed: state.isStrike,
    },
    {
      icon: <CodeBracketIcon className="w-5 h-5" />,
      onClick: () => editor.chain().focus().toggleCodeBlock().run(),
      pressed: state.isCodeBlock,
    },
  ];

  const onFileSelected = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];

    // Reset straight away so picking the same file twice still fires a change event.
    event.target.value = "";

    if (!file) return;

    setUploading(true);

    const formData = new FormData();
    formData.append("file", file);

    try {
      const response = await fetch("/api/images", {
        method: "POST",
        body: formData,
      });

      const body = await response.json();

      if (!response.ok) {
        errorToast({
          message: body.message ?? "Upload failed",
          status: response.status,
        });
        return;
      }

      editor.chain().focus().setImage({ src: body.url, alt: file.name }).run();
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="rounded space-x-1 pb-1 z-50">
      {options.map((option, index) => (
        <Button
          key={index}
          // Without an explicit type this would submit the form it lives in.
          type="button"
          radius="sm"
          size="sm"
          isIconOnly
          color={option.pressed ? "primary" : "default"}
          onPress={option.onClick}
        >
          {option.icon}
        </Button>
      ))}
      {/* The image extension only renders <img>; the file has to reach storage some other way,
          which is what this button and /api/images do. */}
      <Button
        type="button"
        radius="sm"
        size="sm"
        isIconOnly
        isLoading={uploading}
        onPress={() => fileInputRef.current?.click()}
      >
        <PhotoIcon className="w-5 h-5" />
      </Button>
      <input
        ref={fileInputRef}
        type="file"
        accept="image/*"
        className="hidden"
        onChange={onFileSelected}
      />
    </div>
  );
}
