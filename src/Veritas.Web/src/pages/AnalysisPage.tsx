import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { comparePacks, comparePacksCsv } from '../api/compare'
import type { MultiPackComparison } from '../types/veritas'

export default function AnalysisPage() {
  const { corpusId } = useParams<{ corpusId: string }>()
  const [packInput, setPackInput] = useState('')
  const [results, setResults] = useState<MultiPackComparison[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string>()

  const packIds = packInput.split(',').map((p) => p.trim()).filter(Boolean)

  const handleCompare = async (e: React.FormEvent) => {
    e.preventDefault()
    if (packIds.length < 2) { setError('Enter at least 2 pack IDs'); return }
    if (!corpusId) return
    setLoading(true); setError(undefined)
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

  /** FR-4.6: Print / PDF export via browser print dialog */
  const handlePdfExport = () => window.print()

  return (
    <div>
      {/* Print-only header with corpus + pack reference (FR-4.6) */}
      <div className="hidden print:block mb-6">
        <p className="text-sm font-medium">Veritas — Analysis Report</p>
        <p className="text-xs text-gray-500 mt-1">
          All findings are correlational. Specific to corpus [{corpusId}],
          packs [{packIds.join(', ')}] and their stated assumptions.
        </p>
      </div>

      <div className="print:hidden">
        <h1 className="text-2xl font-semibold text-gray-900 mb-6">Multi-pack analysis</h1>

        <form onSubmit={handleCompare} className="flex gap-3 mb-6 items-end">
          <label className="flex-1">
            <span className="text-sm font-medium text-gray-700">Pack IDs (comma-separated, min 2)</span>
            <input
              value={packInput}
              onChange={(e) => setPackInput(e.target.value)}
              placeholder="lenr-example, another-pack"
              className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm"
            />
          </label>
          <button type="submit" disabled={loading}
            className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50 h-9">
            {loading ? 'Loading…' : 'Compare'}
          </button>
          {results.length > 0 && (
            <>
              <button type="button" onClick={handleCsvExport}
                className="px-4 py-2 text-sm border border-gray-300 rounded hover:bg-gray-50 h-9">
                Export CSV
              </button>
              <button type="button" onClick={handlePdfExport}
                className="px-4 py-2 text-sm border border-gray-300 rounded hover:bg-gray-50 h-9">
                Export PDF
              </button>
            </>
          )}
        </form>

        {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>}
      </div>

      {results.length > 0 && (
        <>
          <div className="bg-white border border-gray-200 rounded-lg overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Document</th>
                  {packIds.map((p) => (
                    <th key={p} className="text-left px-4 py-3 font-medium text-gray-600">{p}</th>
                  ))}
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Agreement</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Diffs</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {results.map((row) => (
                  <tr key={row.document_id} className={row.agreement ? 'hover:bg-gray-50' : 'bg-red-50 hover:bg-red-100'}>
                    <td className="px-4 py-3 font-mono text-xs text-gray-400">{row.document_id.slice(0, 12)}…</td>
                    {packIds.map((p) => {
                      const cls = row.results.find((c) => c.pack_id === p)
                      return (
                        <td key={p} className="px-4 py-3 text-gray-700">
                          {cls ? (
                            <span>{cls.outcome} <span className="text-gray-400 text-xs">({(cls.outcome_confidence * 100).toFixed(0)}%)</span></span>
                          ) : '—'}
                        </td>
                      )
                    })}
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${
                        row.agreement ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                      }`}>
                        {row.agreement ? 'agree' : 'disagree'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-xs text-gray-500">{row.parameter_diffs.join('; ') || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* FR-4.6: Disclaimer footer on every printed page */}
          <p className="text-xs text-gray-400 mt-4 italic print:block">
            All findings are correlational. Specific to corpus [{corpusId}],
            packs [{packIds.join(', ')}] and their stated assumptions.
          </p>
        </>
      )}
    </div>
  )
}

