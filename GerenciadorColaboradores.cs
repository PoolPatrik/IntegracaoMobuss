using IntegracaoMobuss;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;

namespace IntegracaoMobuss
{
    public class GerenciadorColaboradores
    {
        //public void Salvar(List<Colaborador> colaboradores, string cnpj, ConexaoBancoDeDados conexao, bool logAtivo)//monta string para insert de pessoas
        public void Salvar(List<Colaborador> colaboradores, ConexaoBancoDeDados conexao, Parametros parametros)

             {
            var ColaboradoresFiltrados = LimpaColaborador(colaboradores);
            foreach (var colaborador in ColaboradoresFiltrados)
            {
                bool isError = false;
                try
                {
                    {
                        string insert = $" INSERT INTO integracao_externa " +
                $"(n_identificador, n_folha ,rg ,cpf,nome ,empresa_cnpj ,estado ,classificacao, email, filtro3, filtro1, filtro2, obs) " +
                $"VALUES ('{colaborador.N_identificador}','{colaborador.IdColaborador}'," +
                $"'{colaborador.NumeroRG}','{colaborador.NumeroCPF}','{colaborador.NomeColaborador}'," +
                $"'{parametros.Cnpj}','{colaborador.ValorSituacao}','Colaborador','{colaborador.Email}', '{colaborador.IdColaborador}'" +
                $",'{colaborador.Empreiteira}','{colaborador.Funcao}','{colaborador.N_provisorio}')";

                        conexao.ExecutarComando(insert);

                    }
                }

                catch (SqlException ex)
                {
                    for (int i = 0; i < ex.Errors.Count; i++)
                    {
                        var log = (@"Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "colaborador: " + colaborador.NomeColaborador + "\n");
                        GravaLog.Gravar("Houve um erro para ao salvar colaboradores, verifique o log de erros!", false, parametros.LogAtivo);
                        GravaLog.Gravar(log, true, parametros.LogAtivo);
                    }
                    isError = true;
                }
                if (isError) continue;
            }
            AjustaProvisorio(conexao);
        }

        public List<Colaborador> LimpaColaborador(List<Colaborador> colaboradores)//remove colaboradores com Matricula em branco
        {
            List<Colaborador> ColaboradoresDescartados = new List<Colaborador>();//lista de colaboradores descartados
            List<Colaborador> ColaboradoresFiltrados = new List<Colaborador>();// lista de colaboradores que estão ok

            foreach (var colaborador in colaboradores)
            {
                bool isError = false;
                try
                {// IdColaborador é usado como folha
                    if (string.IsNullOrEmpty(colaborador.IdColaborador))//tem um cliente que usa matricula no lucar do IdColaborador como chave unica
                    {
                        ColaboradoresDescartados.Add(colaborador);
                    }
                    else
                    {
                        var nSemCaracteres = ApenasNumeros(colaborador.N_identificador);//Alguns clientes usar matricula outros numero identificador
                        colaborador.N_identificador = nSemCaracteres[0];
                        colaborador.N_provisorio = nSemCaracteres[1];
                        ColaboradoresFiltrados.Add(colaborador);
                    }
                }
                catch (Exception ex)
                {
                        var log = (@"colaborador: " + colaborador.NomeColaborador + " " + ex.Message + "\n");
                        GravaLog.Gravar("Houve um erro para ao salvar colaboradores, verifique o log de erros!", false, true);
                        GravaLog.Gravar(log, true, true);
                    
                    isError = true;
                }
                if (isError) continue;
            }
            //log de descartados
            foreach (var colaborador in ColaboradoresDescartados)
            {
                GravaLog.Gravar(colaborador.NomeColaborador + " não importado, NUMERO DE FOLHA NÃO PODE SER NULO", false, true);
            }
            return ColaboradoresFiltrados;
        }

        static DataTable LerPessoasDoBanco(ConexaoBancoDeDados conexao)//busca pessoas do banco e armazena num datatable
        {
            var datatable = conexao.SelectBatidas($"select n_identificador, n_folha, f.descricao as id_legado_mobuss from pessoas p inner join filtro3 f on f.id = p.filtro3_id");
            return datatable;
        }

        public static void ComparaColaboradores(List<Colaborador> colaboradores, ConexaoBancoDeDados conexao)
        /*comparar o filtro3 (id_legado_mobuss) com o campo Idolaborador(id recebido do mobuss) se for igual,
         atualizar a objeto colaborado com o ID que ja existe no banco de dados do acesso, assim os numeros identificador
         ja ajustado não são perdidos*/
        {
            var pessoas = LerPessoasDoBanco(conexao);//recebe o datatable com as pessoas já cadastrada no banco do acessonet

            foreach (var colaborador in colaboradores)
            {

                foreach (DataRow linha in pessoas.Rows)
                {

                    if (linha["id_legado_mobuss"].ToString() == colaborador.IdColaborador)
                    {
                        colaborador.N_identificador = linha["n_identificador"].ToString();
                    }
                }
            }
        }
        public static void LimparTabela(ConexaoBancoDeDados conexao)//Limpa tabela de integração
        {
            conexao.ExecutarComando($"delete integracao_externa");
        }

        public static void AjustaProvisorio(ConexaoBancoDeDados conexao)//Limpa tabela de integração
        {
            Thread.Sleep(120000);
            conexao.ExecutarComando($"update pessoas set n_provisorio = obs");
        }

        public string[] ApenasNumeros(string str)//remover tudo que não for numerico ou virgula da string
        {
            string[] numeros = new string[2];
            var apenasNumeros = new Regex(@"[^\d]");
            var numerosComVirgula = new Regex(@"[^\d,]");
            numeros[0] = apenasNumeros.Replace(str, "");
            numeros[1] = numerosComVirgula.Replace(str, "");

            return numeros;
        }
    }
}
public class Colaborador
{
    private string _valorSituacao;
    private string _empreiteira;
    public string IdObra { get; set; }
    public string NomeObra { get; set; }
    public string IdColaborador { get; set; }
    public string NomeColaborador { get; set; }
    //bloco de codigo adicionado para tentar evitar que o itendificador fique em branco
    //tem um cliente que precisa que o numeroIdentificador seja igual a o numero de matricula do sistema legado 
    // mas em algusn casos o numero matricula esta duplicado entre pessoas ativas e inaticas, nessa caso nós deixar identificador = a matricula
    // apenas para pessoa ativas no sistema ou bloqueadas.
    public string N_identificador
    { get; set; }
    public string N_provisorio
    { get; set; }
    public string CodigoMatricula { get; set; }
    public object DataAdmissao { get; set; }
    public string NumeroRG { get; set; }
    public string NumeroCPF { get; set; }
    public string Empreiteira
    {//o campo filtro 1 onde essa informação ficará só aceita 51 caracteres, aqui cortamos a informação caso ela seja maior.
        get { return _empreiteira; }
        set
        {
            if (value.Length > 50)
            {
                _empreiteira = value.Remove(value.Length - (value.Length - 50));
            }
            else
            {

                _empreiteira = value;
            }
        }
    }
    public string Funcao { get; set; }
    public string Email { get; set; }
    public string NumeroPIS { get; set; }
    public object DataSituacao { get; set; }
    public object DescricaoSituacao { get; set; }
    public string ValorSituacao
    {
        get { return _valorSituacao; }
        set
        {
            if (value == "Liberado")
            {
                _valorSituacao = "2";//liberado
            }
            else if (value == "Bloqueado")
            {
                _valorSituacao = "1";//bloqueado
            }
            else if (value == "Removido" || value == "Inativo")
            {
                _valorSituacao = "3";//desligado
            }
            else
            {
                _valorSituacao = "1";// qualquer outro estado não tratado será bloqueado
            }
        }
    }

}
