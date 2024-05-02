using CMP.ManipuladorPDF;


string path = "C:\\testepdf\\";
string output = "final.pdf";
string certificate = "C:\\testepdf\\fabio.pfx";
string password = "1234ab";

Adobe.Acrobat.FecharAcrobat();

DocumentoPDF documento = new DocumentoPDF($"{path}tese.pdf");

documento
    .AdicionarMetadado("Name", "Value")
    .Juntar("C:\\testepdf\\tese.pdf")
    .Numerar()
    .AdicionarMetadado(new Metadado("Nome 4", "Valor 4"))
    .Protocolar("99/2024", DateTime.Now, "X0X0X0X0")
    .TornarSemEfeito()
    .Assinar(certificate, password,2,100,300)
    //.AdicionarDetalhesAoFinal("JAHSOMWE")
    .Salvar($"{path}{output}");

Adobe.Acrobat.AbrirAcrobat($"{path}{output}");