// src/services/api.js
import axios from 'axios';

// ATENÇÃO: Verifique a porta do seu back-end (no terminal dele)
// Pode ser 5001, 7001, etc.
const api = axios.create({
  baseURL: 'http://localhost:5089/api'
});

// Funções para chamar o back-end
export const getEmpresas = () => api.get('/empresas');
export const getFornecedores = (params) => api.get('/fornecedores', { params });
export const createFornecedor = (data) => api.post('/fornecedores', data);
export const createEmpresa = (data) => api.post('/empresas', data); // (Bônus, se precisar)

export default api;