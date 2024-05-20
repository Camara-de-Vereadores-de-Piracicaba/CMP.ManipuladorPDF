using CMP.Certificados;
using CMP.ManipuladorPDF;

string path = "C:\\arquivos\\testepdf\\";
string output = "final.pdf";

DocumentoPDF documento = "<html style=\"width:100%\"><body style=\"width:100%\"><table style=\"font-family:Aptos\" cellpadding=\"12\" cellspacing=\"0\" border=\"0\" style=\"border-collapse: collapse; width: 100%;\"> \r\n    <tbody> \r\n        <tr> \r\n            <td style=\"border-color: rgb(0, 0, 0);background: #837f7f;border: 0;width:25%;\"></td>\r\n            <td style=\"text-align: center;border-color: rgb(0, 0, 0);border: 0;width:50%;\"><b>PROCESSO ADMINISTRATIVO</b></td>\r\n            <td style=\"border-color: rgb(0, 0, 0);background: #837f7f;border: 0;width:25%;\"></td>\r\n        </tr>\r\n    </tbody>\r\n</table></body></html>".ConverterParaPdf();
//DocumentoPDF documento = new DocumentoPDF($"{path}sample.pdf");

Certificado raiz = new Certificado("C:\\Users\\0354\\Desktop\\x\\x2\\GeradorCertificados\\ca\\ca.pfx", "ET1w4VGjsRlFuyfUd5kbNamD8oZiXLBp");
Certificado certificado = new Certificado(raiz, "KEILA CRISTINA DE OLIVEIRA BARBOSA RODRIGUES", "keila.rodrigues@camarapiracicaba.sp.gov.br", "1234ab");
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
    //.Assinar(keila, 1, 20, 770)
    //.AdicionarDetalhesAoFinal("JAHSOMWE")
    .Salvar($"{path}{output}");

Adobe.Acrobat.AbrirAcrobat($"{path}{output}");
