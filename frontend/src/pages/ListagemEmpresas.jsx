import React, { useState, useEffect, useCallback } from 'react';
import { getEmpresas, deleteEmpresa } from '../services/api';

function ListagemEmpresas() {
  const [empresas, setEmpresas] = useState([]);
  const [mensagem, setMensagem] = useState('');
  
  // Novos estados para loading e erro
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Usamos useCallback para que a função não seja recriada em cada render
  const carregarEmpresas = useCallback(async () => {
    setIsLoading(true); // Começa a carregar
    setError(null);    // Limpa erros antigos
    setMensagem('');
    
    try {
      const response = await getEmpresas();
      setEmpresas(response.data);
      if (response.data.length === 0) {
        setMensagem("Nenhuma empresa cadastrada.");
      }
    } catch (error) {
      console.error("Erro ao buscar empresas:", error);
      // Define a mensagem de erro para o usuário ver
      setError("Falha ao carregar empresas. A API pode estar a 'acordar'. Tente novamente.");
    } finally {
      setIsLoading(false); // Termina de carregar (com sucesso ou erro)
    }
  }, []); // A função só será recriada se 'getEmpresas' mudar (o que não acontece)

  // Carrega as empresas quando a página abre
  useEffect(() => {
    carregarEmpresas();
  }, [carregarEmpresas]); // Depende da função que definimos

  // Função para apagar
  const handleApagar = async (id) => {
    // (Usamos um confirm em vez de window.confirm para funcionar melhor no browser)
    if (confirm("Tem certeza que deseja apagar esta empresa?")) {
      try {
        await deleteEmpresa(id);
        setMensagem("Empresa apagada com sucesso!");
        // Recarrega a lista
        carregarEmpresas();
      } catch (error) {
        const errorMsg = error.response?.data || "Erro ao apagar empresa.";
        console.error("Erro ao apagar empresa:", errorMsg);
        setMensagem(`Erro: ${errorMsg}`);
      }
    }
  };

  // --- RENDERIZAÇÃO CONDICIONAL ---

  // 1. Estado de Loading
  if (isLoading) {
    return (
      <div>
        <h2>Empresas e Seus Fornecedores</h2>
        <p>A carregar empresas... (A API pode estar a acordar, aguarde...)</p>
      </div>
    );
  }

  // 2. Estado de Erro (permite tentar novamente)
  if (error) {
    return (
      <div>
        <h2>Empresas e Seus Fornecedores</h2>
        <p style={{ color: 'red' }}>{error}</p>
        <button onClick={carregarEmpresas}>Tentar Novamente</button>
      </div>
    );
  }

  // 3. Estado de Sucesso (com ou sem empresas)
  return (
    <div>
      <h2>Empresas e Seus Fornecedores</h2>
      {mensagem && <p>{mensagem}</p>}

      <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
        {empresas.map(empresa => (
          <div key={empresa.id} style={{ border: '1px solid #ccc', padding: '15px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h3>{empresa.nomeFantasia} ({empresa.uf})</h3>
              <button onClick={() => handleApagar(empresa.id)} style={{ color: 'white', backgroundColor: '#007bff', border: 'none', padding: '8px 12px', borderRadius: '4px', cursor: 'pointer' }}>
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
