using CMP.Certificados;
using CMP.ManipuladorPDF;

//DocumentoPDFConfig.DefinirDiretorioDeFontes();

string path = "C:\\arquivos\\testepdf\\";
string output = "final.pdf";

DocumentoPDF documento = "<p style=\"font-family:sans-serif\">aaa</p><h1>A</h1><strong><em>a</em></strong><em>a</em>".ConverterParaPdf();
//DocumentoPDF documento = new DocumentoPDF($"{path}sample.pdf");

Certificado raiz = new Certificado("C:\\Users\\0354\\Desktop\\x\\x2\\GeradorCertificados\\ca\\ca.pfx", "ET1w4VGjsRlFuyfUd5kbNamD8oZiXLBp");
Certificado certificado = new Certificado(raiz, "KEILA CRISTINA DE OLIVEIRA BARBOSA", "keila.rodrigues@camarapiracicaba.sp.gov.br", "1234ab");
certificado.SaveToDisk("C:\\arquivos\\certificados\\keila.pfx");

Certificado keila = new Certificado("C:\\arquivos\\certificados\\keila.pfx","1234ab");
await keila.AdicionarOCSP();

Adobe.Acrobat.FecharAcrobat();

documento
    .AdicionarMetadado("Name", "Value")
    //.Juntar("C:\\testepdf\\tese.pdf")
    //.Numerar()
    .AdicionarMetadado(new Metadado("Nome 4", "Valor 4"))
    //.Protocolar("99/2024", DateTime.Now, "X0X0X0X0")
    //.TornarSemEfeito()
    .Assinar(keila, 1, 20, 770)
    //.AdicionarDetalhesAoFinal("JAHSOMWE")
    .Salvar($"{path}{output}");

Adobe.Acrobat.AbrirAcrobat($"{path}{output}");
