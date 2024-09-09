using iText.Kernel.Pdf;
using QRCoder;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using SixLabors.ImageSharp;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF PaginaFinal(
            DocumentoPDF documento,
            string hash = null,
            List<AssinanteDocumento> assinantes = null
        )
        {

            //documento = documento.DesencriptarCasoNecessario();

            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument newPdfDocument = new PdfDocument(pdfWriter);
            using PdfReader pdfReader = new PdfReader(new MemoryStream(documento.ByteArray));

            if (DocumentoPDFConfig.UNETHICAL_READING)
                pdfReader.SetUnethicalReading(true);
            
            PdfDocument pdfDocument = new PdfDocument(pdfReader);

            TimeZoneInfo fuso = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

            List<AssinanteDocumento> _signatures = documento.Assinantes().GroupBy(s => s.Email).Select(s => s.First()).ToList();

            string title = "Este documento foi assinado digitalmente pelos seguintes signatários:";

            if (assinantes != null)
            {
                title = "Estes documentos foram assinados digitalmente pelos seguintes signatários:";
                _signatures = assinantes;
            }

            string signatures = "";
            string lsv = "";
            foreach (AssinanteDocumento signature in _signatures)
            {

                string docTitle = "";

                if (assinantes != null && signature.Documento.Titulo != null)
                {
                    docTitle = $"<h2>{signature.Documento.Titulo}</h2>";
                }

                string date = signature.Data;
                signatures += $@"
                    <div class=""signature"">
                    <div class=""signature-icon"">
                        <svg xmlns=""http://www.w3.org/2000/svg"" height=""24"" viewBox=""0 -960 960 960"" width=""24""><path d=""M563-491q73-54 114-118.5T718-738q0-32-10.5-47T679-800q-47 0-83 79.5T560-541q0 14 .5 26.5T563-491ZM120-120v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80ZM136-280l-56-56 64-64-64-64 56-56 64 64 64-64 56 56-64 64 64 64-56 56-64-64-64 64Zm482-40q-30 0-55-11.5T520-369q-25 14-51.5 25T414-322l-28-75q28-10 53.5-21.5T489-443q-5-22-7.5-48t-2.5-56q0-144 57-238.5T679-880q52 0 85 38.5T797-734q0 86-54.5 170T591-413q7 7 14.5 10.5T621-399q26 0 60.5-33t62.5-87l73 34q-7 17-11 41t1 42q10-5 23.5-17t27.5-30l63 49q-26 36-60 58t-63 22q-21 0-37.5-12.5T733-371q-28 25-57 38t-58 13Z""/></svg>
                    </div>
                    <div class=""signature-info"">
                        {docTitle}
                        <h1>{signature.Nome}</h1>
                        <h2>{signature.Email}</h2>
                        <h4>Assinado no dia {date}</h4>
                    </div>
                </div>";
            }

            pdfDocument.CopyPagesTo(1, pdfDocument.GetNumberOfPages(), newPdfDocument);
            pdfDocument.Close();

            string validadorUrl = $"{documento.ValidadorURL}";
            string hasHash = ".";

            if (hash != null)
            {
                validadorUrl = $"{documento.ValidadorURL}/{hash}";
                hasHash = $" e informe o código <b>{hash}</b>.";
            }

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(validadorUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode _qrCode = new QRCode(qrCodeData);
            Image qrCodeImage = _qrCode.GetGraphic(4);
            MemoryStream ms = new MemoryStream();
            qrCodeImage.SaveAsPng(ms);
            string qrCode = Convert.ToBase64String(ms.ToArray());

            string html = $@"
                <style>
                    *{{
                        font-family:'Aptos';
                        margin:0;
                        padding:0;
                        box-sizing:border-box;
                    }}
                    @page{{
                        margin-left:2cm;
                        margin-right:2cm;
                        margin-top:1.5cm;
                        margin-bottom:2cm;
                    }}
                    html,body{{
                        height:100%;
                    }}
                    body{{
                        display:flex;
                        flex-direction:column;
                    }}
                    #header{{
                        display:block;
                    }}
                    #header img{{
                        width:8cm;
                    }}
                    #main{{
                        display:block;
                        flex:1;
                    }}
                    #footer{{
                        position:absolute;
                        width:17cm;
                        left:-1cm;
                        bottom:-1.5cm;
                        display:flex;
                        align-items:flex-end;
                    }}
                    #qrcode{{
                        height:2cm;
                        margin-right:0.1cm;
                    }}
                    #info{{
                        display:flex;
                        align-items:flex-end;
                        font-size:0.3cm;
                        padding-bottom:0.15cm;
                    }}
                    .title{{
                        font-size:0.45cm;
                        text-transform:uppercase;
                        margin-bottom:1cm;
                        margin-top:0.5cm;
                    }}
                    .signatures{{
                        width:17cm;
                        display:flex;
                        flex-wrap:wrap;
                        xflex-direction:column;
                    }}
                    .signature{{
                        width:8cm;
                        display:flex;
                        margin-bottom:0.35cm;
                    }}
                    .signature-icon{{
                        margin-right:0.3cm;
                    }}
                    .signature-info{{
                        flex:1;
                    }}
                    .signature h1{{
                        font-size:0.4cm;
                    }}
                    .signature h2{{
                        font-size:0.3cm;
                        font-weight:400;
                        padding-bottom:0.1cm;
                    }}
                    .signature h3{{
                        font-size:0.25cm;
                        font-weight:400;
                    }}
                    .signature h4{{
                        font-size:0.28cm;
                    }}
                    a{{
                        text-decoration:none;
                        color:#000;
                    }}
                </style>
                <div id=""header"">
                    <img src=""https://sistemas.camarapiracicaba.sp.gov.br/arquivos/imagens/horizontal.png"" />
                </div>
                <div id=""main"">
                    <h1 class=""title"">{title}</h1>
                    <div class=""signatures"">{signatures}</div>
                    <div class=""signatures"">{lsv}</div>
                </div>
                <div id=""footer"">
                    <img id=""qrcode"" src=""data:image/png;base64, {qrCode}"" />
                    <div id=""info"">
                        <p>
                            Se você deseja verificar a autenticidade deste documento, use o QR Code ao lado,<br />
                            ou acesse <b><a href=""{documento.ValidadorURL}"">{documento.ValidadorURL}</a></b>{hasHash}</b>
                        </p>
                    </div>
                </div>
            ";
            MemoryStream _paginaFinal = html.ConverterParaPdf().ConverterParaMemoryStream();
            MemoryStream _paginaFinalStream = new MemoryStream(_paginaFinal.ToArray());
            PdfDocument paginaFinal = new PdfDocument(new PdfReader(_paginaFinalStream));
            paginaFinal.CopyPagesTo(1, paginaFinal.GetNumberOfPages(), newPdfDocument);
            paginaFinal.Close();
            //newPdfDocument.Close();

            return new DocumentoPDF(outputStream);
        }

        /// <summary>
        /// Adiciona páginas finais a um documento PDF com detalhes sobre as assinaturas e outros pontos importantes do documento.
        /// </summary>
        /// <param name="documento">Documento para adicionar a página final.</param>
        /// <param name="hash">Hash do documento para adicionar ao validador.</param>
        /// <returns></returns>

        public static DocumentoPDF AdicionarDetalhesAoFinal(
            this DocumentoPDF documento,
            string hash = null,
            List<AssinanteDocumento> assinantes = null
        )
        {
            return PaginaFinal(documento, hash, assinantes);
        }

    }

}
