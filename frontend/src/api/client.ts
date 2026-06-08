import axios from 'axios'
import { apiBaseUrl } from '../config'

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 15000,
})
