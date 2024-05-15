using CMP.Certificados;
using CMP.ManipuladorPDF;

string path = "C:\\arquivos\\testepdf\\";
string output = "final.pdf";

//Certificado raiz = new Certificado("C:\\arquivos\\certificados\\ca.pfx", "C@m@r@1025");
//Certificado certificado = new Certificado(raiz, "KEILA CRISTINA DE OLIVEIRA BARBOSA RODRIGUES", "keila.rodrigues@camarapiracicaba.sp.gov.br", "1234ab");
//certificado.SaveToDisk("C:\\arquivos\\certificados\\keila4.pfx");

Certificado keila3 = new Certificado("C:\\arquivos\\certificados\\keila4.pfx","1234ab");

Adobe.Acrobat.FecharAcrobat();

DocumentoPDF documento = new DocumentoPDF($"{path}sample.pdf");

documento
    //.AdicionarMetadado("Name", "Value")
    //.Juntar("C:\\testepdf\\tese.pdf")
    //.Numerar()
    //.AdicionarMetadado(new Metadado("Nome 4", "Valor 4"))
    //.Protocolar("99/2024", DateTime.Now, "X0X0X0X0")
    //.TornarSemEfeito()
    //.Assinar(raiz, 1, 20, 770)
    .Assinar(keila3, 1, 20, 770)
    //.AdicionarDetalhesAoFinal("JAHSOMWE")
    .Salvar($"{path}{output}");

Adobe.Acrobat.AbrirAcrobat($"{path}{output}");