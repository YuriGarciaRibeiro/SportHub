---
name: sporthub-frontend-form
description: Scaffolds a form component for SportHub storefront using React Hook Form + Zod validation, following the project's exact form conventions. Use this skill whenever the user wants to create or add a form in the frontend — even if they say "criar formulário de X", "adicionar form para Y", "form de cadastro de Z", "tela de edição de X", "campos para Y", or "validação de formulário". Generates a complete form component with Zod schema, fields, error messages, and mutation integration.
---

# sporthub-frontend-form

Gera um componente de formulário completo seguindo os padrões do projeto storefront.

## Stack de Formulários

- **React Hook Form** (`useForm`, `Controller`) para gerenciamento de estado
- **Zod** para schema de validação
- **@hookform/resolvers/zod** para integrar os dois
- **Sonner** (`toast`) para feedback de sucesso/erro
- **shadcn/ui** para os campos: `Input`, `Button`, `FormField` (de `@workspace/ui`)

## Padrão de um Formulário Simples

```tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@workspace/ui/components/button';
import { Input } from '@workspace/ui/components/input';
import { FormField } from '@workspace/ui/components/form-field';
import { useCreateXxx } from '@/hooks/use-xxx';

const schema = z.object({
  name: z.string().min(1, 'Nome é obrigatório'),
  description: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface XxxFormProps {
  onSuccess?: () => void;
}

export function XxxForm({ onSuccess }: XxxFormProps) {
  const createXxx = useCreateXxx();

  const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  async function onSubmit(values: FormValues) {
    await createXxx.mutateAsync(values);
    reset();
    onSuccess?.();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      <FormField label="Nome" required htmlFor="xxx-name" error={errors.name?.message}>
        <Input
          id="xxx-name"
          autoFocus
          {...register('name')}
          placeholder="Ex: ..."
        />
      </FormField>

      <FormField label="Descrição" htmlFor="xxx-desc" error={errors.description?.message}>
        <Input
          id="xxx-desc"
          {...register('description')}
          placeholder="Ex: ..."
        />
      </FormField>

      <Button type="submit" disabled={isSubmitting || createXxx.isPending} className="bg-brand text-white hover:bg-brand-hover">
        {createXxx.isPending ? 'Salvando...' : 'Criar'}
      </Button>
    </form>
  );
}
```

## Padrão para Formulário de Edição (Create + Edit)

Quando o formulário serve para criação E edição, receba `initial` e `id` opcionais:

```tsx
interface XxxFormProps {
  onClose: () => void;
  initial?: FormValues;
  id?: string;
}

export function XxxForm({ onClose, initial, id }: XxxFormProps) {
  const isEdit = !!id;
  const createXxx = useCreateXxx();
  const updateXxx = useUpdateXxx();

  const { register, handleSubmit, formState, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: initial,
  });

  // Sincroniza com `initial` quando muda (para reuso do componente)
  const [lastInitial, setLastInitial] = useState(initial);
  if (initial !== lastInitial) {
    setLastInitial(initial);
    reset(initial);
  }

  async function onSubmit(values: FormValues) {
    if (isEdit) {
      await updateXxx.mutateAsync({ id: id!, data: values });
    } else {
      await createXxx.mutateAsync(values);
    }
    onClose();
  }

  const isPending = createXxx.isPending || updateXxx.isPending;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      {/* campos... */}
      <div className="flex gap-2 justify-end">
        <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
        <Button type="submit" disabled={isPending} className="bg-brand text-white hover:bg-brand-hover">
          {isPending ? 'Salvando...' : isEdit ? 'Salvar' : 'Criar'}
        </Button>
      </div>
    </form>
  );
}
```

## Tipos de Campos Comuns

**Select (dropdown):**
```tsx
import { Controller } from 'react-hook-form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@workspace/ui/components/select';

<Controller
  control={control}
  name="sportId"
  render={({ field }) => (
    <Select value={field.value} onValueChange={field.onChange}>
      <SelectTrigger><SelectValue placeholder="Selecione..." /></SelectTrigger>
      <SelectContent>
        {sports?.map((s) => <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>)}
      </SelectContent>
    </Select>
  )}
/>
```

**Textarea:**
```tsx
import { Textarea } from '@workspace/ui/components/textarea';
<Textarea {...register('description')} rows={4} />
```

**Número com conversão:**
```tsx
<Input type="number" {...register('price', { valueAsNumber: true })} />
// No schema: z.number().min(0, 'Valor inválido')
```

## Regras

- Sempre use `FormField` de `@workspace/ui/components/form-field` para labels e erros (não `<label>` puro)
- Botão de submit usa `className="bg-brand text-white hover:bg-brand-hover"`
- Mensagens de erro em português
- `autoFocus` no primeiro campo
- Se o formulário está dentro de um `Dialog`, use `DialogFooter` para os botões

## Passos

1. Entenda quais campos o formulário precisa e se é só criação ou criação + edição
2. Verifique se os hooks de mutation já existem em `@/hooks/use-<resource>.ts` — se não, sugira criar com `sporthub-frontend-hook`
3. Gere o componente no lugar adequado:
   - Formulários de página admin: `apps/storefront/src/app/[tenantSlug]/admin/<feature>/_components/<resource>-form.tsx`
   - Formulários públicos (login, registro): `apps/storefront/src/app/[tenantSlug]/<page>/_components/`
4. Confirme os nomes e labels dos campos com o usuário se houver ambiguidade
