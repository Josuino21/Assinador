# Assinador Pmenos

Aplicativo Windows para assinar documentos PDF e gerar arquivos `.p7s` ou `.pdf.p7s` usando certificado digital PFX/P12.

## Funcionalidades

- Assinatura de um ou vários PDFs em lote
- Suporte a certificado digital `.pfx` e `.p12`
- Campo de senha com opção de exibir/ocultar
- Modo seguro para envio oficial
- Geração de `.pdf.p7s` com PDF embutido
- Geração de `.p7s` destacado
- Seleção de algoritmo de hash
- Validação básica dos PDFs antes da assinatura
- Verificação de permissões de leitura e gravação
- Execução sem privilégios de administrador
- Executável portátil para Windows

## Requisitos Para Uso

O executável final é portátil/self-contained.

Você não precisa instalar .NET no computador para usar o arquivo:

```text
assinador_pmenos.exe
Como Usar
Abra assinador_pmenos.exe.
Clique em Adicionar PDFs.
Selecione um ou mais arquivos PDF.
Escolha a pasta de saída.
Selecione o certificado digital .pfx ou .p12.
Digite a senha do certificado.
Mantenha o Modo seguro para envio oficial marcado, se desejar gerar .pdf.p7s embutido com SHA-256.
Clique em Assinar documento(s).
Os arquivos assinados serão gerados na pasta escolhida.

Modo Seguro
O modo seguro configura automaticamente:

Formato: .pdf.p7s com PDF embutido
Hash: SHA-256
Esse é o modo recomendado para uso comum.

Linha De Comando
Também é possível usar o aplicativo pelo terminal.

Assinar com PFX/P12
.\assinador_pmenos.exe documento.pdf certificado.pfx -o saida.pdf.p7s --senha MINHA_SENHA --embutido --hash sha256
Assinar com certificado instalado no usuário
.\assinador_pmenos.exe --sign entrada.pdf saida.p7s THUMBPRINT_DO_CERTIFICADO
Testes
Para executar os testes internos:

.\assinador_pmenos.exe --self-test
Resultado esperado:

SELF-TEST OK
Os testes verificam:

validação básica de PDF
permissões de leitura
permissões de gravação
geração correta de nomes .p7s
assinatura em lote com 2 PDFs
execução sem exigir administrador
Desenvolvimento
O projeto foi desenvolvido em C# com WinForms e .NET 8.

Estrutura principal:

src/
  Program.cs
  MainForm.cs
  BatchSigner.cs
  P7sSigner.cs
  PfxCertificateProvider.cs
  CertificateProvider.cs
  PdfValidator.cs
  AccessPolicy.cs
  AppInfo.cs
  Logger.cs
  SelfTests.cs
  assets/
    assinador_pmenos.ico
Como Compilar
Abra o PowerShell na pasta do projeto e execute:

powershell -ExecutionPolicy Bypass -File .\publish-portable.ps1
O executável será gerado em:

publish\win-x86-portable\assinador_pmenos.exe
Políticas De Permissão
O aplicativo foi projetado para respeitar o princípio de menor privilégio:

não solicita administrador automaticamente
usa permissões do usuário atual
valida leitura do PDF antes da assinatura
valida gravação na pasta de saída
registra erros em %LOCALAPPDATA%\AssinadorPmenos
Limitações
A assinatura é gerada usando APIs CMS/PKCS#7 do .NET.

Se um portal exigir regras CAdES muito específicas, como atributos avançados obrigatórios, pode ser necessário validar o arquivo assinado no ambiente oficial ou adaptar a camada de assinatura.