import { get } from './client'
import type { MultiPackComparison } from '../types/veritas'

export const comparePacks = (corpusId: string, packIds: string[]) =>
  get<MultiPackComparison[]>(`/corpora/${corpusId}/compare?packs=${packIds.join(',')}`)

export const comparePacksCsv = (corpusId: string, packIds: string[]) =>
  fetch(`/api/corpora/${corpusId}/compare?packs=${packIds.join(',')}`, {
    headers: { Accept: 'text/csv' },
  }).then((r) => r.blob())
