# Task 7.0: Testes de Integração e Validação Final

**Complexidade:** MEDIUM  
**Status:** Pendente  
**Responsável:** TBD  
**Estimativa:** 4 horas

---

## Objetivo

Executar suite completa de testes (unitários + integração), validar endpoints principais manualmente, verificar performance e documentar todas as mudanças realizadas na refatoração.

---

## Contexto

Esta é a tarefa final de validação após remover completamente a herança de BaseRepository. O objetivo é garantir que o sistema está funcionando perfeitamente sem regressões, com performance mantida ou melhorada, e que toda a documentação está atualizada.

### Pré-requisitos
- ✅ Tasks 1.0 a 6.0 concluídas
- ✅ BaseRepository e IBaseRepository removidos
- ✅ Todos os repositórios refatorados
- ✅ Projeto compilando sem erros

---

## Subtarefas

### 7.1 Executar Suite Completa de Testes Unitários
**Descrição:** Validar que todos os testes unitários passam após a refatoração completa.

**Passos:**
1. Executar todos os testes unitários: `dotnet test --filter Category=Unit`
2. Verificar que 100% dos testes passam
3. Analisar relatório de cobertura de código
4. Identificar áreas com baixa cobertura
5. Validar que cobertura foi mantida ou aumentada

**Critério de Sucesso:**
- 100% dos testes unitários passando
- Cobertura de código ≥ baseline anterior
- Nenhum teste flaky identificado
- Relatório de testes gerado

---

### 7.2 Executar Suite Completa de Testes de Integração
**Descrição:** Validar que todos os testes de integração passam e que a aplicação funciona end-to-end.

**Passos:**
1. Subir banco de dados de testes (PostgreSQL)
2. Subir Redis de testes
3. Executar todos os testes de integração: `dotnet test --filter Category=Integration`
4. Verificar que 100% dos testes passam
5. Validar que não há vazamento de recursos
6. Validar que multi-tenancy funciona corretamente

**Critério de Sucesso:**
- 100% dos testes de integração passando
- Nenhum vazamento de recursos detectado
- Multi-tenancy funcionando corretamente
- Banco de dados limpo após testes

---

### 7.3 Validação Manual de Endpoints Principais
**Descrição:** Testar manualmente os endpoints principais da API para garantir funcionamento correto.

**Passos:**
1. Subir aplicação localmente: `dotnet run --project src/SportHub.Api`
2. Testar endpoints de autenticação:
   - POST /api/auth/login
   - POST /api/auth/refresh
   - POST /api/auth/register
3. Testar endpoints de usuários:
   - GET /api/users
   - GET /api/users/{id}
   - PUT /api/users/{id}
4. Testar endpoints de quadras:
   - GET /api/courts (validar que Sports são incluídos)
   - GET /api/courts/{id} (validar que Sports são incluídos)
   - POST /api/courts
   - PUT /api/courts/{id}
5. Testar endpoints de esportes:
   - GET /api/sports
   - GET /api/sports/{id}
   - POST /api/sports
6. Testar endpoints de reservas:
   - GET /api/reservations (validar que Court é incluído)
   - POST /api/reservations (validar detecção de conflitos)
   - GET /api/reservations/user/{userId}
7. Testar endpoints de tenants:
   - GET /api/tenants
   - POST /api/tenants (provisioning)
   - PUT /api/tenants/{id}

**Critério de Sucesso:**
- Todos os endpoints funcionam corretamente
- Includes (Sports, Court) funcionam
- Detecção de conflitos funciona
- Multi-tenancy funciona (subdomínio e X-Tenant-Slug)
- Autenticação e autorização funcionam

---

### 7.4 Testes de Performance
**Descrição:** Validar que a performance foi mantida ou melhorada após a refatoração.

**Passos:**
1. Executar benchmarks de queries principais:
   - GetAllAsync para cada repositório
   - GetByIdAsync para cada repositório
   - Queries com includes (Courts, Reservations)
2. Comparar com baseline anterior (se disponível)
3. Identificar queries N+1 usando ferramentas de profiling
4. Validar uso correto de AsNoTracking
5. Validar uso correto de AsSplitQuery
6. Medir tempo de resposta dos endpoints principais

**Critério de Sucesso:**
- Performance mantida ou melhorada
- Nenhuma query N+1 detectada
- AsNoTracking usado corretamente
- AsSplitQuery usado onde necessário
- Tempo de resposta dentro do esperado

