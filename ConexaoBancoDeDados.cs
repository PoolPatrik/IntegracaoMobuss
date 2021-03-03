using System.Data;
using System.Data.SqlClient;


namespace IntegracaoMobuss
{
    public class ConexaoBancoDeDados
    {
        private readonly string m_stringConexao;
        public ConexaoBancoDeDados()// monta string para conexão
        {
            var paramentros = LerParametros.LerArquivo();//chama o metodo que le o json localizado na pasta do sistema para recuperar os parametrode conexao

            m_stringConexao = @"server=" + paramentros.Servidor +
                ";Database=" + paramentros.Banco +
                ";integrated security = false" +
                ";user=" + paramentros.Usuario +
                ";Password=" + paramentros.Senha + ";";
        }

        public void ExecutarComando(string query)
        {
            using (SqlConnection connection = new SqlConnection(m_stringConexao))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void Testar()
        {
            using (SqlConnection connection = new SqlConnection(m_stringConexao))
            {
                connection.Open();
            }
        }

        public DataTable SelectBatidas(string sqlQuery)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(m_stringConexao))
            {
                SqlCommand command = new SqlCommand(sqlQuery, connection);
                SqlDataAdapter dataAdapt = new SqlDataAdapter
                {
                    SelectCommand = command
                };

                dataAdapt.Fill(dataTable);
                return dataTable;
            }

        }
    }
}