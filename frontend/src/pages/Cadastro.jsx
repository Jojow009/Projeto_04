import React, { useState, useEffect } from 'react';
import { getEmpresas, createFornecedor } from '../services/api';
import { useNavigate } from 'react-router-dom';

function Cadastro() {
  const [empresas, setEmpresas] = useState([]);
  const [formData, setFormData] = useState({
    nome: '',
    cpfCnpj: '',
    rg: '',
    dataNascimento: '',
    empresaId: ''
  });
  const [telefones, setTelefones] = useState([]);
  const [telefoneAtual, setTelefoneAtual] = useState('');
  const [mensagem, setMensagem] = useState('');
  const navigate = useNavigate();

  const cpfCnpjLimpo = formData.cpfCnpj.replace(/[.\-/ ]/g, '');
  const isPessoaFisica = cpfCnpjLimpo.length === 11;

  useEffect(() => {
    getEmpresas()
      .then(response => setEmpresas(response.data))
      .catch(err => console.error("Erro ao buscar empresas:", err));
  }, []);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const adicionarTelefone = () => {
    if (telefoneAtual) {
      setTelefones([...telefones, { numero: telefoneAtual }]);
      setTelefoneAtual('');
    }
  };

  const calcularIdade = (dataNasc) => {
    const hoje = new Date();
    const nascimento = new Date(dataNasc);
    let idade = hoje.getFullYear() - nascimento.getFullYear();
    const m = hoje.getMonth() - nascimento.getMonth();
    if (m < 0 || (m === 0 && hoje.getDate() < nascimento.getDate())) {
      idade--;
    }
    return idade;
  };

  // Função para lidar com o envio do formulário
  const handleSubmit = async (e) => {
    e.preventDefault();
    setMensagem('');

    const dadosCompletos = {
      ...formData,
      cpfCnpj: cpfCnpjLimpo,
      empresaId: parseInt(formData.empresaId),
      telefones,
      dataNascimento: formData.dataNascimento === '' ? null : formData.dataNascimento
    };

    if (isPessoaFisica) {
      if (!dadosCompletos.rg || !dadosCompletos.dataNascimento) {
        setMensagem("Erro: Para pessoa física, RG e Data de Nascimento são obrigatórios.");
        return; 
      }

      const empresaSelecionada = empresas.find(emp => emp.id === dadosCompletos.empresaId);
      if (empresaSelecionada && empresaSelecionada.uf === 'PR') {
        const idade = calcularIdade(dadosCompletos.dataNascimento);
        if (idade < 18) {
          setMensagem("Erro: Não é permitido cadastrar fornecedor pessoa física menor de idade para empresas do Paraná.");
          return; 
        }
      }

    } else { 
      dadosCompletos.rg = null;
      dadosCompletos.dataNascimento = null;
    }

    try {
      await createFornecedor(dadosCompletos);
      setMensagem('Fornecedor cadastrado com sucesso!');
      setTimeout(() => navigate('/'), 2000); // Vai para a listagem
    } catch (error) {
      if (error.response && error.response.data) {
        let errorMsg = error.response.data;
         if (typeof errorMsg === 'object' && errorMsg.title) {
           errorMsg = errorMsg.title;
         } else if (typeof errorMsg !== 'string') {
           errorMsg = errorMsg.toString();
         }
        setMensagem(`Erro: ${errorMsg}`);
      } else {
        setMensagem('Erro de rede ou servidor indisponível.');
      }
    }
  };

  return (
    <div>
      {/* Botão Voltar */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
        <h2>Cadastro de Fornecedor</h2>
        <button onClick={() => navigate(-1)} type="button">Voltar</button>
      </div>

      <form onSubmit={handleSubmit}>
        <select name="empresaId" value={formData.empresaId} onChange={handleChange} required>
          <option value="">Selecione a Empresa</option>
          {empresas.map(e => (
            <option key={e.id} value={e.id}>{e.nomeFantasia} ({e.uf})</option>
          ))}
        </select>
        <br />

        <input name="nome" value={formData.nome} onChange={handleChange} placeholder="Nome" required />
        <br />

        <input name="cpfCnpj" value={formData.cpfCnpj} onChange={handleChange} placeholder="CPF ou CNPJ" required />
        <br />

        {isPessoaFisica && (
          <>
            <input name="rg" value={formData.rg} onChange={handleChange} placeholder="RG" required />
            <br />
            <label>Data de Nascimento:</label>
            <input name="dataNascimento" type="date" value={formData.dataNascimento} onChange={handleChange} required />
            <br />
          </>
        )}

        <div>
          <input
            type="text"
            value={telefoneAtual}
            onChange={(e) => setTelefoneAtual(e.target.value)}
            placeholder="Telefone"
          />
          <button type="button" onClick={adicionarTelefone}>Adicionar Telefone</button>
          <ul>
            {telefones.map((tel, index) => <li key={index}>{tel.numero}</li>)}
          </ul>
        </div>

        <button type="submit">Salvar</button>
      </form>
      {mensagem && <p>{mensagem}</p>}
    </div>
  );
}

export default Cadastro;
