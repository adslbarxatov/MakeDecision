using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает функционал приложения
	/// </summary>
	public partial class App: Application
		{
		#region Общие переменные и константы

		// Прочие параметры
		private const int masterLinesCount = 10;
		private uint phase = 1;
		/*private bool firstStart = true;*/
		private RDAppStartupFlags flags;

		// Таблицы расчёта результатов
		private List<string> objects = new List<string> (),
			criteria = new List<string> ();
		private List<int> values = new List<int> ();
		private MakeDecisionMath criteriaMath;
		private List<MakeDecisionMath> objectsMaths = new List<MakeDecisionMath> ();

		// Цветовая схема
		private readonly Color
			solutionMasterBackColor = Color.FromHex ("#ffe7f3"),
			solutionFieldBackColor = Color.FromHex ("#ffdeef"),

			aboutMasterBackColor = Color.FromHex ("#F0FFF0"),
			aboutFieldBackColor = Color.FromHex ("#D0FFD0");

		// Имена хранисых параметров
		private const string objectsRegKey = "Object";
		private const string criteriaRegKey = "Criteria";
		private const string valuesRegKey = "Value";
		/*private const string firstStartRegKey = "HelpShownAt";*/

		#endregion

		#region Переменные страниц

		private ContentPage solutionPage, aboutPage;

		private Label aboutLabel, actLabel, resultLabel, aboutFontSizeField;

		private Editor[] textFields = new Editor[masterLinesCount],
			objectsFields = new Editor[masterLinesCount];

		private List<Slider> valueFields = new List<Slider> ();

		private Label[] valueLabels = new Label[masterLinesCount];

		private Xamarin.Forms.Button restartButton, shareButton, languageButton;

		#endregion

		#region Запуск и настройка

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App (RDAppStartupFlags Flags)
			{
			// Инициализация
			InitializeComponent ();
			flags = Flags;

			// Общая конструкция страниц приложения
			MainPage = new MasterPage ();

			solutionPage = AndroidSupport.ApplyPageSettings (MainPage, "SolutionPage",
				RDLocale.GetText ("SolutionPage"), solutionMasterBackColor);
			aboutPage = AndroidSupport.ApplyPageSettings (MainPage, "AboutPage",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				aboutMasterBackColor);
			AndroidSupport.SetMainPage (MainPage);

			#region Основная страница

			AndroidSupport.ApplyButtonSettings (solutionPage, "ResetButton",
				RDDefaultButtons.Delete, solutionFieldBackColor, ResetButton_Clicked);
			restartButton = AndroidSupport.ApplyButtonSettings (solutionPage, "RestartButton",
				RDDefaultButtons.Refresh, solutionFieldBackColor, RestartButton_Clicked);
			AndroidSupport.ApplyButtonSettings (solutionPage, "NextButton",
				RDDefaultButtons.Start, solutionFieldBackColor, NextButton_Clicked);
			shareButton = AndroidSupport.ApplyButtonSettings (solutionPage, "ShareButton",
				RDDefaultButtons.Share, solutionFieldBackColor, ShareResults);

			actLabel = AndroidSupport.ApplyLabelSettings (solutionPage, "ActivityLabel", "",
				RDLabelTypes.HeaderCenter);
			actLabel.FontSize += 2;

			resultLabel = AndroidSupport.ApplyLabelSettings (solutionPage, "ResultLabel", "",
				RDLabelTypes.FieldMonotype, solutionFieldBackColor);

			for (int i = 0; i < masterLinesCount; i++)
				{
				string s = i.ToString ("D02");
				objectsFields[i] = AndroidSupport.ApplyEditorSettings (solutionPage, "ObjectField" + s,
					solutionFieldBackColor, Keyboard.Text, 50, "", ObjectName_TextChanged, true);
				textFields[i] = AndroidSupport.ApplyEditorSettings (solutionPage, "TextField" + s,
					solutionFieldBackColor, Keyboard.Text, 50, "", CriteriaName_TextChanged, true);

				valueFields.Add (AndroidSupport.ApplySliderSettings (solutionPage, "ValueField" + s,
					ValueField_ValueChanged));
				valueLabels[i] = AndroidSupport.ApplyLabelSettings (solutionPage, "ValueLabel" + s,
					"", RDLabelTypes.Semaphore, solutionFieldBackColor);
				ValueField_ValueChanged (valueFields[i], null);
				}

			// Получение настроек перед инициализацией
			for (int i = 0; i < masterLinesCount; i++)
				{
				objects.Add (RDGenerics.GetAppSettingsValue (objectsRegKey + i.ToString ("D2")));
				criteria.Add (RDGenerics.GetAppSettingsValue (criteriaRegKey + i.ToString ("D2")));
				try
					{
					values.Add (int.Parse (RDGenerics.GetAppSettingsValue (valuesRegKey + i.ToString ("D2"))));
					}
				catch
					{
					values.Add (1);
					}
				}
			/*firstStart = RDGenerics.GetAppSettingsValue (firstStartRegKey) == "";*/

			// Инициализация зависимых полей
			ResetApp (false);

			#endregion

			#region Страница "О программе"

			aboutLabel = AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				RDGenerics.AppAboutLabelText, RDLabelTypes.AppAbout);

			AndroidSupport.ApplyButtonSettings (aboutPage, "ManualsButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_ReferenceMaterials),
				aboutFieldBackColor, ReferenceButton_Click, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "HelpButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_HelpSupport),
				aboutFieldBackColor, HelpButton_Click, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "RepeatTips", RDLocale.GetText ("RepeatTips"),
				aboutFieldBackColor, RepeatTips_Clicked, false);
			AndroidSupport.ApplyLabelSettings (aboutPage, "GenericSettingsLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_GenericSettings),
				RDLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (aboutPage, "RestartTipLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Message_RestartRequired),
				RDLabelTypes.Tip);

			AndroidSupport.ApplyLabelSettings (aboutPage, "LanguageLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceLanguage),
				RDLabelTypes.DefaultLeft);
			languageButton = AndroidSupport.ApplyButtonSettings (aboutPage, "LanguageSelector",
				RDLocale.LanguagesNames[(int)RDLocale.CurrentLanguage],
				aboutFieldBackColor, SelectLanguage_Clicked, false);

			AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceFontSize),
				RDLabelTypes.DefaultLeft);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeInc",
				RDDefaultButtons.Increase, aboutFieldBackColor, FontSizeButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeDec",
				RDDefaultButtons.Decrease, aboutFieldBackColor, FontSizeButton_Clicked);
			aboutFontSizeField = AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeField",
				" ", RDLabelTypes.DefaultCenter);

			AndroidSupport.ApplyLabelSettings (aboutPage, "HelpTextLabel",
				RDGenerics.GetEncoding (RDEncodings.UTF8).
				GetString ((byte[])RD_AAOW.Properties.Resources.ResourceManager.
				GetObject (RDLocale.GetHelpFilePath ())), RDLabelTypes.SmallLeft);

			FontSizeButton_Clicked (null, null);

			#endregion

			// Отображение подсказок первого старта
			ShowStartupTips ();
			}

		// Метод отображает подсказки при первом запуске
		private async void ShowStartupTips ()
			{
			// Контроль XPUN
			if (!flags.HasFlag (RDAppStartupFlags.Huawei))
				await AndroidSupport.XPUNLoop ();

			// Защита
			/*if (firstStart)
				{
				switch (TipsNumber)
					{
					case 0:
					case 1:*/

			// Требование принятия Политики
			if (TipsState.HasFlag (TipTypes.PolicyTip))
				return;

			await AndroidSupport.PolicyLoop ();
			/*RDGenerics.SetAppSettingsValue (firstStartRegKey, ProgramDescription.AssemblyVersion);*/

			// Первая подсказка
			await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip00"),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next));
			await AndroidSupport.ShowMessage (string.Format (RDLocale.GetText ("Tip01"),
				masterLinesCount), RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			TipsState |= TipTypes.PolicyTip;
			/*break;

					case 2:
					case 3:
					case 4:
						await AndroidSupport.ShowMessage (string.Format (RDLocale.GetText ("Tip0" +
							TipsNumber.ToString ()), masterLinesCount),
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
						break;

					case 5:
						await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip05"),
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next));
						await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip06"),
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));

						firstStart = false;
						break;
					}
				}*/
			}

		/// <summary>
		/// Сохранение настроек программы
		/// </summary>
		protected override void OnSleep ()
			{
			try
				{
				for (int i = 0; i < masterLinesCount; i++)
					{
					if (phase < 3)
						{
						RDGenerics.SetAppSettingsValue (objectsRegKey + i.ToString ("D2"), objectsFields[i].Text);
						RDGenerics.SetAppSettingsValue (criteriaRegKey + i.ToString ("D2"), textFields[i].Text);
						RDGenerics.SetAppSettingsValue (valuesRegKey + i.ToString ("D2"),
							((int)valueFields[i].Value).ToString ());
						}
					else
						{
						RDGenerics.SetAppSettingsValue (objectsRegKey + i.ToString ("D2"),
							(i < objects.Count) ? objects[i] : "");
						RDGenerics.SetAppSettingsValue (criteriaRegKey + i.ToString ("D2"),
							(i < criteria.Count) ? criteria[i] : "");
						RDGenerics.SetAppSettingsValue (valuesRegKey + i.ToString ("D2"), (i < values.Count) ?
							((int)values[i]).ToString () : "1");
						}
					}
				}
			catch { }
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов просмотра справочных сведений
		/// </summary>
		public static TipTypes TipsState
			{
			get
				{
				if (tipsState < uint.MaxValue)
					return (TipTypes)tipsState;

				try
					{
					tipsState = uint.Parse (RDGenerics.GetAppSettingsValue (tipsStatePar));
					}
				catch
					{
					tipsState = 0;
					}
				return (TipTypes)tipsState;
				}
			set
				{
				tipsState = (uint)value;
				RDGenerics.SetAppSettingsValue (tipsStatePar, tipsState.ToString ());
				}
			}
		private static uint tipsState = uint.MaxValue;
		private const string tipsStatePar = "TipsState";

		/// <summary>
		/// Доступные типы уведомлений
		/// </summary>
		public enum TipTypes
			{
			/// <summary>
			/// Принятие Политики и первая подсказка
			/// </summary>
			PolicyTip = 0x0001,

			/// <summary>
			/// Подсказка по критериям
			/// </summary>
			CriteriaTip = 0x0002,

			/// <summary>
			/// Подсказка по оценкам
			/// </summary>
			RateTip = 0x0004,

			/// <summary>
			/// Подсказка по ошибкам
			/// </summary>
			RestartTip = 0x0008,

			/// <summary>
			/// Подсказка по результатам
			/// </summary>
			ResultTip = 0x0010,
			}

		#endregion

		#region О приложении

		// Выбор языка приложения
		private async void SelectLanguage_Clicked (object sender, EventArgs e)
			{
			languageButton.Text = await AndroidSupport.CallLanguageSelector ();
			}

		// Вызов справочных материалов
		private async void ReferenceButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.CallHelpMaterials (RDHelpMaterials.ReferenceMaterials);
			}

		private async void HelpButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.CallHelpMaterials (RDHelpMaterials.HelpAndSupport);
			}

		// Изменение размера шрифта интерфейса
		private void FontSizeButton_Clicked (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Xamarin.Forms.Button b = (Xamarin.Forms.Button)sender;
				if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Increase))
					AndroidSupport.MasterFontSize += 0.5;
				else if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Decrease))
					AndroidSupport.MasterFontSize -= 0.5;
				}

			aboutFontSizeField.Text = AndroidSupport.MasterFontSize.ToString ("F1");
			aboutFontSizeField.FontSize = AndroidSupport.MasterFontSize;
			}

		// Запуск с начала
		private async void RepeatTips_Clicked (object sender, EventArgs e)
			{
			TipsState = TipTypes.PolicyTip;

			await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip00"),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next));
			await AndroidSupport.ShowMessage (string.Format (RDLocale.GetText ("Tip01"),
				masterLinesCount), RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			}

		#endregion

		#region Рабочая зона

		// Сброс на исходное состояние
		private async void ResetButton_Clicked (object sender, EventArgs e)
			{
			if (!await AndroidSupport.ShowMessage (RDLocale.GetText ("ResetMessage"),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
				return;

			ResetApp (true);
			}

		// Запуск с начала
		private async void RestartButton_Clicked (object sender, EventArgs e)
			{
			if (!await AndroidSupport.ShowMessage (RDLocale.GetText ("RestartMessage"),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
				return;

			ResetApp (false);
			}

		private void ValueField_ValueChanged (object sender, ValueChangedEventArgs e)
			{
			int idx = valueFields.IndexOf ((Slider)sender);
			int v = (int)valueFields[idx].Value;
			valueLabels[idx].Text = v.ToString () + "%";
			}

		private void ResetApp (bool Fully)
			{
			// Сброс состояния
			phase = 1;
			actLabel.Text = RDLocale.GetText ("ActivityLabelText01");
			resultLabel.IsVisible = false;
			restartButton.IsEnabled = shareButton.IsEnabled = false;

			for (int i = 0; i < masterLinesCount; i++)
				{
				objectsFields[i].IsVisible = (i == 0);
				objectsFields[i].Text = "";

				textFields[i].IsVisible = textFields[i].IsReadOnly = false;
				textFields[i].Text = "";

				valueFields[i].IsVisible = valueLabels[i].IsVisible = false;
				valueFields[i].Value = valueFields[i].Minimum;
				}

			// Востановление значений из кэша
			if (!Fully)
				{
				for (int i = 0; i < objects.Count; i++)
					objectsFields[i].Text = objects[i];

				for (int i = 0; i < criteria.Count; i++)
					textFields[i].Text = criteria[i];

				for (int i = 0; i < values.Count; i++)
					valueFields[i].Value = values[i];
				}

			// Обнуление
			objects.Clear ();
			criteria.Clear ();
			values.Clear ();
			objectsMaths.Clear ();
			}

		// Реакция на изменение состава объектов
		private void ObjectName_TextChanged (object sender, TextChangedEventArgs e)
			{
			// Контроль
			if (phase > 1)
				return;

			// Обновление
			for (int i = 1; i < masterLinesCount; i++)
				objectsFields[i].IsVisible = (!string.IsNullOrWhiteSpace (objectsFields[i - 1].Text)) &&
					objectsFields[i - 1].IsVisible;
			}

		// Смена состояния
		private async void NextButton_Clicked (object sender, EventArgs e)
			{
			switch (phase)
				{
				// Переход к указанию критериев сравнения
				case 1:
					// Контроль достаточности объектов
					if (!objectsFields[2].IsVisible)    // Возникает при заполнении первых двух строк
						{
						AndroidSupport.ShowBalloon (RDLocale.GetText ("NotEnoughVariants"), true);
						return;
						}

					// Перенос
					for (int i = 0; i < masterLinesCount; i++)
						{
						if (objectsFields[i].Text == "")
							break;
						else
							objects.Add (objectsFields[i].Text);
						}

					// Изменение состояния
					for (int i = 0; i < masterLinesCount; i++)
						objectsFields[i].IsVisible = false;
					textFields[0].IsVisible = valueFields[0].IsVisible = valueLabels[0].IsVisible = true;

					actLabel.Text = RDLocale.GetText ("ActivityLabelText02");

					// Принудительный вызов на случай уже имеющихся значений полей
					CriteriaName_TextChanged (null, null);

					// Переход далее
					phase++;

					/*ShowTips (2);*/
					if (!TipsState.HasFlag (TipTypes.CriteriaTip))
						{
						await AndroidSupport.ShowMessage (string.Format (RDLocale.GetText ("Tip02"), masterLinesCount),
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
						TipsState |= TipTypes.CriteriaTip;
						}

					break;

				// Переход к ранжированию критериев сравнения
				case 2:
					// Контроль достаточности объектов
					if (!textFields[2].IsVisible)    // Возникает при заполнении первых двух строк
						{
						AndroidSupport.ShowBalloon (RDLocale.GetText ("NotEnoughCriteria"), true);
						return;
						}

					// Перенос
					for (int i = 0; i < masterLinesCount; i++)
						{
						if (textFields[i].Text == "")
							{
							break;
							}
						else
							{
							criteria.Add (textFields[i].Text);
							values.Add ((int)valueFields[i].Value);
							}
						}
					criteriaMath = new MakeDecisionMath (values);

					// Изменение состояния
					for (int i = 0; i < masterLinesCount; i++)
						{
						textFields[i].IsReadOnly = true;
						if (i < objects.Count)
							{
							textFields[i].IsVisible = valueFields[i].IsVisible = valueLabels[i].IsVisible = true;
							textFields[i].Text = objects[i];

							valueFields[i].Value = valueFields[i].Minimum;
							}
						else
							{
							textFields[i].IsVisible = valueFields[i].IsVisible = valueLabels[i].IsVisible = false;
							}
						}

					actLabel.Text = string.Format (RDLocale.GetText ("ActivityLabelText03"), criteria[0]);

					// Переход далее
					phase++;

					/*ShowTips (3);*/
					if (!TipsState.HasFlag (TipTypes.RateTip))
						{
						await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip03"),
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
						TipsState |= TipTypes.RateTip;
						}

					break;

				// Последовательные попытки перехода к результату (ввод рангов объектов по критериям)
				case 3:
					// Добавление математик
					List<int> objectVector = new List<int> ();
					for (int i = 0; i < masterLinesCount; i++)
						{
						if (textFields[i].Text == "")
							break;
						else
							objectVector.Add ((int)valueFields[i].Value);
						}
					objectsMaths.Add (new MakeDecisionMath (objectVector));

					// Переход к следующему шагу
					if (objectsMaths.Count < criteria.Count)
						{
						for (int i = 0; i < objects.Count; i++)
							valueFields[i].Value = valueFields[i].Minimum;

						actLabel.Text = string.Format (RDLocale.GetText ("ActivityLabelText03"),
							criteria[objectsMaths.Count]);

						if (objectsMaths.Count == 1)
							{
							/*ShowTips (4);*/
							if (!TipsState.HasFlag (TipTypes.RestartTip))
								{
								await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip04"),
									RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
								TipsState |= TipTypes.RestartTip;
								}
							}
						}

					// Переход к результату
					else
						{
						// Расчёт
						List<double> result = MakeDecisionMath.EvaluateHierarchy (criteriaMath, objectsMaths);
						actLabel.Text = RDLocale.GetText ("ActivityLabelText04");

						// Подготовка максимума
						double max = result[0];
						for (int i = 1; i < objects.Count; i++)
							{
							if (max < result[i])
								max = result[i];
							}

						// Сортировка
						List<string> sortedObjects = new List<string> (objects);
						bool sorted = false;

						while (!sorted)
							{
							sorted = true;
							for (int i = 1; i < sortedObjects.Count; i++)
								{
								if (result[i] > result[i - 1])
									{
									double v = result[i];
									string s = sortedObjects[i];

									result[i] = result[i - 1];
									sortedObjects[i] = sortedObjects[i - 1];

									result[i - 1] = v;
									sortedObjects[i - 1] = s;

									sorted = false;
									}
								}
							}

						// Вывод результата
						resultLabel.Text = "";
						resultLabel.IsVisible = true;
						for (int i = 0; i < sortedObjects.Count; i++)
							{
							resultLabel.Text += ((i + 1).ToString () + ". " + sortedObjects[i] + " (" +
								((int)(100.0 * result[i] / max)).ToString () + " / 100)");
							if (i < sortedObjects.Count - 1)
								resultLabel.Text += RDLocale.RN;
							}

						// Завершение
						for (int i = 0; i < masterLinesCount; i++)
							textFields[i].IsVisible = valueFields[i].IsVisible = valueLabels[i].IsVisible = false;

						phase++;

						/*ShowTips (5);*/
						if (!TipsState.HasFlag (TipTypes.ResultTip))
							{
							await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip05"),
								RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next));
							await AndroidSupport.ShowMessage (RDLocale.GetText ("Tip06"),
								RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
							TipsState |= TipTypes.ResultTip;
							}
						}
					break;

				// Начать сначала
				case 4:
					ResetApp (false);
					break;
				}

			// Обновление состояния
			restartButton.IsEnabled = (phase > 1);
			shareButton.IsEnabled = (phase == 4);
			}

		// Реакция на изменение состава объектов
		private void CriteriaName_TextChanged (object sender, TextChangedEventArgs e)
			{
			// Контроль
			if (phase > 2)
				return;

			// Обновление
			for (int i = 1; i < masterLinesCount; i++)
				textFields[i].IsVisible = valueFields[i].IsVisible = valueLabels[i].IsVisible =
					(textFields[i - 1].Text != "") && textFields[i - 1].IsVisible;
			}

		// Метод формирует и отправляет результаты
		private async void ShareResults (object sender, EventArgs e)
			{
			// Сборка результата
			string text = ProgramDescription.AssemblyVisibleName + RDLocale.RNRN;
			text += (RDLocale.GetText ("ComparisonObjects") + RDLocale.RN);
			for (int i = 0; i < objects.Count; i++)
				text += ("• " + objects[i] + RDLocale.RN);
			text += (RDLocale.RN + RDLocale.GetText ("ComparisonCriteria") + RDLocale.RN);
			for (int i = 0; i < criteria.Count; i++)
				text += ("• " + criteria[i] + RDLocale.RN);
			text += (RDLocale.RN + actLabel.Text + RDLocale.RN + resultLabel.Text);

			// Отправка
			await Share.RequestAsync (text, ProgramDescription.AssemblyVisibleName);
			}

		#endregion
		}
	}
