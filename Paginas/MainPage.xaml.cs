using System.Windows.Input;
using Tarefas.Constantes;
using Tarefas.Models;
using Tarefas.Servicos;

namespace Tarefas.Paginas;

public partial class MainPage : ContentPage
{
	DatabaseServico<Tarefa> _tarefaServico;

	public ICommand NavigateToDetailCommand { get; private set; }
	
	public MainPage()
	{
		InitializeComponent();
		_tarefaServico = new DatabaseServico<Tarefa>(Db.DB_PATH);

        NavigateToDetailCommand = new Command<Tarefa>(async (tarefa) => await NavigateToDetail(tarefa));

		CarregarTarefas();
	}

	protected override void OnAppearing()
    {
        base.OnAppearing();
        CarregarTarefas();
    }

	private async Task NavigateToDetail(Tarefa tarefa)
    {
        await Navigation.PushAsync(new TarefasDetalhePage(tarefa));
    }

	private async void CarregarTarefas()
	{
		CardBacklog.ItemsSource = await _tarefaServico.TodosFilterAsync().Where(t => t.Status == Enums.Status.Backlog).ToListAsync();
		CardAnalise.ItemsSource = await _tarefaServico.TodosFilterAsync().Where(t => t.Status == Enums.Status.Analise).ToListAsync();
		CardParaFazer.ItemsSource = await _tarefaServico.TodosFilterAsync().Where(t => t.Status == Enums.Status.ParaFazer).ToListAsync();
		CardFazendo.ItemsSource = await _tarefaServico.TodosFilterAsync().Where(t => t.Status == Enums.Status.Desenvolvimento).ToListAsync();
		CardFeito.ItemsSource = await _tarefaServico.TodosFilterAsync().Where(t => t.Status == Enums.Status.Feito).ToListAsync();
	}

	private async void NovoClicked(object sender, EventArgs e)
    {
        var botao = sender as Button;
		if (botao != null)
		{
			var status = (Enums.Status)botao.CommandParameter;
			await Navigation.PushAsync(new TarefasSalvarPage(new Tarefa { Status = status }));
    	}
    }

}

