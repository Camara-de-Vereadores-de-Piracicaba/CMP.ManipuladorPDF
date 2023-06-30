using Assinador;

Console.WriteLine("Hello, World!");

MemoryStream ms = new MemoryStream();

await File.OpenRead("D:\\Desktop\\MINHAS_VANTAGENS.pdf").CopyToAsync(ms);

var retorno = await AssinarPDF.Sign("D:\\Desktop\\Git\\CMP.Compras\\certificados\\teste.p12",
    "123456", ms);
await File.WriteAllBytesAsync("D:\\Desktop\\signed.pdf", retorno.ToArray());

