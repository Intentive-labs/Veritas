import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { listCorpora, createCorpus, deleteCorpus } from '../api/corpora'
import type { CorpusResponse, CreateCorpusRequest } from '../types/veritas'
import StatusBadge from '../components/StatusBadge'
import ConfirmDialog from '../components/ConfirmDialog'

const SOURCE_TYPES = ['user_upload', 'api_import', 'connector']

export default function CorporaPage() {
  const navigate = useNavigate()
  const [corpora, setCorpora] = useState<CorpusResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [showCreate, setShowCreate] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<CorpusResponse>()
  const [form, setForm] = useState<CreateCorpusRequest>({ name: '', source_type: 'user_upload' })
  const [submitting, setSubmitting] = useState(false)

  const load = () => {
    setLoading(true)
    listCorpora()
      .then(setCorpora)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await createCorpus(form)
      setShowCreate(false)
      setForm({ name: '', source_type: 'user_upload' })
      load()
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setSubmitting(false)
    }
  }

  const handleDelete = async () => {
    if (!deleteTarget) return
    try {
      await deleteCorpus(deleteTarget.corpus_id)
      setDeleteTarget(undefined)
      load()
    } catch (e: unknown) {
      setError((e as Error).message)
    }
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Corpora</h1>
        <button
          onClick={() => setShowCreate(true)}
          className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 text-sm"
        >
          + New corpus
        </button>
      </div>

      {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>}

      {loading ? (
        <p className="text-gray-500 text-sm">Loading…</p>
      ) : corpora.length === 0 ? (
        <p className="text-gray-500 text-sm">No corpora yet. Create one to get started.</p>
      ) : (
        <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Name</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Source</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Docs</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Index</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Created</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {corpora.map((c) => (
                <tr
                  key={c.corpus_id}
                  className="hover:bg-gray-50 cursor-pointer"
                  onClick={() => navigate(`/corpora/${c.corpus_id}`)}
                >
                  <td className="px-4 py-3 font-medium text-gray-900">{c.name}</td>
                  <td className="px-4 py-3 text-gray-500">{c.source_type}</td>
                  <td className="px-4 py-3 text-gray-500">{c.document_count}</td>
                  <td className="px-4 py-3"><StatusBadge status={c.index_status} /></td>
                  <td className="px-4 py-3 text-gray-400">{new Date(c.created_at).toLocaleDateString()}</td>
                  <td className="px-4 py-3 text-right">
                    <button
                      onClick={(e) => { e.stopPropagation(); setDeleteTarget(c) }}
                      className="text-red-500 hover:text-red-700 text-xs"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <form onSubmit={handleCreate} className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md mx-4">
            <h2 className="text-lg font-semibold mb-4">New corpus</h2>
            <label className="block mb-3">
              <span className="text-sm font-medium text-gray-700">Name *</span>
              <input
                required
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm"
              />
            </label>
            <label className="block mb-3">
              <span className="text-sm font-medium text-gray-700">Source type</span>
              <select
                value={form.source_type}
                onChange={(e) => setForm({ ...form, source_type: e.target.value })}
                className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm"
              >
                {SOURCE_TYPES.map((t) => <option key={t}>{t}</option>)}
              </select>
            </label>
            <label className="block mb-5">
              <span className="text-sm font-medium text-gray-700">Description</span>
              <input
                value={form.description ?? ''}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm"
              />
            </label>
            <div className="flex gap-3 justify-end">
              <button type="button" onClick={() => setShowCreate(false)} className="px-4 py-2 text-sm border rounded">Cancel</button>
              <button type="submit" disabled={submitting} className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50">
                {submitting ? 'Creating…' : 'Create'}
              </button>
            </div>
          </form>
        </div>
      )}

      {deleteTarget && (
        <ConfirmDialog
          title="Delete corpus"
          message={`Delete "${deleteTarget.name}" and all its documents? This is irreversible.`}
          onConfirm={handleDelete}
          onCancel={() => setDeleteTarget(undefined)}
        />
      )}
    </div>
  )
}
