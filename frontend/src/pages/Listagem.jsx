// src/pages/Listagem.jsx
import React, { useState, useEffect } from 'react';
import { getFornecedores } from '../services/api';

function Listagem() {
  const [fornecedores, setFornecedores] = useState([]);
  const [filtros, setFiltros] = useState({
    nome: '',
    cpfCnpj: '',
    dataCadastro: ''
  });

  const handleFiltroChange = (e) => {
    setFiltros({ ...filtros, [e.target.name]: e.target.value });
  };

  const buscarFornecedores = async () => {
    // Remove filtros vazios antes de enviar
    const params = {};
    if (filtros.nome) params.nome = filtros.nome;
    if (filtros.cpfCnpj) params.cpfCnpj = filtros.cpfCnpj;
    if (filtros.dataCadastro) params.dataCadastro = filtros.dataCadastro;

    try {
      const response = await getFornecedores(params);
      setFornecedores(response.data);
    } catch (error) {
      console.error("Erro ao buscar fornecedores:", error);
    }
  };

  // Busca inicial ao carregar a página
  useEffect(() => {
    buscarFornecedores();
  }, []);

  return (
    <div>
      <h2>Listagem de Fornecedores</h2>

      <div>
        <input name="nome" value={filtros.nome} onChange={handleFiltroChange} placeholder="Filtrar por Nome" />
        <input name="cpfCnpj" value={filtros.cpfCnpj} onChange={handleFiltroChange} placeholder="Filtrar por CPF/CNPJ" />
        <input name="dataCadastro" type="date" value={filtros.dataCadastro} onChange={handleFiltroChange} />
        <button onClick={buscarFornecedores}>Filtrar</button>
      </div>

      <table border="1" style={{ width: '100%', marginTop: '20px' }}>
        <thead>
          <tr>
            <th>Nome</th>
            <th>CPF/CNPJ</th>
            <th>Data Cadastro</th>
            <th>Telefones</th>
          </tr>
        </thead>
        <tbody>
          {fornecedores.map(f => (
            <tr key={f.id}>
              <td>{f.nome}</td>
              <td>{f.cpfCnpj}</td>
              <td>{new Date(f.dataCadastro).toLocaleDateString()}</td>
              <td>
                {f.telefones.map(t => t.numero).join(', ')}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default Listagem;