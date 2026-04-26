import { post } from './client'
import type { RagResponse } from '../types/veritas'

export interface RagQueryRequest {
  query: string
  filters?: Record<string, string>
  top_k?: number
}

export const ragQuery = (corpusId: string, req: RagQueryRequest) =>
  post<RagResponse>(`/corpora/${corpusId}/rag/query`, req)

export const ragStatus = (corpusId: string) =>
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  fetch(`/api/corpora/${corpusId}/rag/status`).then((r) => r.json()) as Promise<any>
