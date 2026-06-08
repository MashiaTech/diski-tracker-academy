import SportsSoccerIcon from '@mui/icons-material/SportsSoccer'
import PlayCircleOutlineIcon from '@mui/icons-material/PlayCircleOutline'
import QueryStatsIcon from '@mui/icons-material/QueryStats'
import {
  AppBar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Container,
  Grid,
  Stack,
  Toolbar,
  Typography,
} from '@mui/material'
import { apiBaseUrl } from './config'

function App() {
  return (
    <Box sx={{ minHeight: '100vh', background: 'linear-gradient(180deg, #f6fbff 0%, #edf5ff 100%)' }}>
      <AppBar position="static" color="transparent" elevation={0}>
        <Toolbar>
          <SportsSoccerIcon sx={{ mr: 1 }} />
          <Typography variant="h6" sx={{ fontWeight: 700 }}>
            DiskiTrack Frontend
          </Typography>
          <Box sx={{ flexGrow: 1 }} />
          <Chip label="MUI" color="primary" size="small" />
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 6 }}>
        <Stack spacing={3}>
          <Typography variant="h3" sx={{ fontWeight: 800 }}>
            Football Analysis Dashboard
          </Typography>
          <Typography color="text.secondary" sx={{ maxWidth: 840 }}>
            This frontend runs independently and consumes your backend through the configured API base URL.
          </Typography>
          <Card variant="outlined" sx={{ borderRadius: 3 }}>
            <CardContent>
              <Typography variant="overline" color="text.secondary">
                API Endpoint
              </Typography>
              <Typography variant="body1" sx={{ fontWeight: 700 }}>
                {apiBaseUrl}
              </Typography>
            </CardContent>
          </Card>

          <Grid container spacing={2}>
            <Grid item xs={12} md={4}>
              <Card sx={{ borderRadius: 3, height: '100%' }}>
                <CardContent>
                  <Stack spacing={1}>
                    <PlayCircleOutlineIcon color="primary" />
                    <Typography variant="h6">Live Matches</Typography>
                    <Typography variant="body2" color="text.secondary">
                      Stream match state, events, and momentum shifts in real time.
                    </Typography>
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} md={4}>
              <Card sx={{ borderRadius: 3, height: '100%' }}>
                <CardContent>
                  <Stack spacing={1}>
                    <QueryStatsIcon color="primary" />
                    <Typography variant="h6">Team Analytics</Typography>
                    <Typography variant="body2" color="text.secondary">
                      Compare team performance with possession, xG, and pressing intensity trends.
                    </Typography>
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} md={4}>
              <Card sx={{ borderRadius: 3, height: '100%' }}>
                <CardContent>
                  <Stack spacing={1}>
                    <SportsSoccerIcon color="primary" />
                    <Typography variant="h6">Player Tracking</Typography>
                    <Typography variant="body2" color="text.secondary">
                      Surface player heat maps, ball touches, and defensive coverage.
                    </Typography>
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          <Stack direction="row" spacing={2}>
            <Button variant="contained">Open Dashboard</Button>
            <Button variant="outlined">Configure API</Button>
          </Stack>
        </Stack>
      </Container>
    </Box>
  )
}

export default App
