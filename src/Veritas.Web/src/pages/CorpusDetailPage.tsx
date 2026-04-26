import { useEffect, useRef, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { getCorpus } from '../api/corpora'
import { listDocuments, uploadDocument, deleteDocument } from '../api/documents'
import type { CorpusResponse, DocumentResponse } from '../types/veritas'
import StatusBadge from '../components/StatusBadge'
import ConfirmDialog from '../components/ConfirmDialog'

const RIGHTS_OPTIONS = [
  'own_content',
  'permission_granted',
  'licensed_for_private_use',
  'public_domain',
  'open_access',
  'unknown_needs_review',
]

export default function CorpusDetailPage() {
  const { corpusId } = useParams<{ corpusId: string }>()
  const [corpus, setCorpus] = useState<CorpusResponse>()
  const [docs, setDocs] = useState<DocumentResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [showUpload, setShowUpload] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<DocumentResponse>()
  const [uploading, setUploading] = useState(false)
  const [rights, setRights] = useState('own_content')
  const [title, setTitle] = useState('')
  const fileRef = useRef<HTMLInputElement>(null)

  const load = () => {
    if (!corpusId) return
    Promise.all([getCorpus(corpusId), listDocuments(corpusId)])
      .then(([c, d]) => { setCorpus(c); setDocs(d) })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [corpusId])

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault()
    const file = fileRef.current?.files?.[0]
    if (!file || !corpusId) return
    setUploading(true)
    try {
      await uploadDocument(corpusId, file, rights, title || undefined)
      setShowUpload(false)
      setTitle('')
      load()
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setUploading(false)
    }
  }

  const handleDelete = async () => {
    if (!deleteTarget || !corpusId) return
    try {
      await deleteDocument(corpusId, deleteTarget.document_id)
      setDeleteTarget(undefined)
      load()
    } catch (e: unknown) {
      setError((e as Error).message)
    }
  }

  if (loading) return <p className="text-gray-500 text-sm">Loading…</p>

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">{corpus?.name}</h1>
          {corpus?.description && <p className="text-sm text-gray-500 mt-1">{corpus.description}</p>}
        </div>
        <button
          onClick={() => setShowUpload(true)}
          className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 text-sm"
        >
          + Upload document
        </button>
      </div>

      <div className="flex gap-4 text-sm text-gray-500 mb-6">
        <span>Index: <StatusBadge status={corpus?.index_status ?? 'pending'} /></span>
        <span>{docs.length} document{docs.length !== 1 ? 's' : ''}</span>
      </div>

      {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>}

      {docs.length === 0 ? (
        <p className="text-gray-500 text-sm">No documents yet.</p>
      ) : (
        <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">File</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Title</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Status</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Uploaded</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {docs.map((d) => (
                <tr key={d.document_id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <Link
                      to={`/corpora/${corpusId}/documents/${d.document_id}`}
                      className="text-indigo-600 hover:underline"
                    >
                      {d.original_filename}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-gray-500 truncate max-w-xs">{d.extracted_title ?? d.title_override ?? '—'}</td>
                  <td className="px-4 py-3"><StatusBadge status={d.status} /></td>
                  <td className="px-4 py-3 text-gray-400">{new Date(d.uploaded_at).toLocaleDateString()}</td>
                  <td className="px-4 py-3 text-right">
                    <button
                      onClick={() => setDeleteTarget(d)}
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

      {showUpload && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <form onSubmit={handleUpload} className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md mx-4">
            <h2 className="text-lg font-semibold mb-4">Upload document</h2>
            <label className="block mb-3">
              <span className="text-sm font-medium text-gray-700">File * (.pdf .docx .md .txt .html)</span>
              <input ref={fileRef} type="file" required accept=".pdf,.docx,.md,.txt,.html"
                className="mt-1 block w-full text-sm text-gray-600" />
            </label>
            <label className="block mb-3">
              <span className="text-sm font-medium text-gray-700">Rights declaration *</span>
              <select value={rights} onChange={(e) => setRights(e.target.value)}
                className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm">
                {RIGHTS_OPTIONS.map((r) => <option key={r}>{r}</option>)}
              </select>
            </label>
            <label className="block mb-5">
              <span className="text-sm font-medium text-gray-700">Title override (optional)</span>
              <input value={title} onChange={(e) => setTitle(e.target.value)}
                className="mt-1 block w-full border border-gray-300 rounded px-3 py-2 text-sm" />
            </label>
            <div className="flex gap-3 justify-end">
              <button type="button" onClick={() => setShowUpload(false)} className="px-4 py-2 text-sm border rounded">Cancel</button>
              <button type="submit" disabled={uploading}
                className="px-4 py-2 text-sm bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50">
                {uploading ? 'Uploading…' : 'Upload'}
              </button>
            </div>
          </form>
        </div>
      )}

      {deleteTarget && (
        <ConfirmDialog
          title="Delete document"
          message={`Delete "${deleteTarget.original_filename}"? This is irreversible.`}
          onConfirm={handleDelete}
          onCancel={() => setDeleteTarget(undefined)}
        />
      )}
    </div>
  )
}
