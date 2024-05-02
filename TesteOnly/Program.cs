using CMP.ManipuladorPDF;
using System.Diagnostics;
using static CMP.ManipuladorPDF.ExtensionMethods;

Console.WriteLine("");

var acrobat = Process.GetProcesses();
foreach (var process in acrobat)
{
    if (process.ProcessName == "Acrobat")
    {
        process.Kill();
    }
}

DocumentoPDF documento = new DocumentoPDF("C:\\testepdf\\multipage.pdf");

documento
    .AdicionarMetadado("Name", "Value")
    .Juntar("C:\\testepdf\\tese.pdf")
    .Numerar()
    .AdicionarMetadado(new Metadado("Nome 4", "Valor 4"))
    .Protocolar("99/2024", DateTime.Now, "X0X0X0X0")
    .TornarSemEfeito()
    .Assinar("C:\\testepdf\\fabio.pfx", "1234ab", 2, 0, 0)
    .AdicionarDetalhesAoFinal("JAHSOMWE")
    .Salvar("C:\\testepdf\\com_metadados.pdf");
/*

var acrobat = Process.GetProcesses();

foreach (var process in acrobat)
{
    if (process.ProcessName == "Acrobat")
    {
        process.Kill();
    }
}

RequisicaoCertificado requisicao = new()
{
    CertificadoRaiz = File.ReadAllBytes("C:\\testepdf\\camara.pfx"),
    SenhaRaiz = "C@m@r@1025",
    Nome = "Usuário De Teste",
    Senha = "1234",
    Email = "usuario.de.teste@camarapiracicaba.sp.gov.br"
};

File.WriteAllBytes("C:\\testepdf\\certificado.pfx", GerarCertificado(requisicao));

MemoryStream ms = new(File.ReadAllBytes("C:\\testepdf\\tese.pdf"));

List<Metadado> metadados = new()
{
    new ("Metadado1", "VALOR METADADO 1"),
    new ("Metadado2", "OUTRO METADADO")
};

//MemoryStream comMetadados = AdicionarMetadados(ms, metadados);

MemoryStream comFinal = ManipuladorPDF.AdicionarPaginaFinal(ms);

File.WriteAllBytes("C:\\testepdf\\example_output.pdf", comFinal.ToArray());


/*
Certificado certificado = ObterCertificado("C:\\testepdf\\certificado.pfx", "1234");

DadosAssinatura dadosAssinatura = new()
{
    Posicao = PosicaoAssinatura.LIVRE,
    X = 54,
    Y = 720,
    Nome = "Usuário De Teste",
    Protocolo = "XOXOXOXO",
    QRCode = false,
    EnderecoValidacao = true
};

Assinatura assinatura = new()
{
    Dados = dadosAssinatura,
    CarimboLateral = true
};

//MemoryStream assinado = AssinarPDF(comFinal, certificado, assinatura);
MemoryStream assinado = AssinarPDF(comMetadados, certificado, assinatura);

//AdicionarPaginaFinal(assinado);

File.WriteAllBytes("C:\\testepdf\\example_output.pdf", assinado.ToArray());
*/

Process.Start("C:\\Program Files\\Adobe\\Acrobat DC\\Acrobat\\Acrobat.exe", "C:\\testepdf\\com_metadados.pdf");