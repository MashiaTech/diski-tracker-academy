import { createTheme } from '@mui/material/styles'

export const appTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#0e7c66',
    },
    secondary: {
      main: '#1f4e79',
    },
    background: {
      default: '#f6fbff',
      paper: '#ffffff',
    },
  },
  shape: {
    borderRadius: 14,
  },
  typography: {
    fontFamily: ['Poppins', 'Segoe UI', 'Arial', 'sans-serif'].join(','),
    h3: {
      fontWeight: 800,
      letterSpacing: '-0.02em',
    },
    h6: {
      fontWeight: 700,
    },
  },
})
