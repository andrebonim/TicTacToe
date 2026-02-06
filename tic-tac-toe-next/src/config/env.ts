export const config = {
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL ?? 'https://localhost:44339',
  signalrHubPath: process.env.NEXT_PUBLIC_SIGNALR_HUB ?? '/gameHub',
} as const;