---

### 7.5 Validação de Multi-Tenancy
**Descrição:** Testar extensivamente o sistema de multi-tenancy para garantir isolamento correto.

**Passos:**
1. Criar 2 tenants de teste (tenant1, tenant2)
2. Criar dados em tenant1 (usuários, quadras, reservas)
3. Criar dados em tenant2 (usuários, quadras, reservas)
4. Validar que tenant1 não vê dados de tenant2
5. Validar que tenant2 não vê dados de tenant1
6. Validar resolução por subdomínio
7. Validar resolução por X-Tenant-Slug header
8. Validar resolução por custom domain
9. Validar que schema public (tenants) está isolado

**Critério de Sucesso:**
- Isolamento completo entre tenants
- Nenhum vazamento de dados entre tenants
- Resolução de tenant funciona por todos os métodos
- Schema public isolado corretamente

---

### 7.6 Code Review e Qualidade de Código
**Descrição:** Revisar código refatorado para garantir qualidade e consistência.

**Passos:**
1. Executar análise estática: `dotnet build /p:TreatWarningsAsErrors=true`
2. Executar linter se disponível
3. Revisar consistência entre repositórios:
   - Mesma estrutura de código
   - Mesmos nomes de métodos
   - Mesma forma de injetar DbContext
4. Revisar comentários XML
5. Revisar tratamento de erros
6. Revisar uso de async/await
7. Identificar code smells remanescentes

**Critério de Sucesso:**
- Nenhum warning de compilação
- Código consistente entre repositórios
- Comentários XML adequados
- Nenhum code smell identificado
- Boas práticas de async/await seguidas

---

### 7.7 Atualizar Documentação Técnica
**Descrição:** Atualizar toda a documentação para refletir as mudanças realizadas.

**Passos:**
1. Atualizar `documentos/techspec-codebase.md`:
   - Remover seção sobre BaseRepository
   - Adicionar seção sobre novo padrão de repositórios
   - Atualizar exemplos de código
   - Documentar diferenças do TenantRepository
2. Atualizar `.windsurf/rules/techspec-codebase.md` se necessário
3. Atualizar README.md se menciona repositórios
4. Criar ou atualizar CHANGELOG.md:
   - Documentar refatoração
   - Listar breaking changes (se houver)
   - Listar melhorias
5. Adicionar comentários no código onde necessário

**Critério de Sucesso:**
- Documentação técnica atualizada
- CHANGELOG atualizado
- README atualizado se necessário
- Código bem comentado

---

### 7.8 Preparar Release Notes
**Descrição:** Preparar notas de release sobre a refatoração.

**Passos:**
1. Criar documento de release notes
2. Listar mudanças principais:
   - Remoção de BaseRepository e IBaseRepository
   - Implementação explícita em cada repositório
   - Melhorias de testabilidade
   - Eliminação de code smells
3. Listar breaking changes (se houver)
4. Listar melhorias de performance
5. Adicionar exemplos de migração se necessário
6. Adicionar métricas (linhas de código, cobertura, etc.)

**Critério de Sucesso:**
- Release notes completas
- Mudanças claramente documentadas
- Breaking changes listados
- Exemplos fornecidos

---

### 7.9 Validação Final e Sign-off
**Descrição:** Validação final antes de considerar a refatoração completa.

**Passos:**
1. Checklist final:
   - [ ] Todos os testes passando (unitários + integração)
   - [ ] Nenhuma regressão funcional
   - [ ] Performance mantida ou melhorada
   - [ ] Multi-tenancy funcionando
   - [ ] Documentação atualizada
   - [ ] Code review aprovado
   - [ ] Release notes preparadas
2. Executar smoke tests em ambiente de staging (se disponível)
3. Obter aprovação de stakeholders
4. Preparar para merge na branch principal

**Critério de Sucesso:**
- Checklist completo
- Aprovação de stakeholders obtida
- Pronto para merge

---

## Testes

### Testes Automatizados
- [ ] 100% testes unitários passando
- [ ] 100% testes integração passando
- [ ] Cobertura de código mantida ou aumentada
- [ ] Nenhum teste flaky

