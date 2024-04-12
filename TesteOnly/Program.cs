using CMP.ManipuladorPDF;
using System.Diagnostics;
using static CMP.ManipuladorPDF.ManipuladorPDF;

//var response = AssinarPDF.Sign("", "", "C:\\Users\\0308\\Desktop\\Calendario-Matricula.pdf");

//var response = HtmlToPDF.ConvertToPDF(html);

//var response = AssinarPDF.Sign("C:\\arquivos\\certificados\\camara.pfx", "C@m@r@1025", "C:\\Users\\0308\\Desktop\\teste-a.pdf");
//var assinaturas = AssinarPDF.ObterAssinaturas("C:\\Users\\0308\\Desktop\\teste-a-assinado.pdf");

//var arquivo = File.OpenRead("C:\\Users\\0308\\Desktop\\Solicitacao-Reserva_assinada.pdf");
//var ms = new MemoryStream();
//arquivo.CopyTo(ms);
//var resposta = ms.GetDigitalSignatures();
//var response = ManipuladorPDF.GetDigitalSignatures(ms);
//ManipuladorPDF.GetDigitalSignatures("D:\\Desktop\\assinado.pdf");

//File.WriteAllBytes("C:\\Users\\0308\\Desktop\\html-to-pdf.pdf", response.ToArray());

//var httpClientHandler = new HttpClientHandler();
//httpClientHandler.ServerCertificateCustomValidationCallback += (message, cert, chain, errors) =>
//{
//    return true;
//};

//var AccessToken= "eyJhbGciOiJSUzI1NiIsInR5cCI6IkJlYXJlciJ9.eyJGdW5jaW9uYXJpbyI6IkZ1bmNpb25hcmlvIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Ik1hcmNvIEFudG9uaW8gUGVyZWlyYSBKdW5pb3IiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6MjQsIk1hdHLDrWN1bGEiOiIwMzA4IiwiaXNzIjoiQ01QLlNTTyIsImF1ZCI6IkRvY3VtZW50byBEaWdpdGFsIiwiZXhwIjoxNzA1NzE5NjAwLCJpYXQiOjE3MDU2NjU1OTYsIm5iZiI6MTcwNTY2NTU5Nn0.TI-AOzZFYTXbfzu0emDja5F_ZKfo-O4_ThkRsPxFVEfGyaCOqEq8ZqKeQsc-mdoEnVeNS1ZT98650wLwHutEL1MScWDgpH_mVZbeH6AmIudcyLLARgx7MQnbQOTy_ukFSKFWkfj1B9Bg9g6nDtSievp8raguUSFd5QNAjD2cNXUiRBA52Tt8QBQTUgw--a0Ncm0Twkxq5TE3HKIrgk3oggCmL0XDkEGRacydgwHLPcKwpAV87ZrXowxpiHl5Abx8XPfkKVEmGY5X-HTo6otWEQDFdAJ-ELkTUNSJyLwz6abqMp4u1fzYvGrow6PjHrE1mPcjSSDgCUV6hFsaf81EmA";

//var _httpClient = new HttpClient(httpClientHandler);
//_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

//var UrlDownloadDocumento = "https://localhost:44302/arquivo/1027/dad55f64-4bcf-43fd-9738-a67cd390bfbd.pdf";

//var responseArquivoParaAssinar = await _httpClient.GetAsync(UrlDownloadDocumento);
//var arquivoParaAssinar = await responseArquivoParaAssinar.Content.ReadAsByteArrayAsync();

/*
var acrobat = Process.GetProcesses();
foreach (var process in acrobat) {
    if (process.ProcessName == "Acrobat") {
        process.Kill();
    }
}

var file = File.ReadAllBytes("C:\\testepdf\\sample.pdf");

var msParaAssinar = new MemoryStream(file);

//var retorno = AssinarPDF.Sign("C:\\testepdf\\0308.pfx", "xy234786", msParaAssinar, y: 0, x: 0, page: 1,rotate: 1);

var retorno = AssinarPDF.AdicionarAssinaturaLateral("C:\\testepdf\\0308.pfx", "xy234786", "C:\\testepdf\\sample.pdf", "QUALQUER COISA","1111");

File.WriteAllBytes("C:\\testepdf\\new.pdf", retorno.PDFAssinado.ToArray());


Process.Start("C:\\Program Files\\Adobe\\Acrobat DC\\Acrobat\\Acrobat.exe","C:\\testepdf\\new.pdf");

*/

