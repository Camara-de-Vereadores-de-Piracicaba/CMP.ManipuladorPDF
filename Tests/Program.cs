using CMP.Certificados;
using CMP.ManipuladorPDF;

//DocumentoPDFConfig.DefinirDiretorioDeFontes();

Adobe.Acrobat.FecharAcrobat();

string path = "C:\\arquivos\\testepdf\\";
string output = "simples.pdf";

//byte[] arr = File.ReadAllBytes($"{path}baiao.pdf");
//byte[] arr = File.ReadAllBytes($"{path}despacho.pdf");

DocumentoPDF documento = new TemplateHtml(
    @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title>SOLICITA&#xC7;&#xC3;O DE NOVA RESERVA PARA A REQUISI&#xC7;&#xC3;O DE COMPRAS</title>
    <link rel=""stylesheet"" href=""/lib/font-awesome/css/all.min.css"" />
    <link rel=""manifest"" href=""/manifest.json"">
            <style scoped>* {
    font-size: 0.9rem !important;
}

body {
    margin: 0;
}

page {
    background-color: white;
    display: block;
    padding: 0;
    margin: 0;
    box-sizing: border-box;
}

    page[size=""A4""] {
        width: auto;
        height: auto;
        margin: 0 auto;
    }

table {
    word-break: break-word;
    word-wrap: break-word;
    width: 650;
    min-width: 650;
    max-width: 650;
    text-align: left;
    box-sizing: border-box;
    padding: 0;
    margin: 0;
}

p, li {
    text-align: justify;
}

td[colspan=""2""] > table {
    border-collapse: collapse;
}

    td[colspan=""2""] > table td, td[colspan=""2""] > table th {
        padding: 2.5;
    }

    td[colspan=""2""] > table th {
        white-space: nowrap;
    }

    td[colspan=""2""] > table:not(.border-none) {
        border: 1 solid #cccccc;
        margin-bottom: 2.5;
    }

table tr:only-child th:only-child:empty {
    display: none;
}

table table thead th, table table thead + tbody td {
    border: 1 solid #cccccc;
    text-align: center;
}

th:not(:empty):only-child {
    background-color: #cccccc;
    padding: 2.5;
}

th + td {
    border: none !important;
}

h1 {
    margin: 0;
    text-align: center;
}

h2, h3, h4, h5 {
    margin: 0;
    text-align: left;
}

h4, h3 {
    padding-left: 1rem;
}

h3, h4, h5 {
    font-weight: normal;
}

img.logo {
    max-height: 115;
}

th:empty {
    padding: 0 !important;
    margin: 0 !important;
}

#cabecalho tr th:first-child {
    width: auto !important;
}
</style>
</head>
<body>
    <page size=""A4"">
        <table id=""main"" style=""text-align: center;"">
            <thead id=""cabecalho""> 
                <tr>
                    <th style=""width: 30%; text-align: right""><img src=""https://sistemas.camarapiracicaba.sp.gov.br/arquivos/imagens/brasao_camara.png""  height=""80"" /></th>
                    <th style=""padding-bottom:0.4rem; text-align: center"">
                        <h3><strong>CÂMARA MUNICIPAL DE PIRACICABA</strong></h3>
                        <h4>Rua Alferes José Caetano, 834, Centro - Piracicaba/SP CEP 13400-120</h4>
                        <h4>CNPJ: 51.327.708/0001-92 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Inscrição Estadual: isento</h4>
                        <h4>Telefone: (19) 3403-6561 / (19) 3403-6529</h4>
                        <h4>E-mail: compras@camarapiracicaba.sp.gov.br</h4>
                    </th>
                </tr>
            </thead>
            <tbody>
                <tr>
                        <td colspan=""2"" style=""background-color: #cccccc"">
                            <h2 style=""padding:0.2rem 0;font-size:0.5rem;margin:0;text-align:center;"">SOLICITAÇÃO DE NOVA RESERVA PARA A REQUISIÇÃO DE COMPRAS</h2>
                        </td>
                </tr>
                <tr>
                    <td colspan=""2"">
                        <table class='border-none'><tr><th style='background-color: #ccc;'></th></tr></table><table><thead><tr><th style='background-color: : ; '>Requisições de Compra</th><th style='background-color: : ; '>Número do Processo</th><th style='background-color: : ; '>Modalidade</th></tr></thead><tbody><tr><td style='background-color: : ; '>68.2026.1.81-RC1 - 29/01/2026</td><td style='background-color: : ; '>68.2026.1.81</td><td style='background-color: : ; '>Dispensa</td> </tr></tbody></table><table>
                            <tr>
                                <th style='width: 35%; text-align: ;'>Discriminação do Pedido:</th>
                                <td>O Setor de Compras e Contratos vem por meio desta,
                solicitar que sejam reservados os valores abaixo citados para
                os seguintes itens deste Processo de Compras.</td>
                            </tr>
                        </table><table class='border-none'><tr><th style='background-color: #ccc;'>Financeiro</th></tr></table><table><thead><tr><th style='background-color: : ; '>Ficha</th><th style='background-color: : ; '>Programa de Trabalho</th><th style='background-color: : ; '>Natureza da Despesa</th><th style='background-color: : ; '>Tipo de Custo</th><th style='background-color: : ; '>Valor</th></tr></thead><tbody><tr><td style='background-color: : ; '>64</td><td style='background-color: : ; '>01031001821010000 <br />Gestão Administrativa da Unidade</td><td style='background-color: : ; '>339030 <br />MATERIAL DE CONSUMO</td><td style='background-color: : ; '>Exercício</td><td style='background-color: : ; white-space: nowrap;'>R$ 726,00</td> </tr></tbody></table><table class='border-none'><tr><th style='background-color: #ccc;'>Itens</th></tr></table><table><thead><tr><th style='background-color: : ; '>#</th><th style='background-color: : ; '>Item</th><th style='background-color: : ; '>Unidade Compra</th><th style='background-color: : ; '>Quantidade</th><th style='background-color: : ; '>Vl. Unitário</th></tr></thead><tbody><tr><td style='background-color: : ; '>1</td><td style='background-color: : ; '>M48.08 - MINIBOMBA PRESSURIZADORA DE ÁGUA </td><td style='background-color: : ; '>UN</td><td style='background-color: : ; '>1,00</td><td style='background-color: : ; white-space: nowrap;'>R$ 612,50</td> </tr><tr><td style='background-color: : ; '>2</td><td style='background-color: : ; '>M47.05 - FILTRO Y </td><td style='background-color: : ; '>UN</td><td style='background-color: : ; '>1,00</td><td style='background-color: : ; white-space: nowrap;'>R$ 113,50</td> </tr></tbody></table><table class='border-none'><tr><th style='background-color: #ccc;'>Informações da Reserva</th></tr></table><table><thead><tr><th style='background-color: : ; '>Código da Reserva</th><th style='background-color: : ; '>Tipo de Solicitação</th><th style='background-color: : ; '>Valor a Reservar</th><th style='background-color: : ; '>Valor a Anular</th></tr></thead><tbody><tr><td style='background-color: : ; '></td><td style='background-color: : ; '>Nova</td><td style='background-color: : ; '>R$ 726,00</td><td style='background-color: : ; '>-</td> </tr></tbody></table><table>
                            <tr>
                                <th style='width: 35%; text-align: ;'>Observações:</th>
                                <td>Nada consta.</td>
                            </tr>
                        </table><table>
                            <tr>
                                <th style='width: 35%; text-align: ;'>Aplicação do Material/Serviço:</th>
                                <td>68.2026.1.81-RC1: A aquisição de um pressurizador de água se faz necessária para garantir a adequada pressão e regularidade no abastecimento hidráulico do 2º andar do prédio principal. O sistema atual apresenta oscilações e baixa pressão, comprometendo o funcionamento adequado das torneiras e descargas. Abaixa pressão de água, especialmente na descarga, tem causado alguns transtornos para seus usuários.</td>
                            </tr>
                        </table>
                    </td>
                </tr>
                    <tr>
                        <td colspan=""2"" style=""border:none!important;"">
                            <table style=""border:none;"">
                                <tr>
                                    <td style=""text-align: right; font-weight: bold;"">
                                        Piracicaba, 11 de fevereiro de 2026
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                <tr>
                    <td colspan=""2"" style=""border:none!important"">
                        <table style=""border:none;"">
                            <tr style=""font-weight: bold; text-align: center"">
                                    <td style=""text-align: center; padding-bottom: 1rem; text-wrap: nowrap; break-inside: avoid;"">
                                        <div style=""border: 1px dashed #ccc; width: 250px; height: 50px; margin: 0 auto; border-bottom: 1px solid #000; margin-bottom: 10px""></div>
                                        Marco Antonio Pereira Junior<br>Chefe do Setor de Desenv. de Sistemas   
                                    </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </tbody>
        </table>
    </page>
    <div id=""modal""></div>
</body>
</html>
"
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
