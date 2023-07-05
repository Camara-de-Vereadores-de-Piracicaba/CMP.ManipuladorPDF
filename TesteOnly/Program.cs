using Assinador;
using ConversorHTML;

Console.WriteLine("Hello, World!");

MemoryStream ms = new MemoryStream();

//await File.OpenRead("D:\\Desktop\\MINHAS_VANTAGENS.pdf").CopyToAsync(ms);
await File.OpenRead("C:\\Users\\0308\\Desktop\\Outros\\teste.pdf").CopyToAsync(ms);

var numerado = HtmlToPDF.NumerarPDF(ms);
await File.WriteAllBytesAsync("C:\\Users\\0308\\source\\repos\\m-marco\\CMP.ManipuladorPDF\\numerado.pdf", numerado.ToArray());


var retorno = AssinarPDF.Sign("D:\\Desktop\\Git\\CMP.Compras\\certificados\\teste.p12",
    "123456", ms);

//var retorno = AssinarPDF.Sign("D:\\Desktop\\Git\\CMP.Compras\\certificados\\teste.pfx",
//    "teste", ms);
try
{
    await File.WriteAllBytesAsync("D:\\Desktop\\signed.pdf", retorno.ToArray());

}
catch (Exception ex)
{

    throw;
}

