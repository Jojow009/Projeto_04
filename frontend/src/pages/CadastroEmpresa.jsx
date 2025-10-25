import React, { useState } from 'react';
import { createEmpresa } from '../services/api';
import { useNavigate } from 'react-router-dom';

function CadastroEmpresa() {
  const [formData, setFormData] = useState({
    uf: '',
    nomeFantasia: '',
    cnpj: ''
  });
  const [mensagem, setMensagem] = useState('');
  const navigate = useNavigate();

  // Função para lidar com mudanças nos inputs
  const handleChange = (e) => {
    let { name, value } = e.target;
    if (name === 'uf') {
      value = value.toUpperCase().slice(0, 2);
    }
    setFormData({ ...formData, [name]: value });
  };

  // Função para lidar com o envio do formulário
  const handleSubmit = async (e) => {
    e.preventDefault();
    setMensagem('');
    
    const dadosEmpresa = {
        ...formData,
        cnpj: formData.cnpj.replace(/[.\-/ ]/g, '')
    };

    if (dadosEmpresa.uf.length !== 2) {
        setMensagem('Erro: A UF deve ter exatamente 2 caracteres.');
        return;
    }

    try {
      await createEmpresa(dadosEmpresa);
      setMensagem('Empresa cadastrada com sucesso! Redirecionando...');
      setTimeout(() => navigate('/empresas'), 2000); // Vai para a lista de empresas
    } catch (error) {
      const errorMsg = error.response?.data?.title || error.response?.data || 'Erro ao cadastrar empresa.';
      setMensagem(`Erro: ${errorMsg}`);
    }
  };

  return (
    <div>
      {/* Botão Voltar */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
        <h2>Cadastro de Empresa</h2>
        <button onClick={() => navigate(-1)} type="button">Voltar</button>
      </div>

      <form onSubmit={handleSubmit}> 
        <div>
          <label>Nome Fantasia:</label>
          <input name="nomeFantasia" value={formData.nomeFantasia} onChange={handleChange} placeholder="Nome Fantasia" required />
        </div>
        <div>
          <label>CNPJ:</label>
          <input name="cnpj" value={formData.cnpj} onChange={handleChange} placeholder="CNPJ" required />
        </div>
        <div>
          <label>UF:</label>
          <input name="uf" value={formData.uf} onChange={handleChange} placeholder="UF (Ex: PR, SC)" required />
        </div>
        <button type="submit">Salvar Empresa</button>
      </form>
      {mensagem && <p>{mensagem}</p>}
    </div>
  );
}

export default CadastroEmpresa;
