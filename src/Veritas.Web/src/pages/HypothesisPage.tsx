import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { testHypothesis, compareHypothesis } from '../api/analysis'
import type { HypothesisTestRequest, HypothesisTestResponse } from '../types/veritas'

export default function HypothesisPage() {
  const { corpusId } = useParams<{ corpusId: string }>()
  const [form, setForm] = useState<HypothesisTestRequest>({
    hypothesis_id: 'lenr-example-h1',
    pack_id: 'lenr-example',
    pack_version: '0.1.0',
  })
  const [result, setResult] = useState<HypothesisTestResponse>()
  const [testing, setTesting] = useState(false)
  const [error, setError] = useState<string>()
  const [comparePacks, setComparePacks] = useState('')
  const [compareResult, setCompareResult] = useState<unknown>()
  const [comparing, setComparing] = useState(false)

  const handleTest = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!corpusId) return
    setTesting(true)
    setError(undefined)
    try {
      setResult(await testHypothesis(corpusId, form))
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setTesting(false)
    }
  }

  const handleCompare = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!corpusId) return
    const packs = comparePacks.split(',').map((p) => p.trim()).filter(Boolean)
    if (packs.length < 2) { setError('Enter at least 2 pack IDs'); return }
    setComparing(true)
    setError(undefined)
    try {
      setCompareResult(await compareHypothesis(corpusId, packs))
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setComparing(false)
    }
  }

  return (
    <div className="max-w-2xl space-y-8">
      <h1 className="text-2xl font-semibold text-gray-900">Hypothesis testing</h1>
      {error && <div className="p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>}

      <section className="bg-white border border-gray-200 rounded-lg p-5">
        <h2 className="text-base font-medium text-gray-800 mb-4">Run hypothesis test</h2>
        <form onSubmit={handleTest} className="space-y-3">
          {[
            { label: 'Hypothesis ID', key: 'hypothesis_id' },
            { label: 'Pack ID', key: 'pack_id' },
            { label: 'Pack version', key: 'pack_version' },
          ].map(({ label, key }) => (
            <label key={key} className="block">
              <span className="text-sm font-medium text-gray-700">{label}</span>
              <input value={(form as unknown as Record<string,string>)[key]}
                onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm" />
            </label>
          ))}
          <button type="submit" disabled={testing}
            className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50">
            {testing ? 'Testing…' : 'Run test'}
          </button>
        </form>

        {result && (
          <div className="mt-5 border-t border-gray-100 pt-4 text-sm space-y-2">
            <div className="flex gap-6">
              <span className="text-gray-500">Confidence: <strong className="text-gray-900">{result.confidence}</strong></span>
              <span className="text-gray-500">Supporting: <strong className="text-gray-900">{result.findings.supporting}</strong></span>
              <span className="text-gray-500">Contradicting: <strong className="text-gray-900">{result.findings.contradicting}</strong></span>
            </div>
            <p className="text-xs text-gray-400 italic">{result.disclaimer}</p>
          </div>
        )}
      </section>

      <section className="bg-white border border-gray-200 rounded-lg p-5">
        <h2 className="text-base font-medium text-gray-800 mb-4">Compare across packs</h2>
        <form onSubmit={handleCompare} className="space-y-3">
          <label className="block">
            <span className="text-sm font-medium text-gray-700">Pack IDs (comma-separated, min 2)</span>
            <input value={comparePacks} onChange={(e) => setComparePacks(e.target.value)}
              placeholder="lenr-example, another-pack"
              className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm" />
          </label>
          <button type="submit" disabled={comparing}
            className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50">
            {comparing ? 'Comparing…' : 'Compare'}
          </button>
        </form>

        {compareResult !== undefined && (
          <pre className="mt-4 bg-gray-50 rounded p-3 text-xs overflow-auto">
            {JSON.stringify(compareResult as object, null, 2)}
          </pre>
        )}
      </section>
    </div>
  )
}
