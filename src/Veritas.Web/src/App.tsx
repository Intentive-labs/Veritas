import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import CorporaPage from './pages/CorporaPage'
import CorpusDetailPage from './pages/CorpusDetailPage'
import DocumentDetailPage from './pages/DocumentDetailPage'
import ValidationQueuePage from './pages/ValidationQueuePage'
import ExperimentsPage from './pages/ExperimentsPage'
import HypothesisPage from './pages/HypothesisPage'
import AnalysisPage from './pages/AnalysisPage'
import SearchPage from './pages/SearchPage'
import PackComparePage from './pages/PackComparePage'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<CorporaPage />} />
          <Route path="corpora/:corpusId" element={<CorpusDetailPage />} />
          <Route path="corpora/:corpusId/documents/:documentId" element={<DocumentDetailPage />} />
          <Route path="corpora/:corpusId/validation" element={<ValidationQueuePage />} />
          <Route path="corpora/:corpusId/search" element={<SearchPage />} />
          <Route path="corpora/:corpusId/experiments" element={<ExperimentsPage />} />
          <Route path="corpora/:corpusId/hypothesis" element={<HypothesisPage />} />
          <Route path="corpora/:corpusId/compare" element={<PackComparePage />} />
          <Route path="corpora/:corpusId/analysis" element={<AnalysisPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
