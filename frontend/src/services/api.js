import axios from 'axios';


const api = axios.create({
  baseURL: 'https://projeto-04.onrender.com/api', 
});

// Empresas
// CORREÇÃO: Removi a barra '/' do início.
// Agora 'empresas' será combinado com a baseURL para formar '/api/empresas'
export const getEmpresas = () => api.get('empresas');
export const createEmpresa = (dados) => api.post('empresas', dados);
export const deleteEmpresa = (id) => api.delete(`empresas/${id}`);

// Fornecedores
export const getFornecedores = (params) => api.get('fornecedores', { params });
export const createFornecedor = (dados) => api.post('fornecedores', dados);
export const deleteFornecedor = (id) => api.delete(`fornecedores/${id}`);
