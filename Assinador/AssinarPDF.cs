using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Math;

namespace Assinador
{
    public static class AssinarPDF
    {
        public static MemoryStream Sign(string caminhoCertificado, string senha, MemoryStream sourceFile, int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null)
        {
            List<string> fileNames = new List<string>();
            List<FileStream> fileStreams = new List<FileStream>();

            try
            {
                var fileName = System.IO.Path.GetTempFileName() + ".pdf";
                int bufferSize = sourceFile.ToArray().Length;
                File.WriteAllBytes(fileName, sourceFile.ToArray());
                var fs = File.OpenRead(fileName);

                fileStreams.Add(fs);
                fileNames.Add(fileName);

                PdfReader reader = new PdfReader(fs);
                return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura);
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

        public static MemoryStream Sign(string caminhoCertificado, string senha, string sourceFile, int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null)
        {
            PdfReader reader = new PdfReader(sourceFile);
            return AssinarInternamente(caminhoCertificado, senha, reader, page, x, y, dataAssinatura);
        }

        private static MemoryStream AssinarInternamente(string caminhoCertificado, string senha, PdfReader reader, int? page = null, int x = 30, int y = 30, DateTime? dataAssinatura = null)
        {
            if (!dataAssinatura.HasValue)
            {
                dataAssinatura = DateTime.Now;
            }

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

            MemoryStream outputStream = new MemoryStream();

            PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties());

            if (!page.HasValue)
            {
                page = signer.GetDocument().GetNumberOfPages();
            }

            PdfSignatureAppearance appearance = signer.GetSignatureAppearance();

            var dadosCertificado = pk12.GetCertificate(alias);

            var subject = dadosCertificado.Certificate.SubjectDN.GetValueList(X509Name.CN);

            appearance
                .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION)
                .SetLayer2Text($"Assinado digitalmente por\n{subject[subject.Count - 1]}")
                .SetLocation("Câmara Municipal de Piracicaba - São Paulo")
                .SetReason("Documento assinado digitalmente nos termos do art. 4º, da Lei nº 14.063, de 23 de setembro de 2020.")
                .SetContact("desenvolvimento@camarapiracicaba.sp.gov.br")
                .SetLayer2FontSize(9)
                .SetSignatureCreator("Biblioteca de Assinatura digital Câmara Municipal de Piracicaba")
                .SetPageRect(new Rectangle(x, y, 200, 50))
                .SetPageNumber(page.Value);

            string signatureName = signer.GetNewSigFieldName();

            signer.SetFieldName(signatureName);
            signer.SetSignDate(dataAssinatura.Value);

            var privateKey = new PrivateKeySignature(pk, DigestAlgorithms.SHA512);

            //signer.SignDetached(privateKey, certPath.ToArray(), null, null, null, 0, PdfSigner.CryptoStandard.CADES);




            PdfDocument pdfDocument = signer.GetDocument();





            // ...

            // Crie uma chave privada para assinar o carimbo de data e hora (apenas para fins de teste; use sua chave privada real em produção)
            //AsymmetricCipherKeyPair keyPair = TspTestUtil.GenerateRSAKeyPair();

            // Crie um certificado X509 para a chave privada (apenas para fins de teste; use seu certificado real em produção)
            //X509Certificate cert = TspTestUtil.CreateSelfSignedCertificate(keyPair);

            // Crie uma requisição de carimbo de data e hora
            //TimeStampRequestGenerator tspRequestGenerator = new TimeStampRequestGenerator();
            //DerObjectIdentifier oid = new DerObjectIdentifier("1.2.840.113549.1.7.2"); // OID para o algoritmo de hash SHA-256
            //byte[] dataToTimestamp = new byte[] { 1, 2, 3, 4, 5 }; // Dados para os quais você deseja obter o carimbo de data e hora
            //tspRequestGenerator.SetCertReq(true);
            //TimeStampRequest timeStampRequest = tspRequestGenerator.Generate(oid, dataToTimestamp);

            //// Crie um "carimbo de data e hora de teste" (apenas para fins de demonstração)
            //byte[] nonce = new byte[] { 0, 1, 2, 3, 4, 5 }; // Valor de nonce arbitrário
            //BigInteger serialNumber = BigInteger.One; // Número de série do carimbo de data e hora arbitrário
            //DateTime timeStampTime = DateTime.UtcNow; // Horário do carimbo de data e hora arbitrário

            //TimeStampResponseGenerator tspResponseGenerator =
            //    new TimeStampResponseGenerator(
            //        new TimeStampTokenGenerator(dadosCertificado.Certificate.GetPublicKey(),
            //        dadosCertificado.Certificate, "", ""),
            //        new List<string>() { DigestAlgorithms.SHA512 });

            ////TimeStampResponseGenerator tspResponseGenerator = new TimeStampResponseGenerator(
            ////    new RespID(cert.GetSubjectDN().ToString()),
            ////    new GenTime(timeStampTime),
            ////    new AccuracySeconds(0),
            ////    new AccuracyMillis(0),
            ////    new AccuracyMicros(0),
            ////    new TstInfo(oid, new DerInteger(serialNumber), new DerGeneralizedTime(timeStampTime), new Accuracy(0, 0, 0), new DerBoolean(false), new DerInteger(0), new DerOid("1.2.3.4"), new Asn1OctetString(nonce)));

            //byte[] tsTokenBytes = tspResponseGenerator.Generate(timeStampRequest, serialNumber, timeStampTime).GetEncoded();

