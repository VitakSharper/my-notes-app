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
            name={user.displayName?.charAt(0) ?? "?"}
          />
          {user.displayName}
        </div>
      </DropdownTrigger>
      <DropdownMenu aria-label="user menu">
        {/* Inert until now. HeroUI collection items take href themselves, so there is no `as` to
            pass here the way Chip and Button need one. */}
        <DropdownItem key="edit" href={`/profiles/${user.id}/edit`}>
          Edit profile
        </DropdownItem>
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
