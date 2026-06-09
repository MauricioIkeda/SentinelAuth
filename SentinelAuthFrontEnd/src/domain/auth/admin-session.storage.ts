export interface AdminSessionStorage {
  get(): string
  set(token: string): void
  clear(): void
}
