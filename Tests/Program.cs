using CMP.Certificados;
using CMP.ManipuladorPDF;

//DocumentoPDFConfig.DefinirDiretorioDeFontes();

Adobe.Acrobat.FecharAcrobat();

string path = "C:\\arquivos\\testepdf\\";
string output = "simples.pdf";

//byte[] arr = File.ReadAllBytes($"{path}baiao.pdf");
//byte[] arr = File.ReadAllBytes($"{path}despacho.pdf");

DocumentoPDF documento = new TemplateHtml(
    @"<!DOCTYPE html>
<!-- saved from url=(0091)https://localhost:44368/planocontratacoes/publico/solicitacoesinclusaopca/imprimir/jnkkcjlm -->
<html><head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
        
        <title>Solicitação de Inclusão PCA 2025</title>
        <style>
            table, div {
                break-inside: avoid-page;
            }
        </style>
    </head>
    <body>
            <div style=""display:flex;justify-content:center;margin-bottom:2rem;"">
                <img src=""./Solicitação de Inclusão PCA 2025_files/logo.png"" height=""80"">
                <div style=""padding-bottom:10px; text-align: center; margin-left:20px;"">
                    <p style=""margin:0"">Rua Alferes José Caetano, 834, Centro - Piracicaba/SP CEP 13400-120</p>
                    <p style=""margin:0"">CNPJ: 51.327.708/0001-92 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Inscrição Estadual: isento</p>
                    <p style=""margin:0"">Telefone: (19) 3403-6561 / (19) 3403-6529</p>
                    <p style=""margin:0"">E-mail: compras@camarapiracicaba.sp.gov.br</p>
                </div>
            </div>
        <div style=""margin-bottom:2rem;"">

        </div>
        <div style=""margin-bottom:2rem;"">
        <p style=""text-align:justify""><strong>Vossa Excelência <span style=""text-transform:capitalize"">rerlison teixeira de rezende</span></strong>, Presidênte da Câmara Municipal de Piracicaba, solicito autorização para inclusão no Plano de Contratação Anual de 2025 os seguintes itens:</p>
                
        <div style=""border:1px solid #000; padding:0 1rem;margin-bottom:0.5rem;"">                    
                    <p><strong>Objeto: </strong>eita faltou esse anda</p>
                    <p><strong>Justificativa: </strong>oioioi</p>
                    <div style=""width:49%;float:left;margin-top:-1rem;border:1px solid #f0f;"">
                        <p><strong>Unidade Compra: </strong>Jogo</p>
                        <p><strong>Quantidade: </strong>23212</p>
                        <p><strong>Valor Unitário: </strong>R$ 2,00</p>
                        <p><strong>Valor Total: </strong>R$ 46.424,00</p>
                    </div>
                    <div style=""width:49%;float:left;margin-top:-1rem;border:1px solid #f0f;"">
                        <p><strong>Data Contratação: </strong>03/04/2025</p>
                        <p><strong>Setor: </strong>Suporte de Tecnologia da Informação</p>
                        <p><strong>Departamento: </strong>Tecnologia da Informação</p>
                    </div>
                    <div style=""clear:both;width:1px;height:1px;""></div>
                </div>
                <div style=""border:1px solid #000; padding:0 1rem;margin-bottom:0.5rem;"">                    
                    <p><strong>Objeto: </strong>e mais esse</p>
                    <p><strong>Justificativa: </strong>rapaiz</p>
                    <div style=""width:50%;float:left;margin-top:-1rem;"">
                        <p><strong>Unidade Compra: </strong>Jogo</p>
                        <p><strong>Quantidade: </strong>43</p>
                        <p><strong>Valor Unitário: </strong>R$ 321,00</p>
                        <p><strong>Valor Total: </strong>R$ 13.803,00</p>
                    </div>
                    <div style=""width:50%;float:left;margin-top:-1rem;"">
                        <p><strong>Data Contratação: </strong>03/04/2025</p>
                        <p><strong>Setor: </strong>Desenvolvimento de Sistemas</p>
                        <p><strong>Departamento: </strong>Tecnologia da Informação</p>
                    </div>
                    <div style=""clear:both""></div>
                </div>
            <div style=""text-align: right; font-weight: bold;margin-top:2rem"">
                Piracicaba, 03 de abril de 2025
            </div>
            <p style=""text-align:justify"">Respeitosamente,</p>
                <div style=""text-align: center; margin-top:1.5cm;"">
                    <span style=""border-top: 1px solid #000;padding-top: 5px;"">
                        DOUGLAS MIRANDA DA SILVA
                    </span>
                    <br>Analista de Sistemas I
                </div>
        </div>
    
<script src=""./Solicitação de Inclusão PCA 2025_files/signalr.js.download""></script>
<script>
window.onload = function() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:44368/logoutHub')
        .configureLogging(signalR.LogLevel.None)
        .withAutomaticReconnect()
        .build();

    connection.on('LogoutMessage', () => {
        location.href = 'https://localhost:44368/logout';
    });

    connection.start().then(function () {
        //console.log('iniciou signalr');
    }).catch(function (err) {
        return console.error(err.toString());
    });
}
</script><script src=""./Solicitação de Inclusão PCA 2025_files/aspnetcore-browser-refresh.js.download""></script>

