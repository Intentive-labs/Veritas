import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { comparePacks, comparePacksCsv } from '../api/compare'
import type { MultiPackComparison } from '../types/veritas'

export default function PackComparePage() {
  const { corpusId } = useParams<{ corpusId: string }>()
  const [packInput, setPackInput] = useState('')
  const [loading, setLoading] = useState(false)
  const [results, setResults] = useState<MultiPackComparison[]>()
  const [error, setError] = useState<string>()

  const packIds = packInput.split(',').map((s) => s.trim()).filter(Boolean)

  const handleCompare = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!corpusId || packIds.length < 2) return
    setLoading(true); setError(undefined); setResults(undefined)
    try {
      setResults(await comparePacks(corpusId, packIds))
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }

  const handleCsvExport = async () => {
    if (!corpusId || packIds.length < 2) return
    const blob = await comparePacksCsv(corpusId, packIds)
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = `compare-${corpusId}.csv`; a.click()
    URL.revokeObjectURL(url)
  }

  const agreementCount = results?.filter((r) => r.agreement).length ?? 0
  const disagreementCount = (results?.length ?? 0) - agreementCount

  return (
    <div className="max-w-5xl">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Multi-pack comparison</h1>

      <form onSubmit={handleCompare} className="mb-6 flex gap-3 items-end">
        <div className="flex-1">
          <label className="block text-xs font-medium text-gray-700 mb-1">
            Pack IDs (comma-separated, ≥ 2)
          </label>
          <input
            value={packInput}
            onChange={(e) => setPackInput(e.target.value)}
            placeholder="lenr-magnetic-field-v1, lenr-magnetic-field-v2"
            className="w-full border border-gray-300 rounded px-3 py-2 text-sm"
            required
          />
        </div>
        <button
          type="submit"
          disabled={loading || packIds.length < 2}
          className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50 whitespace-nowrap"
        >
          {loading ? 'Comparing…' : 'Compare'}
        </button>
        {results && (
          <button
            type="button"
            onClick={handleCsvExport}
            className="px-4 py-2 text-sm border border-gray-300 rounded hover:bg-gray-50 whitespace-nowrap"
          >
            Export CSV
          </button>
        )}
      </form>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>
      )}

      {results && (
        <>
          <div className="grid grid-cols-3 gap-4 mb-6">
            <StatCard label="Documents compared" value={results.length} />
            <StatCard label="Agreement" value={agreementCount} colour="green" />
            <StatCard label="Disagreement" value={disagreementCount} colour="red" />
          </div>

          <div className="overflow-x-auto rounded-lg border border-gray-200">
            <table className="min-w-full divide-y divide-gray-200 text-sm">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-500">Document</th>
                  {packIds.map((p) => (
                    <th key={p} className="px-4 py-3 text-left font-medium text-gray-500">{p}</th>
                  ))}
                  <th className="px-4 py-3 text-left font-medium text-gray-500">Agreement</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500">Diffs</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-100">
                {results.map((row) => (
                  <tr key={row.document_id} className={row.agreement ? '' : 'bg-red-50'}>
                    <td className="px-4 py-3 font-mono text-xs text-gray-600">
                      {row.document_id.slice(0, 16)}…
                    </td>
                    {packIds.map((p) => {
                      const cls = row.results.find((r) => r.pack_id === p)
                      return (
                        <td key={p} className="px-4 py-3">
                          {cls ? (
                            <span className="flex items-center gap-1">
                              <OutcomeBadge outcome={cls.outcome} />
                              <span className="text-xs text-gray-400">
                                {(cls.outcome_confidence * 100).toFixed(0)}%
                              </span>
                            </span>
                          ) : (
                            <span className="text-gray-300 text-xs">—</span>
                          )}
                        </td>
                      )
                    })}
                    <td className="px-4 py-3">
                      {row.agreement ? (
                        <span className="text-green-600 text-xs font-medium">✓ agree</span>
                      ) : (
                        <span className="text-red-600 text-xs font-medium">✗ differ</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-xs text-gray-500">
                      {row.parameter_diffs.join('; ') || '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <p className="text-xs text-gray-400 mt-4 italic">
            All findings are correlational. Specific to corpus [{corpusId}] and the selected pack versions.
          </p>
        </>
      )}
    </div>
  )
}

function StatCard({ label, value, colour }: { label: string; value: number; colour?: 'green' | 'red' }) {
  const text = colour === 'green' ? 'text-green-700' : colour === 'red' ? 'text-red-700' : 'text-gray-800'
  return (
    <div className="bg-white border border-gray-200 rounded-lg p-4">
      <p className="text-xs text-gray-500 mb-1">{label}</p>
      <p className={`text-2xl font-bold ${text}`}>{value}</p>
    </div>
  )
}

function OutcomeBadge({ outcome }: { outcome: string }) {
  const colours: Record<string, string> = {
    positive: 'bg-green-100 text-green-800',
    negative: 'bg-red-100 text-red-800',
    inconclusive: 'bg-yellow-100 text-yellow-800',
  }
  return (
    <span className={`inline-flex px-2 py-0.5 rounded text-xs font-medium ${colours[outcome] ?? 'bg-gray-100 text-gray-700'}`}>
      {outcome}
    </span>
  )
}
