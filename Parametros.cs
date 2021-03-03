using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;

namespace IntegracaoMobuss
{
    public class Parametros
    {
        public string Cnpj { get; set; }
        public string Servidor { get; set; }
        public string Banco { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string Token { get; set; }
        public string TokenBatidas { get; set; }
        public string CompanyId { get; set; }
        public string ObraId { get; set; }
        public string IdLocal { get; set; }
        public string UltimoRegistro { get; set; }
        public bool LogAtivo { get; set; }
        public bool verificarRegistroTrabalhista { get; set; }
        public bool verificarTreinamento { get; set; }
        public bool verificarEPI { get; set; }
        public bool manterIdentificadorDoSistema { get; set; }

    }

    public static class LerParametros
    {

        public static Parametros LerArquivo()//Le e retonar todos os maramentro no json PARAM.JSON, localizado na pasta do executavel
        {
            // Debugger.Launch();
            try
            {
                var caminho = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                using (StreamReader file = File.OpenText(caminho + @"\param.json"))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o2 = (JObject)JToken.ReadFrom(reader);
                    var paramentros = JsonConvert.DeserializeObject<Parametros>(o2.ToString());
                    paramentros.UltimoRegistro = System.IO.File.ReadAllText(caminho + @"\ultimoLido.txt");

                    if (string.IsNullOrEmpty(paramentros.UltimoRegistro))//se o arquivo estiver em branco, seta ultimo lido = zero
                        paramentros.UltimoRegistro = "0";

                    return paramentros;
                }
            }
            catch (FileNotFoundException e)
            {
                GravaLog.Gravar($"Houve um erro ao ler o arquivo PARAM.JSON, verifique o loga erros!", false, true);
                GravaLog.Gravar($"The file was not found: '{e}'", true, true);


                Console.WriteLine($"The file was not found: '{e}'");
                Console.WriteLine();
            }
            catch (DirectoryNotFoundException e)
            {
                GravaLog.Gravar($"Houve um erro ao ler o arquivo PARAM.JSON, verifique o loga erros!", false, true);
                GravaLog.Gravar($"The directory was not found: '{e}'", true, true);

                Console.WriteLine($"The directory was not found: '{e}'");
                Console.WriteLine();
            }
            catch (IOException e)
            {
                GravaLog.Gravar($"Houve um erro ao ler o arquivo PARAM.JSON, verifique o loga erros!", false, true);
                GravaLog.Gravar($"The file could not be opened: '{e}'", true, true);

                Console.WriteLine($"The file could not be opened: '{e}'");
                Console.WriteLine();
            }

            return null;
        }

        public static void AtualizarUltimaBatida(string ultimoLido) //atualizar arquivo ultimoLido.txt com o ultimo valor encontrado
        {
            if (string.IsNullOrWhiteSpace(ultimoLido))
            {

            }
            else
            {
                try
                {
                    string caminho = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
                    
                    using (System.IO.StreamWriter arquivo =
                    new System.IO.StreamWriter(caminho +"ultimoLido.txt", false))
                    {
                        arquivo.WriteLine(ultimoLido);
                    }
                }
                catch (FileNotFoundException e)
                {
                    GravaLog.Gravar($"Houve um erro ao ler o arquivo PARAM.JSON, verifique o loga erros!", false, true);
                    GravaLog.Gravar($"The file was not found: '{e}'", true, true);


                    Console.WriteLine($"The file was not found: '{e}'");
                    Console.WriteLine();
                }
                catch (DirectoryNotFoundException e)
                {
                    GravaLog.Gravar($"Houve um erro ao ler o arquivo PARAM.JSON, verifique o loga erros!", false, true);
                    GravaLog.Gravar($"The directory was not found: '{e}'", true, true);

                    Console.WriteLine($"The directory was not found: '{e}'");
                    Console.WriteLine();
                }
                catch (IOException e)
                {
                    GravaLog.Gravar($"Houve um erro ao ler o arquivo PARAM.JSON, verifique o loga erros!", false, true);
                    GravaLog.Gravar($"The file could not be opened: '{e}'", true, true);

                    Console.WriteLine($"The file could not be opened: '{e}'");
                    Console.WriteLine();
                }
            }
        }
    }



}
