using Assinador;
using ConversorHTML;

Console.WriteLine("Hello, World!");

MemoryStream ms = new MemoryStream();

//await File.OpenRead("D:\\Desktop\\umapaginaapenas_assinado.pdf").CopyToAsync(ms);
//await File.OpenRead("C:\\Users\\0308\\Desktop\\Outros\\teste.pdf").CopyToAsync(ms);

//var onlyOnePage = ms.RetornarApenasUmaPaginaPDF(2);
//await File.WriteAllBytesAsync("D:\\Desktop\\umapaginaapenas.pdf", onlyOnePage.ToArray());
//var numerado = HtmlToPDF.NumerarPDF(ms);
//var numerado = HtmlToPDF.NumerarPDF("https://localhost:44368/arquivos/solicitacaocompra/12/a191d39f-cac7-4e45-84f3-4cb79eea2002.pdf");
//await File.WriteAllBytesAsync("C:\\Users\\0308\\source\\repos\\m-marco\\CMP.ManipuladorPDF\\numerado.pdf", numerado.ToArray());


//var retorno = AssinarPDF.Sign("C:\\Users\\0308\\Desktop\\Git\\CVP.Materiais\\certificados\\teste.p12",
//    "123456", "C:\\Users\\0308\\Desktop\\relatorio_assinado.pdf");

//await File.WriteAllBytesAsync("C:\\Users\\0308\\Desktop\\umapaginaapenas_assinado.pdf", retorno.ToArray());

//retorno = AssinarPDF.Sign("C:\\arquivos\\certificados\\camara.pfx",
//    "C@m@r@1025", "C:\\Users\\0308\\Desktop\\umapaginaapenas_assinado.pdf",null,30,200);

////C

//////var retorno = AssinarPDF.Sign("D:\\Desktop\\Git\\CMP.Compras\\certificados\\teste.pfx",
//////    "teste", ms);
////try
////{
//await File.WriteAllBytesAsync("C:\\Users\\0308\\Desktop\\umapaginaapenas_assinado2.pdf", retorno.ToArray());

var retorno = new List<string>() { "D:\\Desktop\\Outros\\0093_230210154749_001.pdf",
    "D:\\Desktop\\Outros\\2023-02-Fevereiro_assinado.pdf",
    "D:\\Desktop\\Outros\\Controle_Interno.pdf"}.JuntarArquivosPDF();

await File.WriteAllBytesAsync("D:\\Desktop\\juntado.pdf", retorno.ToArray());

retorno = "D:\\Desktop\\Outros\\0093_230210154749_001.pdf".AdicionarRodape("Cópia de documento assinado digitalmente");

await File.WriteAllBytesAsync("D:\\Desktop\\juntado.pdf", retorno.ToArray());

//}
//catch (Exception ex)
//{

//    throw;
//}

