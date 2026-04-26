import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { ragQuery } from '../api/rag'
import type { RagResponse, RagSource } from '../types/veritas'

export default function SearchPage() {
  const { corpusId } = useParams<{ corpusId: string }>()
  const [query, setQuery] = useState('')
  const [topK, setTopK] = useState(10)
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<RagResponse>()
  const [error, setError] = useState<string>()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!corpusId || !query.trim()) return
    setLoading(true)
    setError(undefined)
    setResult(undefined)
    try {
      setResult(await ragQuery(corpusId, { query, top_k: topK }))
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="max-w-3xl">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Search corpus</h1>

      <form onSubmit={handleSubmit} className="mb-6 space-y-3">
        <div className="flex gap-3">
          <input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Ask a question about this corpus…"
            className="flex-1 border border-gray-300 rounded px-3 py-2 text-sm"
            required
          />
          <select
            value={topK}
            onChange={(e) => setTopK(Number(e.target.value))}
            className="border border-gray-300 rounded px-3 py-2 text-sm w-28"
          >
            {[5, 10, 20].map((n) => (
              <option key={n} value={n}>Top {n}</option>
            ))}
          </select>
          <button
            type="submit"
            disabled={loading}
            className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50"
          >
            {loading ? 'Searching…' : 'Search'}
          </button>
        </div>
      </form>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>
      )}

      {result && (
        <div className="space-y-5">
          {result.is_refused ? (
            <div className="p-4 bg-orange-50 border border-orange-200 rounded text-sm">
              <p className="font-medium text-orange-800 mb-1">No answer — query refused</p>
              <p className="text-orange-700">{result.refusal_reason}</p>
            </div>
          ) : (
            <div className="bg-white border border-gray-200 rounded-lg p-5">
              <div className="flex items-center gap-2 mb-3">
                <span className="text-sm font-medium text-gray-700">Answer</span>
                <ConfidenceBadge confidence={result.confidence} />
              </div>
              <p className="text-gray-900 text-sm leading-relaxed whitespace-pre-wrap">{result.answer}</p>
            </div>
          )}

          {result.sources.length > 0 && (
            <div>
              <h2 className="text-sm font-medium text-gray-700 mb-2">
                Sources ({result.sources.length})
              </h2>
              <ul className="space-y-2">
                {result.sources.map((s) => (
                  <SourceCard key={s.chunk_id} source={s} />
                ))}
              </ul>
            </div>
          )}

          <p className="text-xs text-gray-400 italic border-t border-gray-100 pt-3">
            {result.disclaimer}
          </p>
        </div>
      )}
    </div>
  )
}

function ConfidenceBadge({ confidence }: { confidence: string }) {
  const colours: Record<string, string> = {
    high: 'bg-green-100 text-green-800',
    medium: 'bg-yellow-100 text-yellow-800',
    low: 'bg-red-100 text-red-800',
  }
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${colours[confidence] ?? 'bg-gray-100 text-gray-700'}`}>
      {confidence} confidence
    </span>
  )
}

function SourceCard({ source }: { source: RagSource }) {
  return (
    <li className="bg-gray-50 border border-gray-200 rounded px-4 py-3 text-sm">
      <p className="font-medium text-gray-800 mb-1">{source.title}</p>
      <p className="text-gray-500 text-xs leading-relaxed">{source.excerpt}</p>
      <p className="text-gray-400 text-xs mt-1 font-mono">
        doc: {source.document_id.slice(0, 12)}… · chunk: {source.chunk_id}
      </p>
    </li>
  )
}
