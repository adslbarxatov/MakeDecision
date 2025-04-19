using RD_AAOW;
using System.Reflection;
using System.Resources;

// Управление общими сведениями о сборке
// ВИДИМЫЕ СТРОКИ
[assembly: AssemblyTitle (ProgramDescription.AssemblyDescription)]
[assembly: AssemblyCompany (RDGenerics.AssemblyCompany)]
// НЕВИДИМЫЕ СТРОКИ
[assembly: AssemblyDescription (ProgramDescription.AssemblyDescription)]
[assembly: AssemblyProduct (ProgramDescription.AssemblyTitle)]
[assembly: AssemblyCopyright (RDGenerics.AssemblyCopyright)]
[assembly: AssemblyVersion (ProgramDescription.AssemblyVersion)]

namespace RD_AAOW
	{
	/// <summary>
	/// Класс, содержащий сведения о программе
	/// </summary>
	public class ProgramDescription
		{
		/// <summary>
		/// Название программы
		/// </summary>
		public const string AssemblyTitle = AssemblyMainName + " v 3.9";

		/// <summary>
		/// Версия программы
		/// </summary>
		public const string AssemblyVersion = "3.9.0.0";

		/// <summary>
		/// Видимое название сборки
		/// </summary>
		public const string AssemblyVisibleName = "• Make decision •";

		/// <summary>
		/// Последнее обновление
		/// </summary>
		public const string AssemblyLastUpdate = "20.04.2025; 1:06";

		/// <summary>
		/// Пояснение к программе
		/// </summary>
		public const string AssemblyDescription = "Tool for making exact decisions";

		/// <summary>
		/// Основное название сборки
		/// </summary>
		public const string AssemblyMainName = "MakeDecision";

		/// <summary>
		/// Возвращает список менеджеров ресурсов для локализации приложения
		/// </summary>
		public readonly static ResourceManager[] AssemblyResources = [
#if ANDROID
			// Языковые ресурсы
			RD_AAOW.MakeDecision_ru_ru.ResourceManager,
			RD_AAOW.MakeDecision_en_us.ResourceManager
#else
			// Обязательный элемент, хранящий значки приложения
			MakeDecisionResources.ResourceManager,

			// Языковые ресурсы
			MakeDecision_ru_ru.ResourceManager,
			MakeDecision_en_us.ResourceManager,
#endif
			];

		/// <summary>
		/// Возвращает набор ссылок на видеоматериалы по языкам
		/// </summary>
		public readonly static string[] AssemblyVideoLinks = [
			"PHqEliYvioY",
			"ISMEm9-F5ZA",
			];

		/// <summary>
		/// Возвращает набор поддерживаемых языков
		/// </summary>
		public readonly static RDLanguages[] AssemblyLanguages = [
			RDLanguages.ru_ru,
			RDLanguages.en_us,
			];

		/// <summary>
		/// Возвращает описание сопоставлений файлов для приложения
		/// </summary>
		public readonly static string[][] AssemblyAssociations = [];
		}
	}
