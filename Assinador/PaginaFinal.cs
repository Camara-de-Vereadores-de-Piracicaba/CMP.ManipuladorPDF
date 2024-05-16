using iText.Kernel.Pdf;
using iText.Signatures;
using QRCoder;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        private static DocumentoPDF PaginaFinal(
            DocumentoPDF documento,
            string hash = null
        )
        {
            TimeZoneInfo fuso = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

            using MemoryStream outputStream = new MemoryStream();
            using PdfWriter pdfWriter = new PdfWriter(outputStream);
            using PdfDocument newPdfDocument = new PdfDocument(pdfWriter);
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(documento.ByteArray)));
            SignatureUtil signatureUtil = new SignatureUtil(pdfDocument);

            string signatures = "";
            string lsv = "";

            IList<String> names = signatureUtil.GetSignatureNames();
            foreach (String signatureName in names)
            {
                PdfPKCS7 signature = signatureUtil.ReadSignatureData(signatureName);
                if (!signature.IsTsp())
                {
                    var info = CertificateInfo.GetSubjectFields(signature.GetCertificates()[0]);
                    string name = info.GetField("CN");
                    string email = info.GetField("E");
                    string date = TimeZoneInfo.ConvertTime(signature.GetSignDate(), fuso).ToString("G");
                    string reason = signature.GetReason();
                    signatures += $@"
                         <div class=""signature"">
                            <div class=""signature-icon"">
                                <svg xmlns=""http://www.w3.org/2000/svg"" height=""24"" viewBox=""0 -960 960 960"" width=""24""><path d=""M563-491q73-54 114-118.5T718-738q0-32-10.5-47T679-800q-47 0-83 79.5T560-541q0 14 .5 26.5T563-491ZM120-120v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80ZM136-280l-56-56 64-64-64-64 56-56 64 64 64-64 56 56-64 64 64 64-56 56-64-64-64 64Zm482-40q-30 0-55-11.5T520-369q-25 14-51.5 25T414-322l-28-75q28-10 53.5-21.5T489-443q-5-22-7.5-48t-2.5-56q0-144 57-238.5T679-880q52 0 85 38.5T797-734q0 86-54.5 170T591-413q7 7 14.5 10.5T621-399q26 0 60.5-33t62.5-87l73 34q-7 17-11 41t1 42q10-5 23.5-17t27.5-30l63 49q-26 36-60 58t-63 22q-21 0-37.5-12.5T733-371q-28 25-57 38t-58 13Z""/></svg>
                            </div>

                            <div class=""signature-info"">
                                <h1>{name}</h1>
                                <h2>{email}</h2>
                                <h4>Assinado no dia {date}</h4>
                            </div>
                        </div>
                    ";
                }
                else
                {
                    var info = CertificateInfo.GetSubjectFields(signature.GetCertificates()[0]);
                    string name = info.GetField("CN");
                    string date = TimeZoneInfo.ConvertTime(signature.GetSignDate(), fuso).ToString("G");
                    lsv += $@"
                         <div class=""signature"">
                            <div class=""signature-icon"">
                                <svg xmlns=""http://www.w3.org/2000/svg"" height=""24"" viewBox=""0 -960 960 960"" width=""24""><path d=""m438-298 226-226-57-57-169 169-85-85-57 57 142 142Zm42 218q-75 0-140.5-28.5t-114-77q-48.5-48.5-77-114T120-440q0-75 28.5-140.5t77-114q48.5-48.5 114-77T480-800q75 0 140.5 28.5t114 77q48.5 48.5 77 114T840-440q0 75-28.5 140.5t-77 114q-48.5 48.5-114 77T480-80Zm0-360ZM224-866l56 56-170 170-56-56 170-170Zm512 0 170 170-56 56-170-170 56-56ZM480-160q117 0 198.5-81.5T760-440q0-117-81.5-198.5T480-720q-117 0-198.5 81.5T200-440q0 117 81.5 198.5T480-160Z""/></svg>
                            </div>
                            <div class=""signature-info"">
                                <h1>Esse documento possui um carimbo do tempo.</h1>
                                <h2>Carimbado em {date}</h2>
                                <h4>Por: {name}</h4>
                            </div>
                        </div>
                    ";
                }
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
            Bitmap qrCodeImage = _qrCode.GetGraphic(4);
            MemoryStream ms = new MemoryStream();
            qrCodeImage.Save(ms, ImageFormat.Png);
            string qrCode = Convert.ToBase64String(ms.ToArray());

            string html = $@"
                <style>
                    *{{
                        font-family:'Calibri';
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
                    <h1 class=""title"">Este documento foi assinado digitalmente pelos seguintes signatários:</h1>
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
            newPdfDocument.Close();

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
            string hash
        )
        {
            return PaginaFinal(documento, hash);
        }

    }

}
