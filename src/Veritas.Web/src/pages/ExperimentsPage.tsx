import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { listExperiments, createExperiment, getSimilar } from '../api/experiments'
import type { ExperimentResponse, CreateExperimentRequest, SimilarDocument } from '../types/veritas'

export default function ExperimentsPage() {
  const { corpusId } = useParams<{ corpusId: string }>()
  const [experiments, setExperiments] = useState<ExperimentResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [showForm, setShowForm] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [selectedExp, setSelectedExp] = useState<ExperimentResponse>()
  const [similar, setSimilar] = useState<SimilarDocument[]>([])
  const [form, setForm] = useState<CreateExperimentRequest>({
    pack_id: '',
    pack_version: '0.1.0',
    hypothesis_version: 'hypothesis-v1',
    parameters: {},
    notes: '',
  })
  const [paramsJson, setParamsJson] = useState('{}')
  const [paramsError, setParamsError] = useState<string>()

  const load = () => {
    if (!corpusId) return
    listExperiments(corpusId)
      .then(setExperiments)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [corpusId])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setParamsError(undefined)
    let params: Record<string, unknown> = {}
    try { params = JSON.parse(paramsJson) } catch { setParamsError('Parameters must be valid JSON'); return }
    if (!corpusId) return
    setSubmitting(true)
    try {
      await createExperiment(corpusId, { ...form, parameters: params })
      setShowForm(false)
      load()
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setSubmitting(false)
    }
  }

  const handleViewSimilar = async (exp: ExperimentResponse) => {
    setSelectedExp(exp)
    if (!corpusId) return
    const res = await getSimilar(corpusId, exp.experiment_id)
    setSimilar(res.similar)
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Experiments</h1>
        <button onClick={() => setShowForm(true)}
          className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 text-sm">
          + Submit experiment
        </button>
      </div>

      {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>}

      {loading ? (
        <p className="text-gray-500 text-sm">Loading…</p>
      ) : experiments.length === 0 ? (
        <p className="text-gray-500 text-sm">No experiments yet.</p>
      ) : (
        <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">ID</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Pack</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Hypothesis</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Submitted</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {experiments.map((exp) => (
                <tr key={exp.experiment_id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-mono text-xs text-gray-400">{exp.experiment_id.slice(0, 8)}…</td>
                  <td className="px-4 py-3 text-gray-700">{exp.pack_id} <span className="text-gray-400">v{exp.pack_version}</span></td>
                  <td className="px-4 py-3 text-gray-500">{exp.hypothesis_version}</td>
                  <td className="px-4 py-3 text-gray-400">{new Date(exp.submitted_at).toLocaleDateString()}</td>
                  <td className="px-4 py-3 text-right">
                    <button onClick={() => handleViewSimilar(exp)}
                      className="text-indigo-600 hover:underline text-xs">
                      Similar docs
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selectedExp && (
        <div className="mt-6">
          <h2 className="text-base font-medium text-gray-800 mb-3">
            Similar documents for <span className="font-mono text-sm">{selectedExp.experiment_id.slice(0, 8)}…</span>
          </h2>
          {similar.length === 0 ? (
            <p className="text-gray-500 text-sm">No similar documents.</p>
          ) : (
            <ul className="space-y-2">
              {similar.map((s) => (
                <li key={s.document_id} className="flex items-center gap-4 bg-white border border-gray-200 rounded px-4 py-3 text-sm">
                  <span className="text-gray-900">{s.title}</span>
                  <span className="ml-auto text-gray-400">score: {s.similarity_score.toFixed(2)}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {showForm && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-xl p-6 w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
            <h2 className="text-lg font-semibold mb-4">Submit experiment</h2>
            {[
              { label: 'Pack ID *', key: 'pack_id', required: true },
              { label: 'Pack version', key: 'pack_version' },
              { label: 'Hypothesis version', key: 'hypothesis_version' },
              { label: 'Notes', key: 'notes' },
            ].map(({ label, key, required }) => (
              <label key={key} className="block mb-3">
                <span className="text-sm font-medium text-gray-700">{label}</span>
                <input required={required} value={(form as unknown as Record<string,string>)[key] ?? ''}
                  onChange={(e) => setForm({ ...form, [key as keyof CreateExperimentRequest]: e.target.value } as CreateExperimentRequest)}
                  className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm" />
              </label>
            ))}
            <label className="block mb-5">
              <span className="text-sm font-medium text-gray-700">Parameters (JSON)</span>
              <textarea rows={5} value={paramsJson} onChange={(e) => setParamsJson(e.target.value)}
                className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm font-mono" />
              {paramsError && <p className="text-red-600 text-xs mt-1">{paramsError}</p>}
            </label>
            <div className="flex gap-3 justify-end">
              <button type="button" onClick={() => setShowForm(false)} className="px-4 py-2 text-sm border rounded">Cancel</button>
              <button type="submit" disabled={submitting}
                className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50">
                {submitting ? 'Submitting…' : 'Submit'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  )
}