### Testes Manuais
- [ ] Login funciona
- [ ] Refresh token funciona
- [ ] CRUD de usuários funciona
- [ ] CRUD de quadras funciona (com Sports)
- [ ] CRUD de esportes funciona
- [ ] CRUD de reservas funciona (com Court)
- [ ] Detecção de conflitos funciona
- [ ] CRUD de tenants funciona
- [ ] Provisioning funciona

### Testes de Performance
- [ ] Nenhuma query N+1 detectada
- [ ] AsNoTracking usado corretamente
- [ ] AsSplitQuery usado onde necessário
- [ ] Tempo de resposta aceitável

### Testes de Multi-Tenancy
- [ ] Isolamento entre tenants funciona
- [ ] Resolução por subdomínio funciona
- [ ] Resolução por X-Tenant-Slug funciona
- [ ] Resolução por custom domain funciona
- [ ] Schema public isolado

---

## Critérios de Aceitação

- ✅ 100% dos testes unitários passando
- ✅ 100% dos testes de integração passando
- ✅ Todos os endpoints principais validados manualmente
- ✅ Performance mantida ou melhorada
- ✅ Nenhuma query N+1 detectada
- ✅ Multi-tenancy funcionando corretamente
- ✅ Nenhuma regressão funcional detectada
- ✅ Código revisado e aprovado
- ✅ Documentação técnica atualizada
- ✅ Release notes preparadas
- ✅ Aprovação de stakeholders obtida

---

## Arquivos Afetados

- `documentos/techspec-codebase.md`
- `.windsurf/rules/techspec-codebase.md`
- `README.md`
- `CHANGELOG.md`
- `docs/RELEASE_NOTES.md` (novo)

---

## Dependências

- **Task 6.0** - BaseRepository e IBaseRepository removidos
- **Tasks 1.0 a 5.0** - Todos os repositórios refatorados

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Regressão não detectada | Baixa | Crítico | Testes extensivos manuais e automatizados |
| Performance degradada | Baixa | Alto | Benchmarks e profiling |
| Vazamento entre tenants | Muito Baixa | Crítico | Testes específicos de isolamento |
| Documentação incompleta | Média | Médio | Revisão cuidadosa da documentação |

---

## Ferramentas e Comandos

### Testes
```bash
# Todos os testes
dotnet test

# Apenas unitários
dotnet test --filter Category=Unit

# Apenas integração
dotnet test --filter Category=Integration

# Com cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Com verbosidade
dotnet test --verbosity detailed
```

### Performance
```bash
# Profiling com dotnet-trace
dotnet-trace collect --process-id <PID>

# Análise de queries com EF Core logging
# Adicionar no appsettings.json:
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

### Análise Estática
```bash
# Build com warnings como erros
dotnet build /p:TreatWarningsAsErrors=true

# Análise de código
dotnet format --verify-no-changes
```

---

## Checklist Final

### Funcionalidade
- [ ] Autenticação funciona
- [ ] Autorização funciona
- [ ] CRUD de todas as entidades funciona
- [ ] Multi-tenancy funciona
- [ ] Provisioning funciona

### Qualidade
- [ ] Todos os testes passam
- [ ] Cobertura adequada
- [ ] Nenhum code smell
- [ ] Código consistente
- [ ] Bem documentado

### Performance
- [ ] Nenhuma query N+1
- [ ] AsNoTracking usado
- [ ] AsSplitQuery usado
- [ ] Tempo de resposta OK

### Documentação
- [ ] Tech spec atualizada
- [ ] README atualizado
- [ ] CHANGELOG atualizado
- [ ] Release notes criadas
- [ ] Comentários adequados

### Processo
- [ ] Code review feito
- [ ] Aprovação obtida
- [ ] Pronto para merge

---

## Métricas de Sucesso

### Antes da Refatoração
- Repositórios com herança: 5
- Uso de `new` para sobrescrever: 1 (CourtsRepository)
- Linhas de código em BaseRepository: ~65
- Linhas de código em IBaseRepository: ~15

### Depois da Refatoração
- Repositórios independentes: 5
- Uso de `new` para sobrescrever: 0
- Código mais explícito: ✅
- Testabilidade melhorada: ✅
- Flexibilidade aumentada: ✅

---

## Referências

- **PRD:** `tasks/prd-remocao-heranca-base-repository/prd.md`
- **Tasks anteriores:** Tasks 1.0 a 6.0
- **Tech Spec:** `documentos/techspec-codebase.md`
- **Scalar Docs:** http://localhost:5001/scalar/v1 (quando API rodando)
