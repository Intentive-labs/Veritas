import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getDocument, deleteDocument, reprocessDocument } from '../api/documents'
import type { DocumentResponse } from '../types/veritas'
import StatusBadge from '../components/StatusBadge'
import ConfirmDialog from '../components/ConfirmDialog'

export default function DocumentDetailPage() {
  const { corpusId, documentId } = useParams<{ corpusId: string; documentId: string }>()
  const navigate = useNavigate()
  const [doc, setDoc] = useState<DocumentResponse>()
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [showDelete, setShowDelete] = useState(false)
  const [reprocessing, setReprocessing] = useState(false)

  useEffect(() => {
    if (!corpusId || !documentId) return
    getDocument(corpusId, documentId)
      .then(setDoc)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [corpusId, documentId])

  const handleReprocess = async () => {
    if (!corpusId || !documentId) return
    setReprocessing(true)
    try {
      await reprocessDocument(corpusId, documentId)
      setDoc((d) => d ? { ...d, status: 'extracting' } : d)
    } catch (e: unknown) {
      setError((e as Error).message)
    } finally {
      setReprocessing(false)
    }
  }

  const handleDelete = async () => {
    if (!corpusId || !documentId) return
    try {
      await deleteDocument(corpusId, documentId)
      navigate(`/corpora/${corpusId}`)
    } catch (e: unknown) {
      setError((e as Error).message)
    }
  }

  if (loading) return <p className="text-gray-500 text-sm">Loading…</p>
  if (!doc) return <p className="text-gray-500 text-sm">Document not found.</p>

  return (
    <div className="max-w-2xl">
      <div className="flex items-start justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-gray-900">{doc.original_filename}</h1>
          {(doc.extracted_title ?? doc.title_override) && (
            <p className="text-sm text-gray-500 mt-1">{doc.extracted_title ?? doc.title_override}</p>
          )}
        </div>
        <div className="flex gap-2">
          <button
            onClick={handleReprocess}
            disabled={reprocessing}
            className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50"
          >
            {reprocessing ? 'Queued…' : 'Reprocess'}
          </button>
          <button
            onClick={() => setShowDelete(true)}
            className="px-3 py-1.5 text-sm bg-red-600 text-white rounded hover:bg-red-700"
          >
            Delete
          </button>
        </div>
      </div>

      {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>}

      <dl className="bg-white border border-gray-200 rounded-lg divide-y divide-gray-100 text-sm">
        {[
          ['Document ID', doc.document_id],
          ['Status', <StatusBadge status={doc.status} />],
          ['Format', doc.format],
          ['Size', `${(doc.file_size_bytes / 1024).toFixed(1)} KB`],
          ['Rights', doc.rights_declaration.replace(/_/g, ' ')],
          ['SHA-256', <span className="font-mono text-xs text-gray-400">{doc.sha256_hash.slice(0, 20)}…</span>],
          ['Uploaded', new Date(doc.uploaded_at).toLocaleString()],
          ['By', doc.uploaded_by],
        ].map(([label, value]) => (
          <div key={String(label)} className="flex px-4 py-3 gap-4">
            <dt className="w-32 text-gray-500 shrink-0">{label}</dt>
            <dd className="text-gray-900">{value}</dd>
          </div>
        ))}
      </dl>

      {doc.processing_history.length > 0 && (
        <div className="mt-4">
          <h2 className="text-sm font-medium text-gray-700 mb-2">Processing history</h2>
          <ul className="text-xs text-gray-500 space-y-1 font-mono">
            {doc.processing_history.map((h, i) => <li key={i}>{h}</li>)}
          </ul>
        </div>
      )}

      {showDelete && (
        <ConfirmDialog
          title="Delete document"
          message={`Delete "${doc.original_filename}"? This is irreversible.`}
          onConfirm={handleDelete}
          onCancel={() => setShowDelete(false)}
        />
      )}
    </div>
  )
}