</body></html>"
).ConverterParaPdf();

byte[] rarr = File.ReadAllBytes("C:\\arquivos\\certificados\\ca.pfx");
Certificado raiz = new Certificado(rarr, "ET1w4VGjsRlFuyfUd5kbNamD8oZiXLBp");

Certificado keila = new Certificado(raiz, "TESTANDO CERTIFICADO", "keila.rodrigues@camarapiracicaba.sp.gov.br", "1234ab");
keila.SaveToDisk("C:\\arquivos\\certificados\\keila.pfx");

documento
    //.Juntar("C:\\arquivos\\testepdf\\baiao.pdf")
    .Numerar()
    .AdicionarMetadado(new Metadado("Nome 4", "Valor 4"))
    .TornarSemEfeito(new int[] { 255, 50, 50 }, 0.5f)
    //.TornarSemEfeito(new int[] { 50, 50, 50 }, 0.5f)
    //.Assinar(keila, 1, 320, 550, "LTA")
    .Protocolar("AAAAA")
    .Invalidar("MEU_TEXTO", new int[] { 0, 255, 0 })
    .Assinar(keila, 0)
    .Assinar(keila, 0, 0, 0, "B")
    .AdicionarDetalhesAoFinal("XXXXXXXX")
    //.ProtegerComSenha("fabio")
    .Salvar($"{path}{output}");



//documento.Salvar($"{path}{output}");


Adobe.Acrobat.AbrirAcrobat($"{path}{output}");





//Console.WriteLine(documento.OCR());

/*
List<AssinanteDocumento> assinantes = documento.Assinantes();

Console.WriteLine(documento.TestarValidadeDasAssinaturas(ValidacaoAssinatura.PARCIAL));

foreach(AssinanteDocumento assinante in assinantes)
{
    Console.WriteLine(assinante.Validacao.Valido);
    
    if (!assinante.Validacao.Valido)
    {
        Console.WriteLine(assinante.Validacao.Status[0]);
    }

    if (!assinante.ValidacaoCompleta.Valido)
    {
        Console.WriteLine(assinante.ValidacaoCompleta.Status[0]);
    }

}
*/

//Console.WriteLine(assinantes[1].Email);



//Console.WriteLine(documento.TemCarimboAntigo());

//documento.ObterFontesIncorporadas();

//CMP.ManipuladorPDFLegado.AdicionarAssinaturaLateral(string caminhoCertificado, string senha, MemoryStream sourceFile, string texto, string qrcode)

