import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { getValidationQueue } from '../api/documents'
import type { ValidationQueueItem } from '../types/veritas'
import StatusBadge from '../components/StatusBadge'

export default function ValidationQueuePage() {
  const { corpusId } = useParams<{ corpusId: string }>()
  const [items, setItems] = useState<ValidationQueueItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()

  useEffect(() => {
    if (!corpusId) return
    getValidationQueue(corpusId)
      .then(setItems)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [corpusId])

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Validation queue</h1>
      {error && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">{error}</div>}

      {loading ? (
        <p className="text-gray-500 text-sm">Loading…</p>
      ) : items.length === 0 ? (
        <p className="text-gray-500 text-sm">No documents need review. ✓</p>
      ) : (
        <>
          <p className="text-sm text-orange-700 mb-4">{items.length} document{items.length !== 1 ? 's' : ''} need human review.</p>
          <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">File</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Extracted title</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Status</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Uploaded</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {items.map((item) => (
                  <tr key={item.documentId} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <Link
                        to={`/corpora/${corpusId}/documents/${item.documentId}`}
                        className="text-indigo-600 hover:underline"
                      >
                        {item.originalFilename}
                      </Link>
                    </td>
                    <td className="px-4 py-3 text-gray-500">{item.extractedTitle ?? '—'}</td>
                    <td className="px-4 py-3"><StatusBadge status={item.status} /></td>
                    <td className="px-4 py-3 text-gray-400">{new Date(item.uploadedAt).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}