            //// Agora você tem um carimbo de data e hora em tsTokenBytes para testar

            //// Carregue o carimbo de data e hora em um objeto TimeStampToken
            //TimeStampToken tsToken = new TimeStampToken(new CmsSignedData(tsTokenBytes));



            TimeStampRequestGenerator tspRequestGenerator = new TimeStampRequestGenerator();
            DerObjectIdentifier oid = new DerObjectIdentifier("1.2.840.113549.1.7.2"); // OID para o algoritmo de hash SHA-256
            byte[] dataToTimestamp = File.ReadAllBytes("C:\\Users\\0308\\Desktop\\Lei_14063_-_2021.pdf"); // Dados para os quais você deseja obter o carimbo de data e hora
            tspRequestGenerator.SetCertReq(false);
            TimeStampRequest timeStampRequest = tspRequestGenerator.Generate(oid, dataToTimestamp);

            byte[] tsTokenBytes = timeStampRequest.GetEncoded();


            TimeStampToken tsToken = new TimeStampToken(new CmsSignedData(tsTokenBytes));






            //// Crie uma chave privada para assinar o carimbo de data e hora (apenas para fins de teste; use sua chave privada real em produção)
            //AsymmetricCipherKeyPair keyPair = TspTestUtil.GenerateRSAKeyPair();

            //// Crie um certificado X509 para a chave privada (apenas para fins de teste; use seu certificado real em produção)
            //X509Certificate cert = TspTestUtil.CreateSelfSignedCertificate(keyPair);

            //// Crie uma requisição de carimbo de data e hora
            //TimeStampRequestGenerator tspRequestGenerator = new TimeStampRequestGenerator();
            //DerObjectIdentifier oid = new DerObjectIdentifier("1.2.840.113549.1.7.2"); // OID para o algoritmo de hash SHA-256
            //byte[] dataToTimestamp = new byte[] { 1, 2, 3, 4, 5 }; // Dados para os quais você deseja obter o carimbo de data e hora
            //tspRequestGenerator.SetCertReq(true);
            //TimeStampRequest timeStampRequest = tspRequestGenerator.Generate(oid, dataToTimestamp);

            //// Gere o carimbo de data e hora (você precisará de um serviço real de carimbo de data e hora em produção)
            //TimeStamper stamper = new TimeStamper("http://timestamp.digicert.com", null);
            //byte[] tsTokenBytes = stamper.GenerateTimeStampToken(timeStampRequest.GetEncoded(), true);

            //// Agora você tem um carimbo de data e hora em tsTokenBytes para testar

            //// Carregue o carimbo de data e hora em um objeto TimeStampToken
            //TimeStampToken tsToken = new TimeStampToken(new CmsSignedData(tsTokenBytes));

            //// Verifique se o carimbo de data e hora é válido
            //if (tsToken.IsTimeStampValid)
            //{
            //    Console.WriteLine("Carimbo de data e hora válido.");
            //}
            //else
            //{
            //    Console.WriteLine("Carimbo de data e hora inválido.");
            //}










            SignatureUtil signatureUtil = new SignatureUtil(pdfDocument);
            PdfSignature signature = signatureUtil.GetSignature(signatureName);

            //TimeStampToken tsToken = new TimeStampToken(new CmsSignedData(Encoding.ASCII.GetBytes(DateTime.Now.ToString())));

            // Crie um novo dicionário de carimbo de data e hora
            PdfDictionary timeStampDic = new PdfDictionary();
            timeStampDic.Put(PdfName.Type, new PdfName("DocTimeStamp"));
            timeStampDic.Put(PdfName.Filter, PdfName.Adobe_PPKLite);
            timeStampDic.Put(PdfName.SubFilter, PdfName.ETSI_RFC3161);
            timeStampDic.Put(PdfName.Contents, new PdfString(tsToken.GetEncoded()));

            // Anexe o dicionário de carimbo de data e hora à assinatura
            signature.Put(PdfName.DocTimeStamp, timeStampDic);


            // Localize a assinatura existente no documento
            // Obtenha o carimbo de data e hora (byte[] tsTokenBytes) de uma fonte confiável

            // Carregue o carimbo de data e hora em um objeto TimeStampToken

            // Adicione o carimbo de data e hora à assinatura existente
            //X509Certificate cert = tsToken.SignerID.Certificate;
            //TimeStampUtil.AddTimeStamp(signature, tsToken, cert);

            //signer.Timestamp(tsToken, "");

            // Adicione o carimbo de data e hora à assinatura existente

            //PdfDictionary dict = pdfDocument.GetTrailer();
            //PdfString contents = dict.GetAsString(PdfName.Contents);

            //byte[] timestampBytes = Encoding.ASCII.GetBytes(DateTime.Now.ToString());
            //byte[] originalContents = contents.GetValueBytes();

            //// Combine o conteúdo original com o carimbo de data e hora
            //byte[] combinedContents = new byte[originalContents.Length + timestampBytes.Length];
            //System.Buffer.BlockCopy(originalContents, 0, combinedContents, 0, originalContents.Length);
            //System.Buffer.BlockCopy(timestampBytes, 0, combinedContents, originalContents.Length, timestampBytes.Length);

            //// Atualize o dicionário com o novo conteúdo
            //dict.Put(PdfName.Contents, new PdfString(combinedContents));



            // Feche o documento
            pdfDocument.Close();












            return outputStream;
        }
    }
}
