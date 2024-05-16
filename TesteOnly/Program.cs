using CMP.Certificados;
using CMP.ManipuladorPDF;

string path = "C:\\arquivos\\testepdf\\";
string output = "final.pdf";

DocumentoPDF documento = "<h1 style=\"font-family:Aptos\">TESTE</h1>".ConverterParaPdf();
//DocumentoPDF documento = new DocumentoPDF($"{path}sample.pdf");

Certificado raiz = new Certificado(PadroesCertificado.CaminhoCA, PadroesCertificado.SenhaCA);
Certificado certificado = new Certificado(raiz, "KEILA CRISTINA DE OLIVEIRA BARBOSA RODRIGUES", "keila.rodrigues@camarapiracicaba.sp.gov.br", "1234ab");
//certificado.SaveToDisk("C:\\arquivos\\certificados\\keila.pfx");

Certificado keila = new Certificado("C:\\arquivos\\certificados\\keila.pfx","1234ab");

Adobe.Acrobat.FecharAcrobat();

documento
    .AdicionarMetadado("Name", "Value")
    //.Juntar("C:\\testepdf\\tese.pdf")
    .Numerar()
    .AdicionarMetadado(new Metadado("Nome 4", "Valor 4"))
    .Protocolar("99/2024", DateTime.Now, "X0X0X0X0")
    .TornarSemEfeito()
    .Assinar(keila, 1, 20, 770)
    //.AdicionarDetalhesAoFinal("JAHSOMWE")
    .Salvar($"{path}{output}");

Adobe.Acrobat.AbrirAcrobat($"{path}{output}");