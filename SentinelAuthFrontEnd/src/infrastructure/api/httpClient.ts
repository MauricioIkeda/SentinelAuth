import axios, { AxiosError, type AxiosInstance, type AxiosRequestConfig } from 'axios'

type ProblemDetails = {
  title?: string
  detail?: string
  message?: string
}

function readApiError(error: AxiosError<ProblemDetails>) {
  const fallback = 'Nao foi possivel concluir a solicitacao.'
  const problem = error.response?.data

  return problem?.detail || problem?.message || problem?.title || fallback
}

export function isAuthorizationError(error: unknown) {
  return (
    error instanceof Error &&
    (error.message.startsWith('401:') || error.message.startsWith('403:'))
  )
}

export class HttpClient {
  private readonly client: AxiosInstance

  constructor(baseUrl: string) {
    this.client = axios.create({
      baseURL: baseUrl,
      headers: {
        'Content-Type': 'application/json',
      },
    })
  }

  async request<T>(path: string, init?: RequestInit) {
    try {
      const config: AxiosRequestConfig = {
        url: path,
        method: init?.method,
        data: init?.body,
        headers: init?.headers as AxiosRequestConfig['headers'],
      }
      const response = await this.client.request<T>(config)

      if (response.status === 204) {
        return undefined as T
      }

      return response.data
    } catch (error) {
      if (axios.isAxiosError<ProblemDetails>(error) && error.response) {
        throw new Error(`${error.response.status}: ${readApiError(error)}`, {
          cause: error,
        })
      }

      throw error
    }
  }
}
