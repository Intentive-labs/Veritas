import { get, post } from './client'
import type {
  ExperimentResponse,
  CreateExperimentRequest,
  SimilarDocumentsResponse,
} from '../types/veritas'

export const listExperiments = (corpusId: string) =>
  get<ExperimentResponse[]>(`/corpora/${corpusId}/experiments`)

export const getExperiment = (corpusId: string, expId: string) =>
  get<ExperimentResponse>(`/corpora/${corpusId}/experiments/${expId}`)

export const createExperiment = (corpusId: string, req: CreateExperimentRequest) =>
  post<ExperimentResponse>(`/corpora/${corpusId}/experiments`, req)

export const getSimilar = (corpusId: string, expId: string, n = 5) =>
  get<SimilarDocumentsResponse>(`/corpora/${corpusId}/experiments/${expId}/similar?n=${n}`)
