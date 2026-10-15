---
name: uppercase-formatter
description: Recebe um texto do usuário e o devolve formatado em letras maiúsculas com um prefixo especial.
---

# Uppercase Formatter

## Objective
Transformar o texto enviado pelo usuário aplicando letras maiúsculas e adicionando um prefixo fixo.

## Rules
- Converta todo o texto do usuário para **maiúsculas**.
- Adicione sempre o prefixo `[PROCESSADO]: ` no início da resposta.
- Não adicione explicações, saudações ou conversas fiadas; retorne apenas o texto modificado.

## Examples
- Input: "olá mundo, teste de sistema"
- Output: "[PROCESSADO]: OLÁ MUNDO, TESTE DE SISTEMA"