"use client";

import { Avatar } from "@heroui/avatar";
import {
  Dropdown,
  DropdownItem,
  DropdownMenu,
  DropdownTrigger,
} from "@heroui/dropdown";
import type { User } from "next-auth";
import { signOut } from "next-auth/react";

type Props = {
  user: User;
};

export default function UserMenu({ user }: Props) {
  return (
    <Dropdown>
      <DropdownTrigger>
        <div className="flex items-center gap-2 cursor-pointer">
          <Avatar
            color="secondary"
            size="sm"
            name={user.name?.charAt(0) ?? "?"}
          />
          {user.name}
        </div>
      </DropdownTrigger>
      <DropdownMenu aria-label="user menu">
        <DropdownItem key="edit">Edit profile</DropdownItem>
        <DropdownItem
          key="logout"
          className="text-danger"
          color="danger"
          // onPress rather than the course's onClick: HeroUI menu items are press-based, and
          // signOut has to come from next-auth/react so the browser follows the redirect.
          onPress={() => signOut({ redirectTo: "/" })}
        >
          Sign out
        </DropdownItem>
      </DropdownMenu>
    </Dropdown>
  );
}
