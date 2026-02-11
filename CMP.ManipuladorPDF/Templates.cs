namespace CMP.ManipuladorPDF
{

    public class TemplateHtml
    {

        public string _html { get; set; }

        public TemplateHtml(
            string text,
            string departamento = null,
            string setor = null,
            double[] margem = null,
            string css = null)
        {
            AdicionarConteudo(text, departamento, setor, margem, css);
        }

        private void AdicionarConteudo(
            string text,
            string departamento = null,
            string setor = null,
            double[] margem = null,
            string css = null)
        {

            if (margem == null)
            {
                margem = new double[4] { 3, 3, 1.5, 2 };
            }

            if (css == null)
            {
                css = "";
            }

            string subtitulo = "";

            if (departamento != null)
            {
                if (setor != null)
                {
                    subtitulo = $@"<span style=""line-height:0px; font-size:0.27cm; text-align:center; position:relative; top:-0.5cm; text-transform:uppercase; font-weight:300; "">{departamento} &nbsp;•&nbsp; <strong>{setor}</strong></span>";
                }
                else
                {
                    subtitulo = $@"<span style=""line-height:0px; font-size:0.27cm; text-align:center; position:relative; top:-0.5cm; text-transform:uppercase; ""><strong>{departamento}</strong></span>";
                }
            }

            string _style = $@"
                 <style>
                    @page{{
                        margin-left:{margem[0]}cm;
                        margin-right:{margem[1]}cm;
                        margin-top:{margem[2]}cm;
                        margin-bottom:{margem[3]}cm;
                    }}
                    html,body{{
                        font-family:""Aptos"";
                    }}
                    p{{
                        line-height:0.4cm;
                    }}
                    {css}
                </style>";

            string _header = $@"
                <div style=""display:flex; flex-direction:column; align-items:center;"">
                    <img style=""position:relative;width:12cm;top:-0.2cm;"" src=""https://sistemas.camarapiracicaba.sp.gov.br/digital/img/formularios/cabecalho/camara.jpg"" />
                    {subtitulo}
                </div>";

            string _footer = $@"
                <footer style=""position:absolute;bottom:0cm;width:100vw;text-align:center;color:#555;"">
                    <img style=""position:relative;width:9cm;"" src=""https://sistemas.camarapiracicaba.sp.gov.br/digital/img/formularios/rodape/camara.jpg"" />
                </footer>";


            _html = $"{_style}{_header}<div>{text}</div>{_footer}";
        }

        public DocumentoPDF ConverterParaPdf()
        {
            return _html.ToString().ConverterParaPdf();
        }

        public DocumentoPDF ConverterParaPdfTagged()
        {
            return _html.ToString().ConverterParaPdfTagged();
        }

        public override string ToString()
        {
            return _html.ToString();
        }

    }
}
