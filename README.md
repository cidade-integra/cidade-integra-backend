# 🖥️ Cidade Unida - Backend

Este repositório contém a implementação do **backend** da plataforma **Cidade Unida**, desenvolvido utilizando **C#**, **ASP.NET 6.0** e seguindo os princípios da **Arquitetura Limpa**. A API fornece os serviços necessários para o funcionamento da plataforma, incluindo autenticação, gerenciamento de denúncias e integração com o Firebase.

## 🚀 Tecnologias Utilizadas

- **C#** – Linguagem de programação principal.
- **ASP.NET 6.0** – Framework para desenvolvimento da API.
- **Entity Framework Core** – ORM para acesso ao banco de dados.
- **SQL Server** – Banco de dados relacional.
- **Firebase** – Autenticação e notificações.
- **Arquitetura Limpa** – Organização do código em camadas bem definidas.
- 
<!-- 

## 📦 Instalação e Configuração

Para rodar o backend localmente, siga os passos abaixo:

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/organizacao/backend.git
   cd backend
   ```

2. **Configure as variáveis de ambiente:**
   Crie um arquivo `appsettings.Development.json` na pasta `src/WebAPI` e adicione as configurações do banco de dados e Firebase:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=CidadeUnidaDB;User Id=seu_usuario;Password=sua_senha;"
     },
     "Firebase": {
       "ApiKey": "...",
       "AuthDomain": "...",
       "ProjectId": "..."
     }
   }
   ```

3. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```

4. **Execute as migrações do banco de dados:**
   ```bash
   dotnet ef database update
   ```

5. **Inicie a API:**
   ```bash
   dotnet run --project src/WebAPI
   ```

A API estará disponível em `https://localhost:5001`.



## 🏗️ Estrutura do Projeto

```
/backend
│-- src
│   │-- Application    # Regras de negócio e casos de uso
│   │-- Domain         # Entidades e interfaces de domínio
│   │-- Infrastructure # Implementações de repositórios e serviços externos
│   │-- WebAPI         # Endpoints da API e configurações
│-- tests             # Testes unitários e de integração
│-- README.md         # Documentação do repositório
│-- backend.sln       # Solução do projeto
```

-->

## 🔀 Estrutura de Branches

- `main`: Contém a versão estável do backend.
- `develop`: Branch principal para desenvolvimento e integração de novas funcionalidades antes de serem mescladas na `main`.

## 🔹 Padrões de Commits e Pull Requests

Para manter um histórico organizado, seguimos as diretrizes abaixo:

### Commits

Utilize **Conventional Commits**, por exemplo:
```
feat: adicionar endpoint para criação de denúncia
fix: corrigir erro na autenticação
```

Tipos comuns:
- `feat`: Nova funcionalidade
- `fix`: Correção de erro
- `docs`: Alterações na documentação
- `style`: Alterações que não afetam a lógica
- `refactor`: Melhorias no código sem mudanças na funcionalidade

### Pull Requests

1. **Crie uma nova branch a partir da `develop`**:
   ```bash
   git checkout -b feature/nome-da-feature develop
   ```
2. **Faça suas alterações e commit** seguindo o padrão.
3. **Envie para o repositório remoto:**
   ```bash
   git push origin feature/nome-da-feature
   ```
4. **Abra um Pull Request para a branch `develop`** e aguarde a revisão da equipe.

## 📞 Contato

Caso tenha dúvidas ou sugestões, envie seu Pull Request e contribua com melhorias! 🚀

