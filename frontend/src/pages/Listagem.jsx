import React, { useState } from 'react';
// Importa as funções da API, incluindo a de delete
import { getFornecedores, deleteFornecedor } from '../services/api';

function Listagem() {
  const [fornecedores, setFornecedores] = useState([]);
  const [mensagem, setMensagem] = useState('');
  const [filtros, setFiltros] = useState({
    nome: '',
    cpfCnpj: '',
    dataCadastro: ''
  });

  const handleFiltroChange = (e) => {
    setFiltros({ ...filtros, [e.target.name]: e.target.value });
  };

  const buscarFornecedores = async () => {
    setMensagem(''); // Limpa mensagens antigas
    const params = {};
    if (filtros.nome) params.nome = filtros.nome;
    if (filtros.cpfCnpj) params.cpfCnpj = filtros.cpfCnpj.replace(/[.\-/ ]/g, '');
    if (filtros.dataCadastro) params.dataCadastro = filtros.dataCadastro;

    try {
      const response = await getFornecedores(params);
      setFornecedores(response.data);
      if (response.data.length === 0) {
        setMensagem("Nenhum fornecedor encontrado.");
      }
    } catch (error) {
      console.error("Erro ao buscar fornecedores:", error);
      setMensagem("Erro ao buscar fornecedores.");
    }
  };

  const handleApagar = async (id) => {
    if (window.confirm("Tem certeza que deseja apagar este fornecedor?")) {
      try {
        await deleteFornecedor(id);
        setMensagem("Fornecedor apagado com sucesso!");
        setFornecedores(fornecedores.filter(f => f.id !== id));
      } catch (error) {
        console.error("Erro ao apagar fornecedor:", error);
        setMensagem("Erro ao apagar fornecedor.");
      }
    }
  };

  // A linha 'isPessoaFisica' FOI REMOVIDA DAQUI

  return (
    <div>
      <h2>Listagem de Fornecedores</h2>

      {/* Requisito 4: Filtros por Nome, CPF/CNPJ e data de cadastro. */}
      <div>
        <input 
          name="nome" 
          value={filtros.nome} 
          onChange={handleFiltroChange} 
          placeholder="Filtrar por Nome" 
        />
        <input 
          name="cpfCnpj" 
          value={filtros.cpfCnpj} 
          onChange={handleFiltroChange} 
          placeholder="Filtrar por CPF/CNPJ" 
        />
        <input 
          name="dataCadastro" 
          type="date" 
          value={filtros.dataCadastro} 
          onChange={handleFiltroChange} 
        />
        <button onClick={buscarFornecedores}>Filtrar</button>
      </div>

      {mensagem && <p>{mensagem}</p>}

      <table border="1" style={{ width: '100%', marginTop: '20px' }}>
        <thead>
          <tr>
            <th>Nome</th>
            <th>CPF/CNPJ</th>
            <th>Data Cadastro</th>
            <th>Telefones</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {fornecedores.map(f => (
            <tr key={f.id}>
              <td>{f.nome}</td>
              <td>{f.cpfCnpj}</td>
              <td>{new Date(f.dataCadastro).toLocaleString()}</td>
              <td>{f.telefones.map(t => t.numero).join(', ')}</td>
              <td>
                <button onClick={() => handleApagar(f.id)} style={{color: 'red'}}>
                  Apagar
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default Listagem;