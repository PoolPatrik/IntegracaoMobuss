using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace IntegracaoMobuss
{
    class Program
    {
        static void Main(string[] arg)
        {
            GravarNoBancoAsync().Wait();
            GerenciadorBatidas.EnviarBatidasAsync().Wait();
        }

        static async Task GravarNoBancoAsync()
        {
            var parametros = LerParametros.LerArquivo();

            try
            {
                GravaLog.Gravar("###########Inicido da importação###########", false, true);
                var conexao = new ConexaoBancoDeDados();
                GerenciadorColaboradores.LimparTabela(conexao);


                var gerenciadorColaboradores = new GerenciadorColaboradores();
                var colaboradores = await Requisicao.GetColaboradoresAsync(parametros);

                if (parametros.manterIdentificadorDoSistema == true)
                {
                    GerenciadorColaboradores.ComparaColaboradores(colaboradores, conexao);
                }

                GravaLog.Gravar(colaboradores.Count + " colaboradores lidos", false, true);

                gerenciadorColaboradores.Salvar(colaboradores, conexao, parametros);

                GravaLog.Gravar("###########Fim da importação###########", false, true);
            }
            catch (Exception ex)
            {
                GravaLog.Gravar(ex.Message, true, parametros.LogAtivo);
            }

        }
        private ConexaoBancoDeDados CriarConexaoSql()
        {
            return new ConexaoBancoDeDados();
        }

    }
}


