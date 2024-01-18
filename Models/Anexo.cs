using SQLite;
using Tarefas.Servicos;

namespace Tarefas.Models;

public class Anexo
{

    [PrimaryKey, AutoIncrement]
    public int Id { get;set; }
    
    public string Arquivo { get;set; }
    public string Latitude { get;set; }
    public string Longitude { get;set; }
    public int TarefaId { get;set; }
}