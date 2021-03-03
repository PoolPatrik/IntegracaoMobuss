using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace IntegracaoMobuss
{
    class GerenciadorBatidas
    {
        public static async System.Threading.Tasks.Task EnviarBatidasAsync()
        {
            var listaDeBatidas = new GerenciadorBatidas().BuscarBatidas();
            var paramentros = LerParametros.LerArquivo();
            if (listaDeBatidas.Count > 0)// se houver batidas para exportar
            {
                GravaLog.Gravar("###########Inicio da exportação de batidas###########", false, paramentros.LogAtivo);
                GravaLog.Gravar(listaDeBatidas.Count + " batida lidas", false, paramentros.LogAtivo);

                int index = 0;
                foreach (var batida in listaDeBatidas)// chama o metodo PostBatidasAsync para cadas batida existente
                {
                    bool retornoRequisicao = await Requisicao.PostBatidasAsync(paramentros, listaDeBatidas[index]);
                    if (retornoRequisicao == true)
                    {
                        string ultimoLido = listaDeBatidas[index].idAcesso;
                        LerParametros.AtualizarUltimaBatida(ultimoLido);
                        GravaLog.GravarBatidasEnviadas(batida);
                        index++;
                    }
                    else
                        break;//não houve retorno da requisição, neste caso a tarefa é interrompida para que o ultimo registro lido seja integro
                }
                GravaLog.Gravar("Fim da exportação de batidas", false, paramentros.LogAtivo);
            }
            else
            {
                GravaLog.Gravar("###########Inicio da exportação de batidas###########", false, paramentros.LogAtivo);
                GravaLog.Gravar("Não existem novas batidas para exportação", false, paramentros.LogAtivo);
                GravaLog.Gravar("Fim da exportação de batidas", false, paramentros.LogAtivo);
            }
        }

        public List<Batida> BuscarBatidas()//busca as batidas na tabela vw_acessos (essa tabela foi modificada para atender esse cliente)
        {
            var ultimoRegistro = LerParametros.LerArquivo().UltimoRegistro;//chama metodo que lê o valor salvo no arquivo ultimoLido.txt
            var conexao = new ConexaoBancoDeDados();
            var datatable = conexao.SelectBatidas($"SELECT concat (DATEPART(YEAR, data), '-', DATEPART(MONTH, data),'-', DATEPART(day, data)) as data, hora, tipo_acessos,equipamento_descricao, id_legado_mobuss, id, negado from vw_acessos_mobuss where id > {ultimoRegistro} order by id");
            return ConverterBatidas(datatable);
        }

        static List<Batida> ConverterBatidas(DataTable dt)//converte dataTable para objetos do tipo Batida
        {
            var paramentros = LerParametros.LerArquivo();
            var convertedList = (from rw in dt.AsEnumerable()
                                 select new Batida()
                                 {
                                     dataAcesso = Convert.ToString(rw["data"]) + "T" + Convert.ToString(rw["hora"]) + ".218Z",
                                     flagEntrada = Convert.ToString(rw["tipo_acessos"]),
                                     idLocal = Convert.ToString(paramentros.IdLocal),
                                     idObra = Convert.ToString(paramentros.ObraId),
                                     idColaborador = Convert.ToString(rw["id_legado_mobuss"]),
                                     idLegado = Convert.ToString(rw["id"])+ "_"+ paramentros.ObraId,//em alguns casos mais de uma obra aponta para o mesmo banco de dados do Mobuss (tokken de batidas), então são usados dois programinha de importação, cada uma apontado para um IDObra diferente, concatenando o IdObra evitamos o envio de ids duplicados entre as obras
                                     idAcesso = Convert.ToString(rw["id"]),
                                     flgBloqueado = Convert.ToBoolean(rw["negado"])
                                 }).ToList();

            return convertedList;
        }
    }

    public class Batida
    {
        [JsonIgnore]
        public string idAcesso { get; set; }
        public string idLegado { get; set; }
        public string dataAcesso
        {
            set { _dataHoraBatida = value; }

            get { return (_dataHoraBatida); }
        }
        public string idColaborador { get; set; }

        private string _flgEntrada;

        public Boolean flgEntrada
        {
            get
            {
                if (_flgEntrada == "E")
                {
                    return true;
                }
                else
                    return false;
            }
        }
        public string flagEntrada
        {
            set { _flgEntrada = value; }
        }
        public string idObra { get; set; }
        public string idLocal { get; set; }
        private string _dataHoraBatida { get; set; }

        public bool flgBloqueado { get; set; }

    }

}
