import { NavLink, Outlet, useParams } from 'react-router-dom'

export default function Layout() {
  const { corpusId } = useParams()

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <header className="bg-white border-b border-gray-200 px-6 py-3 flex items-center gap-4">
        <NavLink to="/" className="text-lg font-semibold text-indigo-700 hover:text-indigo-900">
          Veritas
        </NavLink>
        {corpusId && (
          <>
            <span className="text-gray-300">/</span>
            <span className="text-sm text-gray-500 font-mono">{corpusId.slice(0, 8)}…</span>
            <nav className="flex gap-4 text-sm ml-4">
              {[
                { to: `/corpora/${corpusId}`, label: 'Documents' },
                { to: `/corpora/${corpusId}/search`, label: 'Search' },
                { to: `/corpora/${corpusId}/validation`, label: 'Validation' },
                { to: `/corpora/${corpusId}/experiments`, label: 'Experiments' },
                { to: `/corpora/${corpusId}/hypothesis`, label: 'Hypothesis' },
                { to: `/corpora/${corpusId}/compare`, label: 'Compare Packs' },
                { to: `/corpora/${corpusId}/analysis`, label: 'Analysis' },
              ].map(({ to, label }) => (
                <NavLink
                  key={to}
                  to={to}
                  end={to.endsWith(corpusId)}
                  className={({ isActive }) =>
                    isActive
                      ? 'text-indigo-700 font-medium'
                      : 'text-gray-600 hover:text-indigo-600'
                  }
                >
                  {label}
                </NavLink>
              ))}
            </nav>
          </>
        )}
      </header>
      <main className="flex-1 p-6 max-w-6xl mx-auto w-full">
        <Outlet />
      </main>
    </div>
  )
}
