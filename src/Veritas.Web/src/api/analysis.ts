import { get, post } from './client'
import type { HypothesisTestRequest, HypothesisTestResponse, MultiPackComparisonResponse } from '../types/veritas'

export const testHypothesis = (corpusId: string, req: HypothesisTestRequest) =>
  post<HypothesisTestResponse>(`/corpora/${corpusId}/hypothesis/test`, req)

export const compareHypothesis = (corpusId: string, packIds: string[]) =>
  get<unknown>(`/corpora/${corpusId}/hypothesis/compare?packs=${packIds.join(',')}`)

export const compareAnalysis = (corpusId: string, packIds: string[]) =>
  get<MultiPackComparisonResponse[]>(`/corpora/${corpusId}/compare?packs=${packIds.join(',')}`)
