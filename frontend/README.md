# DiskiTrack Frontend

Independent React + TypeScript + Vite frontend for DiskiTrack.

This app is intentionally separate from the backend runtime and consumes APIs via environment variables.

## Stack

- React 18
- Vite 5
- MUI (Material UI)
- Axios

## Setup

1. Install dependencies

   npm install

2. Configure environment

   Copy .env.example to .env and update the API URL if needed.

3. Run the app

   npm run dev

## Environment Variables

- VITE_API_BASE_URL
  - Example: http://localhost:5080
  - Used by src/config.ts and src/api/client.ts

## Environment Files

- .env.example
   - Template for required variables.
- .env.development
   - Shared development defaults for the team.
- .env.local
   - Machine-specific overrides (ignored by git).

When running `npm run dev`, Vite loads `.env.development` and then `.env.local`, so `.env.local` takes precedence.
