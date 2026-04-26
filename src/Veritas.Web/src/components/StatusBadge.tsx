const colours: Record<string, string> = {
  ready: 'bg-green-100 text-green-800',
  indexing: 'bg-blue-100 text-blue-800',
  pending: 'bg-gray-100 text-gray-700',
  extracting: 'bg-yellow-100 text-yellow-800',
  error: 'bg-red-100 text-red-800',
  needs_review: 'bg-orange-100 text-orange-800',
  uploaded: 'bg-gray-100 text-gray-700',
  experimental: 'bg-purple-100 text-purple-800',
  stable: 'bg-green-100 text-green-800',
  deprecated: 'bg-red-100 text-red-800',
}

export default function StatusBadge({ status }: { status: string }) {
  const cls = colours[status] ?? 'bg-gray-100 text-gray-700'
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}>
      {status.replace(/_/g, ' ')}
    </span>
  )
}
