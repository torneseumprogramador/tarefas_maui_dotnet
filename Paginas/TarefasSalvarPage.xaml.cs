using Tarefas.Models;
using Tarefas.Enums;
using Tarefas.Servicos;
using Tarefas.Constantes;

namespace Tarefas.Paginas;

public partial class TarefasSalvarPage : ContentPage
{
	DatabaseServico<Tarefa> _tarefaServico;
	Tarefa _tarefa = null;

	public TarefasSalvarPage(Tarefa tarefa)
	{
		var status = tarefa.Status;
		_tarefa = tarefa;

		InitializeComponent();

		_tarefaServico = new DatabaseServico<Tarefa>(Db.DB_PATH);

        BindingContext = _tarefa;

		StatusPicker.ItemsSource = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
        UsuarioPicker.ItemsSource = UsuariosServico.Instancia().Todos();

		StatusPicker.SelectedItem = status;
    	UsuarioPicker.SelectedItem = _tarefa.Usuario;

		this.Appearing += OnPageAppearing;
	}

	private async void OnPageAppearing(object sender, EventArgs e)
	{
		await Task.Delay(100);
		TituloEntry.Focus();
	}

	private async void OnSaveClicked(object sender, EventArgs e)
    {
		if(string.IsNullOrEmpty(TituloEntry.Text))
		{
			await DisplayAlert("Erro", "O Nome é obrigatório", "Ok");
			TituloEntry.Focus();
			return;
		}

		_tarefa.Titulo = TituloEntry.Text;
        _tarefa.Descricao = DescricaoEditor.Text;

		if(StatusPicker.SelectedItem != null)
        	_tarefa.Status = (Status)StatusPicker.SelectedItem;
		else
        	_tarefa.Status = Status.Backlog;

		if(UsuarioPicker.SelectedItem != null)
        	_tarefa.UsuarioId = ((Usuario)UsuarioPicker.SelectedItem).Id;

		if(_tarefa.Id == 0)
			await _tarefaServico.IncluirAsync(_tarefa);
		else
		{
			_tarefa.DataAtualizacao = DateTime.Now;
			await _tarefaServico.AlterarAsync(_tarefa);
		}

        await Navigation.PopAsync();
    }

	private async void OnVoltarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

