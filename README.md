
# Desafio Umbler

Esta é uma aplicação web que recebe um domínio e mostra suas informações de DNS.

Este é um exemplo real de sistema que utilizamos na Umbler.

Ex: Consultar os dados de registro do dominio `umbler.com`

**Retorno:**
- Name servers (ns254.umbler.com)
- IP do registro A (177.55.66.99)
- Empresa que está hospedado (Umbler)

Essas informações são descobertas através de consultas nos servidores DNS e de WHOIS.

*Obs: WHOIS (pronuncia-se "ruís") é um protocolo específico para consultar informações de contato e DNS de domínios na internet.*

Nesta aplicação, os dados obtidos são salvos em um banco de dados, evitando uma segunda consulta desnecessaria, caso seu TTL ainda não tenha expirado.

*Obs: O TTL é um valor em um registro DNS que determina o número de segundos antes que alterações subsequentes no registro sejam efetuadas. Ou seja, usamos este valor para determinar quando uma informação está velha e deve ser renovada.*

Tecnologias Backend utilizadas:

- C#
- Asp.Net Core
- MySQL
- Entity Framework

Tecnologias Frontend utilizadas:

- Webpack
- Babel
- ES7

Para rodar o projeto você vai precisar instalar:

- dotnet Core SDK (https://www.microsoft.com/net/download/windows dotnet Core 6.0.201 SDK)
- Um editor de código, acoselhamos o Visual Studio ou VisualStudio Code. (https://code.visualstudio.com/)
- NodeJs v17.6.0 para "buildar" o FrontEnd (https://nodejs.org/en/)
- Um banco de dados MySQL (vc pode rodar localmente ou criar um site PHP gratuitamente no app da Umbler https://app.umbler.com/ que lhe oferece o banco Mysql adicionamente)

Com as ferramentas devidamente instaladas, basta executar os seguintes comandos:

Para "buildar" o javascript basta executar:

`npm install`
`npm run build`

Para Rodar o projeto:

Execute a migration no banco mysql:

`dotnet tool update --global dotnet-ef`
`dotnet tool ef database update`

E após: 

`dotnet run` (ou clique em "play" no editor do vscode)

# Objetivos:

Se você rodar o projeto e testar um domínio, verá que ele já está funcionando. Porém, queremos melhorar varios pontos deste projeto:

# FrontEnd

 - Os dados retornados não estão formatados, e devem ser apresentados de uma forma legível.
 - Não há validação no frontend permitindo que seja submetido uma requsição inválida para o servidor (por exemplo, um domínio sem extensão).
 - Está sendo utilizado "vanilla-js" para fazer a requisição para o backend, apesar de já estar configurado o webpack. O ideal seria utilizar algum framework mais moderno como ReactJs ou Blazor.  

# BackEnd

 - Não há validação no backend permitindo que uma requisição inválida prossiga, o que ocasiona exceptions (erro 500).
 - A complexidade ciclomática do controller está muito alta, o ideal seria utilizar uma arquitetura em camadas.
 - O DomainController está retornando a própria entidade de domínio por JSON, o que faz com que propriedades como Id, Ttl e UpdatedAt sejam mandadas para o cliente web desnecessariamente. Retornar uma ViewModel (DTO) neste caso seria mais aconselhado.

# Testes

 - A cobertura de testes unitários está muito baixa, e o DomainController está impossível de ser testado pois não há como "mockar" a infraestrutura.
 - O Banco de dados já está sendo "mockado" graças ao InMemoryDataBase do EntityFramework, mas as consultas ao Whois e Dns não. 

# Dica

- Este teste não tem "pegadinha", é algo pensado para ser simples. Aconselhamos a ler o código, e inclusive algumas dicas textuais deixadas nos testes unitários. 
- Há um teste unitário que está comentado, que obrigatoriamente tem que passar.
- Diferencial: criar mais testes.

# Entrega

- Enviei o link do seu repositório com o código atualizado.
- O repositório deve estar público para que possamos acessar..
- Modifique Este readme adicionando informações sobre os motivos das mudanças realizadas.

# Modificações:

### 1. Arquitetura e Backend
- **Refatoração para Camadas (Services):** A lógica de negócios foi desacoplada do Controller e movida para a classe `DomainService`, seguindo princípios de SOLID e Clean Code.
- **DTOs (ViewModel):** Implementação do `DomainResponseViewModel` para evitar o vazamento de dados sensíveis da entidade (como ID e colunas de controle) para o frontend.
- **Injeção de Dependência:** Configuração correta do container de DI para o `DatabaseContext`, `IWhoisClient` e `IDomainService`.
- **Validação de Backend:** Adicionada verificação de nulidade e formato de domínio antes de processar a requisição.

### 2. Banco de Dados e Integração
- **MySQL Real:** Configuração do Entity Framework para utilizar MySQL em vez de InMemory database para a aplicação em produção.
- **Migrations:** Criação e aplicação de migrations para garantir a estrutura correta das tabelas.
- **Whois Client Real:** Correção da implementação do `WhoisClient` (que possuía um mock hardcoded) para realizar consultas reais à internet utilizando a biblioteca `Whois.NET`.
- **Tratamento de Dados:** Implementação de lógica de parser com Regex e LINQ para extrair corretamente os *Name Servers* e a empresa de hospedagem (*Registrar*) de diferentes formatos de resposta Whois.

### 3. Testes e Qualidade
- **Mocking:** Utilização da biblioteca **Moq** para simular o comportamento do `IWhoisClient` e do Banco de Dados nos testes unitários.
- **Correção de Testes:** Ajuste dos testes antigos que falhavam devido à mudança de tipos de retorno e dependências. Cobertura de testes garantida para cenários de sucesso e domínios não encontrados.
- **Novos Cenários de Teste:** Implementação de testes adicionais para validar regras de negócio críticas, como o uso correto de Cache (TTL), expiração de dados e validação de inputs inválidos.
  
### 4. Frontend
- **Modernização com Blazor:** Reescrita completa da camada de apresentação utilizando **Blazor**, substituindo o antigo *Vanilla JS*. Isso atende ao requisito de modernização do frontend sugerido no desafio.
