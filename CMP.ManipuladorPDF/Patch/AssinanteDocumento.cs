

namespace CMP.ManipuladorPDF.Patch
{
    public static class AssinanteDocumentoPatch
    {

        public static AssinanteDocumento Patch(this AssinanteDocumento assinante)
        {
            
            if (assinante.Nome == "Piracicaba Camara")
            {
                assinante.Nome = "Câmara Municipal de Piracicaba";
                assinante.Email = "Certificado da entidade";
            }

            if (assinante.Nome == "Wagner Alexandre de Oliveira" && assinante.Email == "mariane@camarapiracicaba.sp.gov.br")
            {
                assinante.Email = "wagnao@camarapiracicaba.sp.gov.br";
            }

            if (assinante.Email == "alinercamposmello@gmail.com")
            {
                assinante.Email = "aline@camarapiracicaba.sp.gov.br";
            }

            return assinante;

        }

    }
}