var acrobat = Process.GetProcesses();
foreach (var process in acrobat) 
{
    if (process.ProcessName == "Acrobat") 
    {
        process.Kill();
    }

}

var file = File.ReadAllBytes("C:\\testepdf\\apresentacao.pdf");
var ms = new MemoryStream(file);

//var sms = ManipuladorPDF.AdicionarRodape(ms,"TEXTO NO RODAPÉ");

//var cms = new MemoryStream(ManipuladorPDF.GerarCertificado());
//File.WriteAllBytes("C:\\testepdf\\certificadofabio.pfx", cms.ToArray());

var lista = new List<Metadado>();

lista.Add(new Metadado() { Nome = "Title", Valor = "NOVO TÍTULO" });
lista.Add(new Metadado() { Nome = "Author", Valor = "NOVO AUTOR" });
lista.Add(new Metadado() { Nome = "Subject", Valor = "NOVO ASSUNTO" });
lista.Add(new Metadado() { Nome = "Keywords", Valor = "lista, de, keywords compostas, separadas, por, vírgula" });
lista.Add(new Metadado() { Nome = "Producer", Valor = "Câmara Municipal de Piracicaba" });
lista.Add(new Metadado() { Nome = "Creator", Valor = "CMP" });
lista.Add(new Metadado() { Nome = "CUSTOM", Valor = "çççççç'\"aááááááaáááá" });

var md = AdicionarMetadados(ms, lista);
var nms = new MemoryStream(md.ToArray());

var sms = AssinarPDF(
    nms,
    "C:\\testepdf\\certificadofabio.pfx",
    "teste123",
    new Assinatura()
    {
        Pagina = 1,
        CarimboLateral = true,
        Dados = new DadosAssinatura()
        {
            Nome = "Keila Cristina de Oliveira Barbosa Rodrigues",
            Protocolo = "289FE2E6",
            Posicao = PosicaoAssinatura.LIVRE,
            EnderecoValidacao = true,
            X = 70,
            Y = 300
        }
    }
);

File.WriteAllBytes("C:\\testepdf\\testeassinado.pdf", sms.ToArray());
Process.Start("C:\\Program Files\\Adobe\\Acrobat DC\\Acrobat\\Acrobat.exe", "C:\\testepdf\\testeassinado.pdf");

/*

var xms = new MemoryStream(sms.ToArray());

var mdd = ObterMetadados(xms);

foreach (var m in mdd)
{
    Console.WriteLine(m.Nome + "->" + m.Valor);
}

File.WriteAllBytes("C:\\testepdf\\testeassinado.pdf", xms.ToArray());

var file2 = File.ReadAllBytes("C:\\testepdf\\testeassinado.pdf");
var ms2 = new MemoryStream(file2);

var lista2 = new List<Metadado>();

lista2.Add(new Metadado() { Nome = "Title", Valor = "NOVO TÍTULO DEPOIS ASSINAR" });
lista2.Add(new Metadado() { Nome = "CUSTOM", Valor = "TESTE INVALIDO" });
lista2.Add(new Metadado() { Nome = "CUSTOM2", Valor = "TESTE INVALIDO 2" });

//var md2 = AdicionarMetadados(ms2, lista2);

File.WriteAllBytes("C:\\testepdf\\metadadodepois.pdf", ms2.ToArray());

//var sms = ManipuladorPDF.Numerar(ms, ManipuladorPDFNumberPosition.TOP_RIGHT);
//var sms = ManipuladorPDF.TornarSemEfeito(ms,"INVÁLIDO");

//File.WriteAllBytes("C:\\testepdf\\semefeito.pdf", sms.ToArray());

//Process.Start("C:\\Program Files\\Adobe\\Acrobat DC\\Acrobat\\Acrobat.exe", "C:\\testepdf\\testeassinado.pdf");
Process.Start("C:\\Program Files\\Adobe\\Acrobat DC\\Acrobat\\Acrobat.exe", "C:\\testepdf\\metadadodepois.pdf");

/*
var file = File.ReadAllBytes("C:\\testepdf\\assinado.pdf");
var ms = new MemoryStream(file);

var signatures = ManipuladorPDF.GetDigitalSignatures("C:\\testepdf\\assinado.pdf");
//var signatures=ManipuladorPDF.GetDigitalSignatures(ms);

foreach (var signature in signatures) {
    Console.WriteLine(signature.CertificadoAssinante);
}
*/