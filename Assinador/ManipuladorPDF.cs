using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Assinador
{
    public static class ManipuladorPDF
    {
        public static List<DadosAssinaturaDTO> GetDigitalSignatures(this string filePath)
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

        public static List<DadosAssinaturaDTO> GetDigitalSignatures(this MemoryStream filePath)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fileName = Path.GetTempFileName() + ".pdf";
                int bufferSize = filePath.ToArray().Length;
                File.WriteAllBytes(fileName, filePath.ToArray());
                var fs = File.OpenRead(fileName);

                fileStreams.Add(fs);
                fileNames.Add(fileName);

                List<DadosAssinaturaDTO> signatures = new List<DadosAssinaturaDTO>();

                using (PdfReader pdfReader = new PdfReader(fs))
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
            catch (Exception)
            {
                foreach (var fs in fileStreams)
                {
                    fs.Dispose();
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }
            finally
            {
                foreach (var fs in fileStreams)
                {
                    fs.Dispose();
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }
            return null;
        }

    }

    public class DadosAssinaturaDTO
    {
        public DateTime DataAssinatura { get; set; }
        public X509Certificate2 CertificadoAssinante { get; set; }
        public List<X509Certificate2> CadeiaCertificados { get; set; }
    }
}
