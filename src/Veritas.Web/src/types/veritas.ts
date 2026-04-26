// TypeScript types mirroring C# contract records in Veritas.Core.Contracts

export interface ClassificationResult {
  document_id: string
  pack_id: string
  pack_version: string
  outcome: string
  outcome_confidence: number
  supporting_evidence: string[]
}

export interface MultiPackComparison {
  corpus_id: string
  document_id: string
  results: ClassificationResult[]
  agreement: boolean
  parameter_diffs: string[]
}

export interface RagSource {
  document_id: string
  title: string
  chunk_id: string
  excerpt: string
}

export interface RagResponse {
  answer: string
  confidence: string
  sources: RagSource[]
  disclaimer: string
  is_refused: boolean
  refusal_reason?: string
}

export interface CorpusResponse {
  corpus_id: string
  name: string
  owner: string
  visibility: string
  source_type: string
  description?: string
  created_at: string
  updated_at: string
  document_count: number
  index_status: string
}

export interface CreateCorpusRequest {
  name: string
  source_type: string
  description?: string
  visibility?: string
}

export interface DocumentResponse {
  document_id: string
  corpus_id: string
  original_filename: string
  format: string
  file_size_bytes: number
  sha256_hash: string
  rights_declaration: string
  uploaded_at: string
  uploaded_by: string
  title_override?: string
  status: string
  extracted_title?: string
  processing_history: string[]
}

export interface ExperimentResponse {
  experiment_id: string
  corpus_id: string
  pack_id: string
  pack_version: string
  hypothesis_version: string
  submitted_at: string
  submitted_by: string
  parameters: Record<string, unknown>
  notes?: string
}

export interface CreateExperimentRequest {
  pack_id: string
  pack_version: string
  hypothesis_version: string
  parameters: Record<string, unknown>
  notes?: string
}

export interface SimilarDocument {
  document_id: string
  title: string
  similarity_score: number
}

export interface SimilarDocumentsResponse {
  experiment_id: string
  similar: SimilarDocument[]
}

export interface ValidationQueueItem {
  documentId: string
  originalFilename: string
  status: string
  extractedTitle?: string
  uploadedAt: string
}

export interface HypothesisTestRequest {
  hypothesis_id: string
  pack_id: string
  pack_version: string
}

export interface HypothesisTestResponse {
  hypothesis_id: string
  pack_id: string
  pack_version: string
  coverage: { relevant: number; processed: number; coverage_ratio: number }
  findings: { supporting: number; contradicting: number; inconclusive: number }
  confidence: string
  publication_bias_flag?: string
  disclaimer: string
}

export interface MultiPackComparisonResponse {
  corpus_id: string
  document_id: string
  /** @deprecated use MultiPackComparison instead */
  classifications: { document_id: string; pack_id: string; pack_version: string; outcome: string; confidence: number; evidence: string[] }[]
  agreement: boolean
  parameter_diffs: string[]
}
