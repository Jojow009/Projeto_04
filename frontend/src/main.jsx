// src/main.jsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';

import './index.css'; // ✅ Import do arquivo de estilos global

import Listagem from './pages/Listagem';
import Cadastro from './pages/Cadastro';

// Componente principal que define o layout e as rotas
function App() {
  return (
    <BrowserRouter>
      <div>
        <nav>
          <ul>
            <li><Link to="/">Listagem de Fornecedores</Link></li>
            <li><Link to="/cadastrar">Cadastrar Novo Fornecedor</Link></li>
            {/* Você pode adicionar um link para cadastrar empresa aqui */}
          </ul>
        </nav>

        <hr />

        <Routes>
          <Route path="/" element={<Listagem />} />
          <Route path="/cadastrar" element={<Cadastro />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
