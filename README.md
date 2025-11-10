# Cidade Unida - Backend

Este repositório contém a implementação do **backend** da plataforma **Cidade Unida**, desenvolvido utilizando **C#**, **ASP.NET 6.0** e seguindo os princípios da **Arquitetura Limpa**. A API fornece os serviços necessários para o funcionamento da plataforma, atualmente incluindo funcionalidade de backup do Banco de Dados e integração com o Firebase. Funcionalidades futuras planejadas incluem autenticação, gerenciamento de denúncias, sistema de gamificação.
 
## Funcionalidades
- Integração com Firebase
- Sistema de Backup

## Tecnologias Utilizadas

- **C#** – Linguagem de programação principal.
- **ASP.NET 6.0** – Framework para desenvolvimento da API.
- **Entity Framework Core** – ORM para acesso ao banco de dados.
- **SQL Server** – Banco de dados relacional.
- **Firebase** – Autenticação e Firestore Database.
- **Arquitetura Limpa** – Organização do código em camadas bem definidas.
- **Swagger** – Interface interativa para documentação e testes dos endpoints da API.

## Estrutura de Branches

- `main`: Contém a versão estável do backend.
- `develop`: Branch principal para desenvolvimento e integração de novas funcionalidades antes de serem mescladas na `main`.
- `feature/nome-da-feature`: Branch temporária usada para implementar funcionalidades, correções ou melhorias antes de serem integradas à `develop`.

## Padrões de Commits e Pull Requests

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