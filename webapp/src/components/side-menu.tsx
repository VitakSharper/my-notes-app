"use client";

import { Listbox, ListboxItem } from "@heroui/listbox";
import {
  HomeIcon,
  QuestionMarkCircleIcon,
  TagIcon,
  UserIcon,
} from "@heroicons/react/24/solid";
import { usePathname } from "next/navigation";

export default function SideMenu() {
  const pathname = usePathname();

  const navLinks = [
    { key: "home", icon: HomeIcon, text: "home", href: "/" },
    {
      key: "questions",
      icon: QuestionMarkCircleIcon,
      text: "questions",
      href: "/questions",
    },
    { key: "tags", icon: TagIcon, text: "tags", href: "/tags" },
    { key: "session", icon: UserIcon, text: "user session", href: "/session" },
  ];

  return (
    <Listbox
      aria-label="nav links"
      variant="faded"
      items={navLinks}
      className="sticky top-20 ml-6"
    >
      {({ key, href, icon: Icon, text }) => (
        <ListboxItem
          key={key}
          href={href}
          aria-labelledby={key}
          aria-describedby={text}
          startContent={<Icon className="h-6" />}
          classNames={{
            base: pathname === href ? "text-secondary" : "",
            title: "text-lg",
          }}
        >
          {text}
        </ListboxItem>
      )}
    </Listbox>
  );
}
