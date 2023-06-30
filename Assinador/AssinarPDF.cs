using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Assinador
{
    public static class AssinarPDF
    {
        public static MemoryStream Sign(string caminhoCertificado, string senha, MemoryStream sourceFile, int page = 1, int x = 30, int y = 30, DateTime? dataAssinatura = null)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                if (!dataAssinatura.HasValue)
                {
                    dataAssinatura = DateTime.Now;
                }

                var fileName = System.IO.Path.GetTempFileName() + ".pdf";
                int bufferSize = sourceFile.ToArray().Length;
                File.WriteAllBytes(fileName, sourceFile.ToArray());
                var fs = File.OpenRead(fileName);

                fileStreams.Add(fs);
                fileNames.Add(fileName);

                char[] PASSWORD = senha.ToCharArray();

                Pkcs12Store pk12 = new Pkcs12Store(new FileStream(caminhoCertificado, FileMode.Open, FileAccess.Read), PASSWORD);
                string alias = null;
                foreach (object a in pk12.Aliases)
                {
                    alias = ((string)a);
                    if (pk12.IsKeyEntry(alias))
                    {
                        break;
                    }
                }

                AsymmetricKeyEntry key = pk12.GetKey(alias);
                ICipherParameters pk = key.Key;

                //X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
                //X509Certificate[] chain = new X509Certificate[ce.Length];
                //for (int k = 0; k < ce.Length; ++k)
                //{
                //    chain[k] = ce[k].Certificate;
                //}

                #region Construindo o caminho do certificado do assinante até o certificado de emissor 
                X509Certificate2 subscriberCert = new X509Certificate2(caminhoCertificado, senha);
                X509Chain chain2 = new X509Chain();
                chain2.Build(subscriberCert);

                X509Certificate2Collection certificates = new X509Certificate2Collection();
                foreach (X509ChainElement element in chain2.ChainElements)
                {
                    certificates.Add(element.Certificate);
                }

                List<X509Certificate> certPath = new List<X509Certificate>();
                foreach (X509Certificate2 cert in certificates)
                {
                    certPath.Add(new X509CertificateParser().ReadCertificate(cert.RawData));
                }
                #endregion

                PdfReader reader = new PdfReader(fs);

                MemoryStream outputStream = new MemoryStream();

                PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties());

                PdfSignatureAppearance appearance = signer.GetSignatureAppearance();

                var dadosCertificado = pk12.GetCertificate(alias);

                var subject = dadosCertificado.Certificate.SubjectDN.GetValueList();

                appearance
                    .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION)
                    .SetLayer2Text($"Assinado digitalmente por\n{subject[subject.Count - 1]}")
                    .SetLocation("Câmara Municipal de Piracicaba - São Paulo")
                    .SetReason("Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.")
                    .SetContact("desenvolvimento@camarapiracicaba.sp.gov.br")
                    .SetLayer2FontSize(9)
                    .SetSignatureCreator("Biblioteca de Assinatura digital Câmara Municipal de Piracicaba")
                    .SetPageRect(new Rectangle(x, y, 200, 50))
                    .SetPageNumber(page);

                signer.SetFieldName("assinatura");
                signer.SetSignDate(dataAssinatura.Value);

                var privateKey = new PrivateKeySignature(pk, DigestAlgorithms.SHA512);

                signer.SignDetached(privateKey, certPath.ToArray(), null, null, null, 0, PdfSigner.CryptoStandard.CADES);

                //fileName = System.IO.Path.GetTempFileName() + ".pdf";
                //fileNames.Add(fileName);

                //File.WriteAllBytes(fileName, outputStream.ToArray());

                //fs = File.OpenRead(fileName);
                //fileStreams.Add(fs);

                //reader = new PdfReader(fs);
                //PdfWriter writer = new PdfWriter(System.IO.Path.GetTempFileName() + ".pdf");
                //PdfDocument document = new PdfDocument(reader, writer, new StampingProperties().UseAppendMode());

                //OCSPVerifier ocspVerifier = new OCSPVerifier(null, null);
                //OcspClientBouncyCastle ocspClient = new OcspClientBouncyCastle(ocspVerifier);
                //CrlClientOnline crlClient = new CrlClientOnline();

                //LtvVerification ltvVerification = new LtvVerification(document);

                //SignatureUtil signatureUtil = new SignatureUtil(document);
                //var names = signatureUtil.GetSignatureNames();
                //var sigName = names.Last();
                //PdfPKCS7 pkcs7 = signatureUtil.ReadSignatureData(sigName);
                //if (pkcs7.IsTsp())
                //{
                //    //ltvVerification.AddVerification(sigName, ocsp, crl, LtvVerification.CertificateOption.WHOLE_CHAIN,
                //    //    LtvVerification.Level.OCSP_CRL, LtvVerification.CertificateInclusion.NO);
                //    ltvVerification.AddVerification(sigName, ocspClient, crlClient, LtvVerification.CertificateOption.WHOLE_CHAIN,
                //           LtvVerification.Level.OCSP_CRL,
                //           LtvVerification.CertificateInclusion.YES);
                //}
                //else
                //{
                //    foreach (var name in names)
                //    {
                //        //v.AddVerification(name, ocsp, crl, LtvVerification.CertificateOption.WHOLE_CHAIN,
                //        //    LtvVerification.Level.OCSP_CRL, LtvVerification.CertificateInclusion.NO);
                //        ltvVerification.AddVerification(name, ocspClient, crlClient, LtvVerification.CertificateOption.WHOLE_CHAIN,
                //            LtvVerification.Level.OCSP_CRL,
                //            LtvVerification.CertificateInclusion.YES);
                //    }
                //}
                //ltvVerification.Merge();

                return outputStream;
            }
            catch (Exception ex)
            {
                foreach (var fs in fileStreams)
                {
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
                    fs.Close();
                }

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }

            return sourceFile;
        }
    }
}
