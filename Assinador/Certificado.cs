using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Bouncycastle.Crypto;
using iText.Signatures;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using _S = System.Security.Cryptography.X509Certificates;
using iText.Bouncycastle.X509;
using System.Security.Cryptography;
using System.Security;
using System;
using static System.Security.Cryptography.X509Certificates.RSACertificateExtensions;
using iText.Commons.Bouncycastle.Cert;
using System.Collections;

namespace CMP.ManipuladorPDF
{
    public static partial class ManipuladorPDF
    {
        private static Certificado AbrirCertificado(
            Stream certificado,
            string senha
        )
        {
            Pkcs12Store store = new Pkcs12Store(certificado,senha.ToCharArray());
            string alias = null;
            foreach (object _alias in store.Aliases)
            {
                alias = (string)_alias;
                if (store.IsKeyEntry(alias)) 
                    break;
            }

            var pks = new PrivateKeySignature(
                new PrivateKeyBC(store.GetKey(alias).Key), 
                DigestAlgorithms.SHA256
            );

            X509CertificateEntry[] certificateEntries = store.GetCertificateChain(alias);
            List<X509CertificateBC> _chain = new List<X509CertificateBC>();
            foreach (X509CertificateEntry certificateEntry in certificateEntries)
            {
                _chain.Add(new X509CertificateBC(certificateEntry.Certificate));
            }

            IX509Certificate[] chain = _chain.ToArray();

            X509Certificate dadosCertificado = store.GetCertificate(alias).Certificate;
            string assinante = dadosCertificado.ObterAssinante();

            return new Certificado {
                Assinante = assinante,
                PKS = pks,
                Chain = chain
            };
        }

        private static Certificado AbrirCertificadoA3(Stream certificado)
        {
            
            _S.X509Store store = new _S.X509Store(_S.StoreName.My, _S.StoreLocation.CurrentUser);
            store.Open(_S.OpenFlags.ReadOnly);
            _S.X509Certificate2 certificate2 = store.Certificates.Find(_S.X509FindType.FindBySubjectDistinguishedName, certificado, true)[0];

            X509Certificate certificate = new X509CertificateParser().ReadCertificate(certificate2.GetRawCertData());

            IX509Certificate[] chain = new IX509Certificate[1];
            chain[0] = new X509CertificateBC(certificate);

            return new Certificado
            {
                Assinante = certificate.ObterAssinante(),
                Chain = chain,
                PKS = new X509Certificate2RSASignature(certificate2)
            };
        }

        private static byte[] GeraCertificado(RequisicaoCertificado requisicao)
        {
            _S.X509Certificate2 certificadoRaiz = new _S.X509Certificate2(requisicao.CertificadoRaiz, requisicao.SenhaRaiz);
            RSA rsaKey = RSA.Create();

            string dadosCertificado = $@"
                    L=Piracicaba,
                    ST=SP,
                    C=BR,
                    E={requisicao.Email},
                    DC=camarapiracicaba,
                    DC=sp,
                    DC=gov,
                    DC=br,
                    O=CV Piracicaba,
                    CN={requisicao.Nome}
            ";

            _S.CertificateRequest certificateRequest = new _S.CertificateRequest(dadosCertificado,rsaKey,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);

            byte[] serial = new byte[20];
            Random random = new Random();
            random.NextBytes(serial);
            
            DateTimeOffset notBefore = DateTimeOffset.UtcNow;
            DateTimeOffset notAfter = notBefore.AddYears(1);

            _S.X509Certificate2 certificado = certificateRequest.Create(certificadoRaiz, notBefore, notAfter, serial).CopyWithPrivateKey(rsaKey);

            SecureString secureString = new SecureString();

            foreach (Char _char in requisicao.Senha)
            {
                secureString.AppendChar(_char);
            }

            return certificado.Export(_S.X509ContentType.Pfx, secureString);

        }

        private static string ObterAssinante(this X509Certificate dadosCertificado)
        {
            var subject = dadosCertificado.SubjectDN.GetValueList(X509Name.CN);
            return new string(subject[subject.Count-1].ToString().Where(x => char.IsLetter(x) || x == ' ').ToArray());
        }

        public static Certificado ObterCertificado(string certificado, string senha)
        {
            return AbrirCertificado(new FileStream(certificado, FileMode.Open, FileAccess.Read),senha);
        }

        public static Certificado ObterCertificado(byte[] certificado, string senha)
        {

            return AbrirCertificado(new MemoryStream(certificado), senha);
        }

        public static byte[] GerarCertificado(RequisicaoCertificado requisicao)
        {
            return GeraCertificado(requisicao);
        }

    }

    public class RequisicaoCertificado
    {
        public byte[] CertificadoRaiz { get; set; }
        public string SenhaRaiz { get; set; }
        public string Nome { get; set; }
        public string Senha { get; set; }
        public string Email { get; set; }
    }

    public class Certificado
    {
        public string Assinante { get; set; }
        public IExternalSignature PKS { get; set; }
        public IX509Certificate[] Chain { get; set; }
    }
}
