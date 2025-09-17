# 🌱 SportHub - Dados dos Seeders

Este documento descreve todos os dados que são criados automaticamente pelos seeders do SportHub durante a inicialização do banco de dados.

## 📋 Índice

- [👤 Usuários](#-usuários)
- [🏢 Estabelecimentos](#-estabelecimentos)
- [⚽ Esportes](#-esportes)
- [🏟️ Quadras](#-quadras)
- [📅 Reservas](#-reservas)
- [⭐ Avaliações](#-avaliações)

---

## 👤 Usuários

### 🔑 Administrador
O sistema cria automaticamente **1 usuário administrador** com as credenciais configuradas no `appsettings.json`:

- **Email**: Configurado via `AdminUserSettings:Email`
- **Senha**: Configurada via `AdminUserSettings:Password`
- **Nome**: Configurado via `AdminUserSettings:FirstName` e `LastName`
- **Função**: `Admin`

### 🏢 Membros de Estabelecimentos
**3 usuários** com função `EstablishmentMember`:

| Nome | Email | Senha | ID |
|------|-------|-------|-----|
| John Smith | john.smith@sporthub.com | SportHub123! | 11111111-1111-1111-1111-111111111111 |
| Mary Johnson | mary.johnson@sporthub.com | SportHub123! | 22222222-2222-2222-2222-222222222222 |
| Charles Williams | charles.williams@sporthub.com | SportHub123! | 33333333-3333-3333-3333-333333333333 |

### 👥 Usuários Regulares
**5 usuários** com função `User`:

| Nome | Email | Senha | ID |
|------|-------|-------|-----|
| Anna Brown | anna.brown@email.com | SportHub123! | 44444444-4444-4444-4444-444444444444 |
| Peter Davis | peter.davis@email.com | SportHub123! | 55555555-5555-5555-5555-555555555555 |
| Lucy Miller | lucy.miller@email.com | SportHub123! | 66666666-6666-6666-6666-666666666666 |
| Robert Wilson | robert.wilson@email.com | SportHub123! | 77777777-7777-7777-7777-777777777777 |
| Emily Moore | emily.moore@email.com | SportHub123! | 88888888-8888-8888-8888-888888888888 |

**Total de usuários**: 9 (1 admin + 3 establishment members + 5 users)

---

## 🏢 Estabelecimentos

O sistema cria **25+ estabelecimentos** localizados em Aracaju/SE, cada um associado a um membro específico e oferecendo diferentes esportes.

### 🌟 Estabelecimentos Principais

#### 1. SportHub Arena Central
- **Responsável**: John Smith
- **Localização**: Rua João Pessoa, 450 - Centro, Aracaju/SE
- **Esportes**: Football, Basketball, Volleyball
- **Descrição**: Complexo esportivo completo no centro de Aracaju

#### 2. Club Premium Atalaia
- **Responsável**: Mary Johnson
- **Localização**: Avenida Santos Dumont, 1500 - Atalaia, Aracaju/SE
- **Esportes**: Tennis, Padel
- **Descrição**: Clube premium focado em tênis e padel na Praia de Atalaia

#### 3. Centro Esportivo Grageru
- **Responsável**: Charles Williams
- **Localização**: Rua Campos, 800 - Grageru, Aracaju/SE
- **Esportes**: Futsal, Handball
- **Descrição**: Centro esportivo moderno no bairro Grageru

### 📍 Distribuição Geográfica
Os estabelecimentos estão distribuídos pelos principais bairros de Aracaju:
- Centro
- Atalaia
- Grageru
- Treze de Julho
- Jardins
- São José
- Salgado Filho
- Coroa do Meio
- Ponto Novo
- Farolândia

**Total de estabelecimentos**: 25+

---

## ⚽ Esportes

O sistema cria **8 modalidades esportivas**:

| Nome | Descrição | ID |
|------|-----------|-----|
| **Football** | Esporte jogado em campo de grama com dois times de 11 jogadores | A1111111-1111-1111-1111-111111111111 |
| **Basketball** | Esporte jogado em quadra indoor por dois times de 5 jogadores | A2222222-2222-2222-2222-222222222222 |
| **Volleyball** | Esporte jogado em quadra dividida por rede, com dois times de 6 jogadores | A3333333-3333-3333-3333-333333333333 |
| **Tennis** | Esporte de raquete jogado individualmente ou em pares | A4444444-4444-4444-4444-444444444444 |
| **Futsal** | Variante do futebol jogada em quadra indoor, com dois times de 5 jogadores | A5555555-5555-5555-5555-555555555555 |
| **Handball** | Esporte de equipe jogado com as mãos, dois times de 7 jogadores | A6666666-6666-6666-6666-666666666666 |
| **Padel** | Esporte de raquete jogado em duplas em quadra fechada | A7777777-7777-7777-7777-777777777777 |
| **Beach Tennis** | Esporte de raquete jogado na areia sem deixar a bola quicar | A8888888-8888-8888-8888-888888888888 |

---

## 🏟️ Quadras

### 📊 Geração Dinâmica
O sistema gera automaticamente **múltiplas quadras** para cada estabelecimento baseado nos esportes oferecidos:

- **Football**: 1-2 quadras por estabelecimento
- **Basketball**: 2-3 quadras por estabelecimento  
- **Volleyball**: 2-3 quadras por estabelecimento
- **Tennis**: 2-4 quadras por estabelecimento
- **Futsal**: 3-4 quadras por estabelecimento
- **Handball**: 1-2 quadras por estabelecimento
- **Padel**: 2-4 quadras por estabelecimento
- **Beach Tennis**: 2-3 quadras por estabelecimento

### 🏷️ Nomenclatura
As quadras seguem padrões de nomes realistas:
- **Football**: "Campo Principal", "Campo Secundário"
- **Basketball**: "Quadra A", "Quadra B", "Quadra C"
- **Tennis**: "Quadra 1", "Quadra 2", "Quadra Central"
- **Futsal**: "Quadra 1", "Quadra 2", "Quadra 3", "Quadra 4"

### 💰 Preços
- Preços variam entre **R$ 10 - R$ 50 por slot**
- Cada slot representa **30 minutos** de uso
- Preços são gerados aleatoriamente para cada quadra

**Total estimado de quadras**: 200+

---

## 📅 Reservas

### 🔄 Geração Automática
O sistema cria reservas automaticamente para **todos os usuários** (incluindo admin e establishment members):

### 📊 Distribuição
- **Cada usuário recebe**: 1-4 reservas aleatórias
- **Período**: 10 dias no passado até 15 dias no futuro
- **Horários**: Entre 8h e 22h
- **Duração**: 1-3 horas por reserva

### 💵 Valores
- **Preço por hora**: R$ 10 - R$ 50 (baseado no tipo de quadra)
- **Cálculo**: Preço/hora × duração da reserva
- **Slots**: Calculados como duração × 2 (slots de 30min)

### 📈 Volume Esperado
- **9 usuários** × 1-4 reservas = **18-36 reservas** aproximadamente
- Distribuídas entre **200+ quadras** de **25+ estabelecimentos**

---

## ⭐ Avaliações

### 🎯 Cobertura Completa
O sistema gera avaliações para **todos os estabelecimentos**:

### 👥 Participação
- **Avaliadores**: Apenas usuários com role `User` (5 usuários)
- **Por estabelecimento**: 3-6 avaliações aleatórias
- **Total estimado**: 75-150 avaliações

### ⭐ Sistema de Rating
**Distribuição realista baseada no tipo de estabelecimento**:

#### 🏆 Estabelecimentos Premium/Elite
- 10% recebem 3 estrelas
- 25% recebem 4 estrelas  
- 65% recebem 5 estrelas

#### 🏢 Estabelecimentos Centro/Clube
- 5% recebem 2 estrelas
- 15% recebem 3 estrelas
- 40% recebem 4 estrelas
- 40% recebem 5 estrelas

#### 🏟️ Estabelecimentos Gerais
- 3% recebem 1 estrela
- 8% recebem 2 estrelas
- 20% recebem 3 estrelas
- 50% recebem 4 estrelas
- 19% recebem 5 estrelas

### 💬 Comentários
**70% das avaliações incluem comentários**:

#### 🌟 Comentários Positivos (4-5 estrelas)
- "Excelente estabelecimento! Instalações modernas e bem cuidadas."
- "Ótima experiência! Staff muito atencioso e quadras em perfeito estado."
- "Lugar incrível para praticar esportes. Recomendo!"

#### 😐 Comentários Neutros (3 estrelas)  
- "Lugar ok, atende às necessidades básicas."
- "Instalações razoáveis, nada excepcional."
- "Ambiente agradável, mas pode melhorar."

#### 😞 Comentários Negativos (1-2 estrelas)
- "Instalações precisam de manutenção."
- "Atendimento deixa a desejar."
- "Preço um pouco salgado para o que oferece."

---

## 🚀 Como Executar os Seeders

### 🔧 Configuração
1. Configure as credenciais do admin no `appsettings.json`
2. Execute as migrações do banco de dados
3. Inicie a aplicação

### ⚡ Execução Automática
Os seeders são executados automaticamente na inicialização da aplicação na seguinte ordem:

1. **UserSeeder** (Order: 1) - Cria admin e usuários de teste
2. **SportSeeder** (Order: 2) - Cria modalidades esportivas  
3. **EstablishmentSeeder** (Order: 3) - Cria estabelecimentos
4. **CourtSeeder** (Order: 4) - Cria quadras para cada estabelecimento
5. **ReservationSeeder** (Order: 5) - Cria reservas para todos os usuários
6. **EvaluationSeeder** (Order: 6) - Cria avaliações para todos os estabelecimentos

### 🔄 Proteção contra Duplicação
- Todos os seeders verificam se os dados já existem antes de criar
- Execução segura em múltiplas inicializações
- Log detalhado de todas as operações

---

## 📊 Resumo dos Dados

| Categoria | Quantidade | Características |
|-----------|------------|-----------------|
| **👤 Usuários** | 9 | 1 admin + 3 establishment members + 5 users |
| **🏢 Estabelecimentos** | 25+ | Distribuídos por Aracaju/SE |
| **⚽ Esportes** | 8 | Modalidades populares no Brasil |
| **🏟️ Quadras** | 200+ | Geradas dinamicamente por estabelecimento |
| **📅 Reservas** | 18-36 | Distribuídas entre passado e futuro |
| **⭐ Avaliações** | 75-150 | Ratings realistas com comentários |

---

*Este documento é atualizado automaticamente conforme as modificações nos seeders.*