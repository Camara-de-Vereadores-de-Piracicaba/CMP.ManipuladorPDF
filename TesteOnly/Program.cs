﻿using Assinador;
using ConversorHTML;

Console.WriteLine("Hello, World!");

MemoryStream ms = new MemoryStream();

//await File.OpenRead("D:\\Desktop\\umapaginaapenas_assinado.pdf").CopyToAsync(ms);
//await File.OpenRead("C:\\Users\\0308\\Desktop\\Outros\\teste.pdf").CopyToAsync(ms);

//var onlyOnePage = ms.RetornarApenasUmaPaginaPDF(2);
//await File.WriteAllBytesAsync("D:\\Desktop\\umapaginaapenas.pdf", onlyOnePage.ToArray());
//var numerado = HtmlToPDF.NumerarPDF(ms);
//await File.WriteAllBytesAsync("C:\\Users\\0308\\source\\repos\\m-marco\\CMP.ManipuladorPDF\\numerado.pdf", numerado.ToArray());


var retorno = AssinarPDF.Sign("D:\\Desktop\\Git\\CMP.Compras\\certificados\\teste.p12",
    "123456", "D:\\Desktop\\umapaginaapenas_assinado.pdf");

////var retorno = AssinarPDF.Sign("D:\\Desktop\\Git\\CMP.Compras\\certificados\\teste.pfx",
////    "teste", ms);
//try
//{
await File.WriteAllBytesAsync("D:\\Desktop\\umapaginaapenas_assinado.pdf", retorno.ToArray());

//}
//catch (Exception ex)
//{

//    throw;
//}

