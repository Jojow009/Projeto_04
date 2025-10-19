import React, { useState, useEffect } from 'react';
import { getEmpresas, deleteEmpresa } from '../services/api';

function ListagemEmpresas() {
  const [empresas, setEmpresas] = useState([]);
  const [mensagem, setMensagem] = useState('');

  // Função para carregar as empresas
  const carregarEmpresas = async () => {
    setMensagem('');
    try {
      const response = await getEmpresas();
      setEmpresas(response.data);
      if (response.data.length === 0) {
        setMensagem("Nenhuma empresa cadastrada.");
      }
    } catch (error) {
      console.error("Erro ao buscar empresas:", error);
      setMensagem("Erro ao buscar empresas.");
    }
  };

  // Carrega as empresas quando a página abre
  useEffect(() => {
    carregarEmpresas();
  }, []);

  // Função para apagar
  const handleApagar = async (id) => {
    if (window.confirm("Tem certeza que deseja apagar esta empresa?")) {
      try {
        await deleteEmpresa(id);
        setMensagem("Empresa apagada com sucesso!");
        // Recarrega a lista
        carregarEmpresas();
      } catch (error) {
        // Pega a mensagem de erro específica do backend (Ex: "Não pode apagar")
        const errorMsg = error.response?.data || "Erro ao apagar empresa.";
        console.error("Erro ao apagar empresa:", errorMsg);
        setMensagem(`Erro: ${errorMsg}`);
      }
    }
  };

  return (
    <div>
      <h2>Empresas e Seus Fornecedores</h2>
      {mensagem && <p>{mensagem}</p>}

      <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
        {empresas.map(empresa => (
          <div key={empresa.id} style={{ border: '1px solid #ccc', padding: '15px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h3>{empresa.nomeFantasia} ({empresa.uf})</h3>
              <button onClick={() => handleApagar(empresa.id)} style={{ color: 'red' }}>
                Apagar Empresa
              </button>
            </div>
            <p><strong>CNPJ:</strong> {empresa.cnpj}</p>
            
            <h4>Fornecedores desta Empresa:</h4>
            {empresa.fornecedores && empresa.fornecedores.length > 0 ? (
              <table border="1" style={{ width: '100%', fontSize: '0.9em' }}>
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>CPF/CNPJ</th>
                    <th>Telefones</th>
                  </tr>
                </thead>
                <tbody>
                  {empresa.fornecedores.map(f => (
                    <tr key={f.id}>
                      <td>{f.nome}</td>
                      <td>{f.cpfCnpj}</td>
                      <td>{f.telefones.map(t => t.numero).join(', ')}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p>Nenhum fornecedor cadastrado para esta empresa.</p>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

export default ListagemEmpresas;