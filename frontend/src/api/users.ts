import api from "./api";

export type User = {
  id: number;
  username: string;
};

export async function fetchUsers(): Promise<User[]> {
  const res = await api.get<User[]>("/user");
  return res.data;
}

export async function resolveUserIdByUsername(
  username: string
): Promise<number | null> {
  const users = await fetchUsers();
  const match = users.find(
    (u) => u.username.toLowerCase() === username.trim().toLowerCase()
  );
  return match?.id ?? null;
}

export function toUserMap(users: User[]): Record<number, string> {
  return users.reduce<Record<number, string>>((acc, u) => {
    acc[u.id] = u.username;
    return acc;
  }, {});
}
