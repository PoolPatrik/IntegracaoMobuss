using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IntegracaoMobuss
{
    public class Requisicao
    {
        public static async Task<List<Colaborador>> GetColaboradoresAsync(Parametros paramentros)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("https://www.mobuss.com.br/ccweb/rest/v1/seguranca/");                     
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("companyID", paramentros.CompanyId);
                    client.DefaultRequestHeaders.Add("idObra", paramentros.ObraId);
                    client.DefaultRequestHeaders.Add("token", paramentros.Token);
                    client.DefaultRequestHeaders.Add("verificarRegistroTrabalhista", paramentros.verificarRegistroTrabalhista.ToString());
                    client.DefaultRequestHeaders.Add("verificarTreinamento", paramentros.verificarTreinamento.ToString());
                    client.DefaultRequestHeaders.Add("verificarEPI", paramentros.verificarEPI.ToString());

                    var response = await client.PostAsJsonAsync("consultarRegularidade","colaboradores");


                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jsonObject = (JObject)JsonConvert.DeserializeObject(result);
                        var status = jsonObject["status"].ToString();

                        if (status == "ERRO")
                        {
                            GravaLog.Gravar("Houve um erro para buscar colaboradores na API, verifique o log de erros!", false, paramentros.LogAtivo);
                            GravaLog.Gravar(jsonObject.ToString(), true, paramentros.LogAtivo);
                        }

                        else
                        {
                            var a = jsonObject["colaboradores"].ToObject<List<Colaborador>>();
                            return a;

                            //return jsonObject["colaboradores"].ToObject<List<Colaborador>>();

                        }
                    }
                    else
                    {
                        GravaLog.Gravar("Houve um erro para buscar colaboradores na API, verifique o log de erros!", false, paramentros.LogAtivo);
                        GravaLog.Gravar(response.ReasonPhrase, true, paramentros.LogAtivo);
                    }
                    return new List<Colaborador>();
                }

                catch (Exception e)
                {
                    GravaLog.Gravar("Houve um erro para buscar colaboradores na API, verifique o log de erros!", false, paramentros.LogAtivo);
                    GravaLog.Gravar(e.ToString(), true, paramentros.LogAtivo);

                    return new List<Colaborador>();
                }
            }
        }

        public static async Task<bool> PostBatidasAsync(Parametros parametros, Batida batida)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var batidaSerial = JsonConvert.SerializeObject(batida, Formatting.None);
                    batidaSerial = "[" + batidaSerial + "]";
                    client.BaseAddress = new Uri("https://www.mobuss.com.br/ccweb/rest/v1/catraca/");
                    client.DefaultRequestHeaders
                          .Accept
                          .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parametros.TokenBatidas);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "incluirRegistroAcesso")
                    {
                        Content = new StringContent(batidaSerial,
                                                        System.Text.Encoding.UTF8,
                                                        "application/json")//CONTENT-TYPE header
                    };

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();//guarda o retorno da requisição
                        var jsonObject = (JObject)JsonConvert.DeserializeObject(result);
                        var status = jsonObject["status"].ToString();
                        var mensagem = jsonObject["mensagem"].ToString();

                        if (status == "400")
                        {
                            GravaLog.Gravar("Houve um erro para enviar batidas para a API, verifique o log de erros!", false, parametros.LogAtivo);
                            GravaLog.Gravar(jsonObject.ToString(), true, parametros.LogAtivo);

                            return true; //houve um erro tratado, provalemente batida duplicada 
                        }

                        else
                        {
                            GravaLog.Gravar("iD Acesso: " + batida.idLegado + " - " + "Id ColaboradorMobuss: " + batida.idColaborador + ": " + mensagem, false, parametros.LogAtivo);
                            return true; //não houve erro
                        }

                    }
                    else
                    {
                        GravaLog.Gravar("Houve um erro para enviar batidas para a API, verifique o log de erros!", false, parametros.LogAtivo);
                        GravaLog.Gravar(response.ReasonPhrase, true, parametros.LogAtivo);
                        return false; //houve erro
                    }
                }
                catch (Exception e)
                {

                    GravaLog.Gravar("Houve um erro para enviar batidas para a API, verifique o log de erros!", false, parametros.LogAtivo);
                    GravaLog.Gravar(e.ToString(), true, parametros.LogAtivo);
                    return false; //houve erro
                }
               
            }
        }

    }
}
