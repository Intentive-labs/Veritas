import { get, post, del } from './client'
import type { CorpusResponse, CreateCorpusRequest } from '../types/veritas'

export const listCorpora = () => get<CorpusResponse[]>('/corpora')
export const getCorpus = (id: string) => get<CorpusResponse>(`/corpora/${id}`)
export const createCorpus = (req: CreateCorpusRequest) => post<CorpusResponse>('/corpora', req)
export const deleteCorpus = (id: string) => del(`/corpora/${id}?confirm=true`)
