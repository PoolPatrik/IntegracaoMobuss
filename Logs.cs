using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace IntegracaoMobuss
{
    public static class GravaLog
    {
        public static void Gravar(string texto, bool erro, bool ativo)
        {
            string local = string.Empty;
            if (ativo == true)
            {
                if (erro == false)
                {
                    local = @"\log.txt";
                }
                else
                {
                    local = @"\logErros.txt";
                }

                var caminho = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                using (StreamWriter outputFile = new StreamWriter(caminho + local, true))
                {
                    String data = DateTime.Now.ToShortDateString();
                    String hora = DateTime.Now.ToShortTimeString();
                    outputFile.WriteLine(data + " " + hora + " : " + texto);
                }
            }
        }



        public static void GravarBatidasEnviadas(Batida batida)
        {
            var a = DateTime.Now;
            string path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();

            using (StreamWriter arquivoBatidas = new StreamWriter(path + @"\\Batidas\\" + a.Day + "-" + a.Month + "-" + a.Year + ".txt", true))
            {

                string JSONString = string.Empty;
                JSONString = JsonConvert.SerializeObject(batida);
                arquivoBatidas.WriteLine(JSONString);
            }

        }
    }


}