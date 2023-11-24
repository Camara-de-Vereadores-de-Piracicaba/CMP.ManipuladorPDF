using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Assinador
{
    public class ManipuladorPDF
    {
        public static List<DadosAssinaturaDTO> GetDigitalSignatures(string filePath)
        {
            List<DadosAssinaturaDTO> signatures = new List<DadosAssinaturaDTO>();

            using (PdfReader pdfReader = new PdfReader(filePath))
            {
                PdfDocument pdfDocument = new PdfDocument(pdfReader);

                var signatureUtil = new SignatureUtil(pdfDocument);
                var signatureNames = signatureUtil.GetSignatureNames();

                foreach (var signatureName in signatureNames)
                {
                    var signature = signatureUtil.GetSignature(signatureName);
                    var conteudo = signature.GetContents();
                    var dataAssinatura = signature.GetDate().GetValue();
                    byte[] signatureBytes = conteudo.GetValueBytes();
                    var signedData = new SignedCms();
                    signedData.Decode(signatureBytes);
                    var signerInfos = signedData.SignerInfos;

                    var dados = new DadosAssinaturaDTO
                    {
                        CadeiaCertificados = new List<X509Certificate2>(),
                        CertificadoAssinante = signerInfos[0].Certificate,
                        DataAssinatura = PdfDate.Decode(dataAssinatura),
                    };

                    foreach (var certificate in signedData.Certificates)
                    {
                        dados.CadeiaCertificados.Add(certificate);
                    }

                    signatures.Add(dados);
                }
            }

            return signatures;
        }
    }

    public class DadosAssinaturaDTO
    {
        public DateTime DataAssinatura { get; set; }
        public X509Certificate2 CertificadoAssinante { get; set; }
        public List<X509Certificate2> CadeiaCertificados { get; set; }
    }
}