//documento = documento.IncorporarFonte("Helvetica");

//DocumentoPDF documento2 = new DocumentoPDF($"{path}{output}");

//var ass = documento2.CarimbosDoTempo();

//Console.WriteLine(ass[0].Nome);



//string str = File.ReadAllText("C:\\app\\compras-erro.html");

//str = str.Replace("%", "");

//DocumentoPDF documento = str.ConverterParaPdf();

//string file = "pdf20";
//string file = "new";

//DocumentoPDF documento = new DocumentoPDF($"{path}{file}.pdf");
//documento = documento.IncorporarFonte("Helvetica");

//documento = documento.ConverterParaPDFA();

/*
 byte[] rarr = File.ReadAllBytes("C:\\arquivos\\certificados\\ca.pfx");
Certificado raiz = new Certificado(rarr, "ET1w4VGjsRlFuyfUd5kbNamD8oZiXLBp");
Certificado keila = new Certificado(raiz, "TESTANDO CERTIFICADO", "keila.rodrigues@camarapiracicaba.sp.gov.br", "1234ab");
keila.SaveToDisk("C:\\arquivos\\certificados\\keila.pfx");
*/

//Certificado keila = new Certificado("C:\\arquivos\\certificados\\keila.pfx","1234ab");
//await keila.AdicionarOCSP();

//Certificado keila = new Certificado("CN=Fabio Cardoso, OU=Fabio Cardoso, O=Fabio Cardoso, L=Piracicaba, S=Sao Paulo, C=BR");
//await keila.AdicionarOCSP();

//AssinarPDFResponse npdf = CMP.ManipuladorPDFLegado.AssinarPDF.AdicionarAssinaturaLateral("C:\\arquivos\\certificados\\keila.pfx", "1234ab",documento.ConverterParaMemoryStream(),"QQ COISA","QQ COISA CODE");
//documento = new DocumentoPDF(npdf.PDFAssinado);

//documento = documento.AdicionarMetadado("NOME", "VALOR");
//documento = documento.AdicionarMetadado("NOME2", "VALOR2");

//List<Metadado> metadata = documento.ObtemMetadados();


//documento.ExtrairPagina(1).Salvar($"{path}{output}");

//DocumentoPDF documento = new DocumentoPDF($"{path}1.pdf");


/*
documento
    //.Juntar("C:\\arquivos\\testepdf\\baiao.pdf")
    .Numerar()
    //.AdicionarMetadado(new Metadado("Nome 4", "Valor 4"))
    //.TornarSemEfeito(new int[] { 255, 50, 50 }, 0.5f)
    //.TornarSemEfeito(new int[] { 50, 50, 50 }, 0.5f)
    //.Assinar(keila, 1, 320, 550, "LTA")
    //.Protocolar("AAAAA")
    //.Invalidar("MEU_TEXTO", new int[] { 0, 255, 0 })
    .Assinar(keila, 0)
    .Assinar(keila,0,0,0,"B")
    //.AdicionarDetalhesAoFinal("XXXXXXXX")
    //.ProtegerComSenha("fabio")
    .Salvar($"{path}{output}");



DocumentoPDF documento2 = new DocumentoPDF($"{path}{output}");
List<AssinanteDocumento> assinantes = documento2.Assinantes();


foreach (var assinante in assinantes)
{
    Console.WriteLine($"Nome: {assinante.Nome}");
    Console.WriteLine($"Email: {assinante.Email}");
    Console.WriteLine($"Razão: {assinante.Razao}");
    Console.WriteLine($"Data: {assinante.Data}");
    Console.WriteLine($"Emissor: {assinante.Emissor}");
    Console.WriteLine($"Tipo: {assinante.Tipo}");
    Console.WriteLine($"Validação: {assinante.Validacao}");
    Console.WriteLine(new string('-', 40));
}

Adobe.Acrobat.AbrirAcrobat($"{path}{output}");

*/
