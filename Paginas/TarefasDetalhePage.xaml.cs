using System.Collections.ObjectModel;
using Tarefas.Constantes;
using Tarefas.Models;
using Tarefas.Servicos;

namespace Tarefas.Paginas;

public partial class TarefasDetalhePage : ContentPage
{
	DatabaseServico<Tarefa> _tarefaServico;
	DatabaseServico<Comentario> _comentarioServico;
	DatabaseServico<Anexo> _anexoServico;
	
	public Tarefa Tarefa { get;set; }
	public TarefasDetalhePage(Tarefa tarefa)
	{
		InitializeComponent();

		_tarefaServico = new DatabaseServico<Tarefa>(Db.DB_PATH);
		_comentarioServico = new DatabaseServico<Comentario>(Db.DB_PATH);
		_anexoServico = new DatabaseServico<Anexo>(Db.DB_PATH);
		Tarefa = tarefa;

        UsuarioPicker.ItemsSource = UsuariosServico.Instancia().Todos();

		CarregarComentarios();
		CarregarFotosAnexos();
		CarregarLocalizacoesAnexos();
		BindingContext = this;
	}

	public async void CarregarComentarios()
	{
        ComentariosCollection.ItemsSource = await _comentarioServico.TodosFilterAsync().Where(c => c.TarefaId == Tarefa.Id).ToListAsync();
	}

	public async void CarregarLocalizacoesAnexos()
	{
		var localizacoes = await _anexoServico.TodosFilterAsync().Where(c => c.TarefaId == Tarefa.Id && string.IsNullOrEmpty(c.Arquivo)).ToListAsync();
		if(localizacoes.Count > 0)
		{
			LocalizacaoFrame.IsVisible = true;
			LocalizacaoCollection.ItemsSource = localizacoes;
			return;
		}

		LocalizacaoFrame.IsVisible = false;
	}

	public async void CarregarFotosAnexos()
	{
		var fotos = await _anexoServico.TodosFilterAsync().Where(c => c.TarefaId == Tarefa.Id && !string.IsNullOrEmpty(c.Arquivo)).ToListAsync();
		if(fotos.Count > 0)
		{
			FotosFrame.IsVisible = true;
			FotosCollection.ItemsSource = fotos;
			return;
		}

		FotosFrame.IsVisible = false;
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
	
	private async void TirarFotoClicked(object sender, EventArgs e)
	{
		try
		{
			// Verifica se a câmera está disponível no dispositivo
			if (MediaPicker.Default.IsCaptureSupported)
			{
				// Tira uma foto e obtém o arquivo resultante
				FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

				if (photo != null)
				{
					// Cria um stream a partir do arquivo da foto
					using Stream stream = await photo.OpenReadAsync();

					// Define o diretório e o nome do arquivo onde a foto será salva
					string directory = FileSystem.AppDataDirectory;
					string filename = Path.Combine(directory, $"{DateTime.Now.ToString("ddMMyyyy_hhmmss")}.jpg");

					// Salva a foto no diretório definido
					using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
					await stream.CopyToAsync(fileStream);

					await _anexoServico.IncluirAsync(new Anexo
					{
						Arquivo = filename,
						TarefaId = Tarefa.Id,
					});

					CarregarFotosAnexos();
				}
			}
			else
			{
				await DisplayAlert("Erro", "A captura de fotos não é suportada neste dispositivo.", "OK");
			}
		}
		catch (FeatureNotSupportedException fnsEx)
		{
			await DisplayAlert("Erro", "A captura de fotos não é suportada neste dispositivo. - " + fnsEx.Message, "OK");
		}
		catch (PermissionException pEx)
		{
			await DisplayAlert("Erro", "Permissão para acessar a câmera não concedida. - " + pEx.Message, "OK");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
		}
	}
	
	private async void GPSClicked(object sender, EventArgs e)
	{
		var confirmado = await DisplayAlert("Localização", $"Confirma a capitura de sua localização?", "Localizar", "Cancelar");
		if(confirmado)
		{
			LocalizacaoButton.Text = "Carregando...";
			LocalizacaoButton.IsEnabled = false;

			try
			{
				var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
				if (status != PermissionStatus.Granted)
				{
					status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
					if (status != PermissionStatus.Granted)
					{
						await DisplayAlert("Permissão de Localização", "Permissão de acesso à localização não concedida.", "OK");
						return;
					}
				}

				var request = new GeolocationRequest(GeolocationAccuracy.Medium);
				var location = await Geolocation.GetLocationAsync(request);

				if (location != null)
				{
					await _anexoServico.IncluirAsync(new Anexo
					{
						Latitude = location.Latitude.ToString(),
						Longitude = location.Longitude.ToString(),
						TarefaId = Tarefa.Id,
					});

					CarregarLocalizacoesAnexos();

					await DisplayAlert("Localização", $"Latitude: {location.Latitude}, Longitude: {location.Longitude}", "OK");
				}
			}
			catch (FeatureNotSupportedException fnsEx)
			{
				await DisplayAlert("Erro", "GPS não suportado neste dispositivo. - " + fnsEx.Message, "OK");
			}
			catch (FeatureNotEnabledException fneEx)
			{
				await DisplayAlert("Erro", "GPS não está habilitado. - " + fneEx.Message, "OK");
			}
			catch (PermissionException pEx)
			{
				await DisplayAlert("Erro", "Permissão de GPS negada. - " + pEx.Message, "OK");
			}
			catch (Exception ex)
			{
				await DisplayAlert("Erro", "Não foi possível obter a localização. - " + ex.Message, "OK");
			}
			finally
			{
				LocalizacaoButton.Text = "Pegar Coordenadas do GPS";
				LocalizacaoButton.IsEnabled = true;
			}
		}
	}

	private async void LabelLinkGoogleMaps_Tapped(object sender, EventArgs e)
	{
		var label = sender as Label;
		if (label != null)
		{
			var url = label.Text.Split('-')[1].Trim();
			if (!string.IsNullOrWhiteSpace(url))
			{
				await Launcher.OpenAsync(new Uri(url));
			}
		}
	}

	private async void ExcluirClicked(object sender, EventArgs e)
	{
		bool confirm = await DisplayAlert("Confirmação", "Deseja excluir esta tarefa?", "Sim", "Não");
		if (confirm)
		{
			await _anexoServico.TodosFilterAsync().Where(c => c.TarefaId == Tarefa.Id).DeleteAsync();
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

