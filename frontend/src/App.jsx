import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './index.css'; // Mudei de App.css para index.css (como corrigimos)

// 1. Importe suas páginas
import Listagem from './pages/Listagem';
import Cadastro from './pages/Cadastro';
import CadastroEmpresa from './pages/CadastroEmpresa';
import ListagemEmpresas from './pages/ListagemEmpresas'; // <-- ADICIONE

function App() {
  return (
    <Router>
      <div className="App">

        {/* 2. SEU NOVO MENU DE NAVEGAÇÃO */}
        <nav className="navbar" style={{ padding: '10px', background: '#eee' }}>
          <Link to="/" style={{ marginRight: '10px' }}>Listagem de Fornecedores</Link>
          <Link to="/empresas" style={{ marginRight: '10px' }}>Empresas e Fornecedores</Link> {/* <-- ADICIONE */}
          <Link to="/cadastrar" style={{ marginRight: '10px' }}>Cadastrar Novo Fornecedor</Link>
          <Link to="/cadastrar-empresa">Cadastrar Nova Empresa</Link> 
        </nav>

        {/* 3. ONDE AS PÁGINAS VÃO CARREGAR */}
        <main className="content" style={{ padding: '20px' }}>
          <Routes>
            <Route path="/" element={<Listagem />} />
            <Route path="/empresas" element={<ListagemEmpresas />} /> {/* <-- ADICIONE */}
            <Route path="/cadastrar" element={<Cadastro />} />
            <Route path="/cadastrar-empresa" element={<CadastroEmpresa />} />
          </Routes>
        </main>

      </div>
    </Router>
  );
}

export default App;