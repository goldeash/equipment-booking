import '@mantine/core/styles.css'
import '@mantine/dates/styles.css'
import '@mantine/notifications/styles.css'
import './styles/global.css'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './app/App'

const container = document.getElementById('root')
if (!container) {
  throw new Error('The root element is missing.')
}

createRoot(container).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
