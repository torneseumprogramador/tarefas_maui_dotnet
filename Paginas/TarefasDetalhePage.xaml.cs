using System.Collections.ObjectModel;
using Tarefas.Constantes;
using Tarefas.Models;
using Tarefas.Servicos;

namespace Tarefas.Paginas;

public partial class TarefasDetalhePage : ContentPage
{
	DatabaseServico<Tarefa> _tarefaServico;
	DatabaseServico<Comentario> _comentarioServico;
	public Tarefa Tarefa { get;set; }
	public TarefasDetalhePage(Tarefa tarefa)
	{
		InitializeComponent();

		_tarefaServico = new DatabaseServico<Tarefa>(Db.DB_PATH);
		_comentarioServico = new DatabaseServico<Comentario>(Db.DB_PATH);
		Tarefa = tarefa;

        UsuarioPicker.ItemsSource = UsuariosServico.Instancia().Todos();

		CarregarComentarios();
		BindingContext = this;
	}

	public async void CarregarComentarios()
	{
        ComentariosCollection.ItemsSource = await _comentarioServico.TodosFilterAsync().Where(c => c.TarefaId == Tarefa.Id).ToListAsync();
	}

	private async void AdicionarComentarioClicked(object sender, EventArgs e)
    {
       	if (UsuarioPicker.SelectedIndex == -1 || string.IsNullOrWhiteSpace(NovoComentarioEditor.Text))
        {
            await DisplayAlert("Erro", "É necessário selecionar um usuário e escrever um comentário.", "OK");
            return;
        }

        var usuarioSelecionado = (Usuario)UsuarioPicker.SelectedItem;
        var novoComentario = new Comentario
        {
            UsuarioId = usuarioSelecionado.Id,
            Texto = NovoComentarioEditor.Text,
            TarefaId = Tarefa.Id,
            Data = DateTime.Now
        };

	 	await _comentarioServico.IncluirAsync(novoComentario);
		
        NovoComentarioEditor.Text = string.Empty; // Limpa o editor após adicionar
        UsuarioPicker.SelectedIndex = -1; // Reseta o picker após adicionar

		CarregarComentarios();
    }
	

	private async void ExcluirClicked(object sender, EventArgs e)
	{
		bool confirm = await DisplayAlert("Confirmação", "Deseja excluir esta tarefa?", "Sim", "Não");
		if (confirm)
		{
			await _comentarioServico.TodosFilterAsync().Where(c => c.TarefaId == Tarefa.Id).DeleteAsync();
			await _tarefaServico.DeleteAsync(Tarefa);
        	await Navigation.PopAsync();
		}
	}

	private async void IrParaAlteracaoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TarefasSalvarPage(Tarefa));
    }

	private async void VoltarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

