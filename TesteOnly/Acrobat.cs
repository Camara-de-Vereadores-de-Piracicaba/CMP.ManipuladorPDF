using System.Diagnostics;

namespace Adobe
{
    public static class Acrobat
    {
        public static void FecharAcrobat()
        {
            var acrobat = Process.GetProcesses();
            foreach (var process in acrobat)
            {
                if (process.ProcessName == "AcroRd32" || process.ProcessName == "Acrobat")
                {
                    process.Kill();
                }
            }
        }

        public static void AbrirAcrobat(string arquivo)
        {
            Process.Start("C:\\Program Files\\Adobe\\Acrobat DC\\Acrobat\\Acrobat.exe", arquivo);
        }

    }
}
