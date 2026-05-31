# SentinelAuth Frontend

Interface React/Vite para login e cadastro global do SentinelAuth.

## Rodando local

```bash
npm install
npm run dev
```

Por padrao, o frontend chama a API em `http://localhost:5254`.

## Fluxo de autorizacao

Abra o app informando o aplicativo consumidor:

```text
http://localhost:5173/authorize?client_id=ingressinhos-api&redirect_uri=ingressinhos://auth/callback&state=valor-aleatorio
```

Depois do login, o SentinelAuth redireciona para o `redirect_uri` com `code` e `state`.

## Administracao

O painel administrativo fica em:

```text
http://localhost:5173/admin
```

Nele da para cadastrar aplicacoes, cadastrar roles por aplicacao, atribuir roles a usuarios e consultar os vinculos ja existentes.

A aba `Integracao` mostra o passo a passo e exemplos de codigo para integrar um app novo ou migrar um sistema que ja possui auth proprio.
