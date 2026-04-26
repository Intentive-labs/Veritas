import { get, del, postForm, post } from './client'
import type { DocumentResponse, ValidationQueueItem } from '../types/veritas'

export const listDocuments = (corpusId: string) =>
  get<DocumentResponse[]>(`/corpora/${corpusId}/documents`)

export const getDocument = (corpusId: string, docId: string) =>
  get<DocumentResponse>(`/corpora/${corpusId}/documents/${docId}`)

export const uploadDocument = (
  corpusId: string,
  file: File,
  rightsDeclaration: string,
  title?: string,
) => {
  const form = new FormData()
  form.append('file', file)
  form.append('rights_declaration', rightsDeclaration)
  if (title) form.append('title', title)
  return postForm<DocumentResponse>(`/corpora/${corpusId}/documents`, form)
}

export const deleteDocument = (corpusId: string, docId: string) =>
  del(`/corpora/${corpusId}/documents/${docId}`)

export const reprocessDocument = (corpusId: string, docId: string) =>
  post<void>(`/corpora/${corpusId}/documents/${docId}/reprocess`)

export const getValidationQueue = (corpusId: string) =>
  get<ValidationQueueItem[]>(`/corpora/${corpusId}/validation/queue`)
