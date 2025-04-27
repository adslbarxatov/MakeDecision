namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу решения
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class SolutionPage: ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public SolutionPage ()
			{
			InitializeComponent ();
			}

		// Исправление дефекта интерфейса MAUI, позволяющего обрушить приложение
		// нажатием системной кнопки Назад на главной странице. Применимо, соответственно,
		// только к главной странице
		protected override bool OnBackButtonPressed ()
			{
			return true;
			}
		}
	}
