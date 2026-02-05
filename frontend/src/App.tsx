import { Routes, Route, Navigate } from 'react-router-dom'
import SignUpPage from './pages/SignUpPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/sign-up" replace />} />
      <Route path="/sign-up" element={<SignUpPage />} />
    </Routes>
  )
}

export default App
