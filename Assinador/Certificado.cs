using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
using X509ContentType = System.Security.Cryptography.X509Certificates.X509ContentType;
using RSASignaturePadding = System.Security.Cryptography.RSASignaturePadding;
using RSA = System.Security.Cryptography.RSA;
using HashAlgorithmName = System.Security.Cryptography.HashAlgorithmName;
using CertificateRequest = System.Security.Cryptography.X509Certificates.CertificateRequest;
using System.Security.Cryptography.X509Certificates;

namespace CMP.ManipuladorPDF.Certificados
{
    public static partial class ExtensionMethods
    {
        private static byte[] GerarCertificado(
            byte[] raiz,
            string senhaRaiz,
            string nome,
            string email,
            string senha
        )
        {
            X509Certificate2 certificadoRaiz = new X509Certificate2(raiz, senhaRaiz);
            RSA rsaKey = RSA.Create();
            string dadosCertificado = $@"
                    L=Piracicaba,
                    ST=SP,
                    C=BR,
                    E={email},
                    DC=camarapiracicaba,
                    DC=sp,
                    DC=gov,
                    DC=br,
                    O=CV Piracicaba,
                    CN={nome}
            ";

            CertificateRequest certificateRequest = new CertificateRequest(dadosCertificado, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            byte[] serial = new byte[20];
            Random random = new Random();
            random.NextBytes(serial);

            DateTimeOffset notBefore = DateTimeOffset.UtcNow;
            DateTimeOffset notAfter = notBefore.AddYears(15);

            X509Certificate2 certificado = certificateRequest.Create(certificadoRaiz, notBefore, notAfter, serial).CopyWithPrivateKey(rsaKey);

            SecureString secureString = new SecureString();
            foreach (Char _char in senha)
            {
                secureString.AppendChar(_char);
            }
            return certificado.Export(X509ContentType.Pfx, secureString);
        }

        public static Certificado Gerar(
           this Certificado certificado,
           byte[] certificadoRaiz,
           string senhaRaiz,
           string nome,
           string email,
           string senha
        )
        {
            byte[] _novoCertificado = GerarCertificado(
                certificadoRaiz,
                senhaRaiz,
                nome,
                email,
                senha
            );
            CertificadoDTO _Certificado = Certificado.AbrirCertificado(new MemoryStream(_novoCertificado), senha);
            certificado.PKS = _Certificado.PKS;
            certificado.Chain = _Certificado.Chain;
            return certificado;
        }

        public static Certificado Gerar(
           this Certificado certificado,
           string certificadoRaiz,
           string senhaRaiz,
           string nome,
           string email,
           string senha
        )
        {
            byte[] _certificadoRaiz = File.ReadAllBytes(certificadoRaiz);
            byte[] _novoCertificado = GerarCertificado(
                _certificadoRaiz,
                senhaRaiz,
                nome,
                email,
                senha
            );
            CertificadoDTO _Certificado = Certificado.AbrirCertificado(new MemoryStream(_novoCertificado), senha);
            certificado.PKS = _Certificado.PKS;
            certificado.Chain = _Certificado.Chain;
            return certificado;
        }

    }

    public class Certificado
    {
        private CertificadoDTO _Certificado { get; set; }
        public string Assinante { get; set; }
        public IExternalSignature PKS { get; set; }
        public IX509Certificate[] Chain { get; set; }

        public static CertificadoDTO AbrirCertificado(
           Stream certificado,
           string senha
        ){
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.Load(certificado, senha.ToCharArray());

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

            return new CertificadoDTO
            {
                PKS = pks,
                Chain = chain
            };

        }

        /// <summary>
        /// Cria uma referência a um certificado.
        /// </summary>
        public Certificado() { }



        /// <summary>
        /// Cria uma referência a um certificado.
        /// </summary>
        /// <param name="certificado">ByteArray do certificado.</param>
        /// <param name="senha">Senha do certificado.</param>
        
        public Certificado(byte[] certificado, string senha)
        {
            _Certificado = AbrirCertificado(new MemoryStream(certificado), senha);
            PKS = _Certificado.PKS;
            Chain = _Certificado.Chain;
        }



        /// <summary>
        /// Cria uma referência a um certificado.
        /// </summary>
        /// <param name="certificado">Caminho absoluto do certificado.</param>
        /// <param name="senha">Senha do certificado.</param>

        public Certificado(string certificado, string senha)
        {
            _Certificado = AbrirCertificado(new FileStream(certificado, FileMode.Open, FileAccess.Read), senha);
            PKS = _Certificado.PKS;
            Chain = _Certificado.Chain;
        }



        /// <summary>
        /// Cria uma referência a um certificado.
        /// </summary>
        /// <param name="certificado">MemoryStream do certificado.</param>
        /// <param name="senha">Senha do certificado.</param>

        public Certificado(MemoryStream certificado, string senha)
        {
            _Certificado = AbrirCertificado(certificado, senha);
            PKS = _Certificado.PKS;
            Chain = _Certificado.Chain;
        }

    }

    public class CertificadoDTO
    {
        public IExternalSignature PKS { get; set; }
        public IX509Certificate[] Chain { get; set; }
    }

}
