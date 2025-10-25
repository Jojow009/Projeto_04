
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://knewin-api.onrender.com/api', // Confira sua porta
});

// Empresas
export const getEmpresas = () => api.get('/empresas');
export const createEmpresa = (dados) => api.post('/empresas', dados);
export const deleteEmpresa = (id) => api.delete(`/empresas/${id}`); // <-- ADICIONE

// Fornecedores
export const getFornecedores = (params) => api.get('/fornecedores', { params });
export const createFornecedor = (dados) => api.post('/fornecedores', dados);
export const deleteFornecedor = (id) => api.delete(`/fornecedores/${id}`); // <-- ADICIONE