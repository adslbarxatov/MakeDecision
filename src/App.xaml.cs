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
		private bool firstStart = true;

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
		private const string firstStartRegKey = "HelpShownAt";

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
		public App (bool Huawei)
			{
			// Инициализация
			InitializeComponent ();

			// Общая конструкция страниц приложения
			MainPage = new MasterPage ();

			solutionPage = AndroidSupport.ApplyPageSettings (MainPage, "SolutionPage",
				Localization.GetText ("SolutionPage"), solutionMasterBackColor);
			aboutPage = AndroidSupport.ApplyPageSettings (MainPage, "AboutPage",
				Localization.GetDefaultText (LzDefaultTextValues.Control_AppAbout),
				aboutMasterBackColor);
			AndroidSupport.SetMainPage (MainPage);

			#region Основная страница

			AndroidSupport.ApplyButtonSettings (solutionPage, "ResetButton",
				ASButtonDefaultTypes.Delete, solutionFieldBackColor, ResetButton_Clicked);
			restartButton = AndroidSupport.ApplyButtonSettings (solutionPage, "RestartButton",
				ASButtonDefaultTypes.Refresh, solutionFieldBackColor, RestartButton_Clicked);
			AndroidSupport.ApplyButtonSettings (solutionPage, "NextButton",
				ASButtonDefaultTypes.Start, solutionFieldBackColor, NextButton_Clicked);
			shareButton = AndroidSupport.ApplyButtonSettings (solutionPage, "ShareButton",
				ASButtonDefaultTypes.Share, solutionFieldBackColor, ShareResults);

			actLabel = AndroidSupport.ApplyLabelSettings (solutionPage, "ActivityLabel", "",
				ASLabelTypes.HeaderCenter);
			actLabel.FontSize += 2;

			resultLabel = AndroidSupport.ApplyLabelSettings (solutionPage, "ResultLabel", "",
				ASLabelTypes.FieldMonotype, solutionFieldBackColor);

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
					"", ASLabelTypes.Semaphore, solutionFieldBackColor);
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
			firstStart = RDGenerics.GetAppSettingsValue (firstStartRegKey) == "";

			// Инициализация зависимых полей
			ResetApp (false);

			#endregion

			#region Страница "О программе"

			aboutLabel = AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				RDGenerics.AppAboutLabelText, ASLabelTypes.AppAbout);

			AndroidSupport.ApplyButtonSettings (aboutPage, "ManualsButton",
				Localization.GetDefaultText (LzDefaultTextValues.Control_ReferenceMaterials),
				aboutFieldBackColor, ReferenceButton_Click, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "HelpButton",
				Localization.GetDefaultText (LzDefaultTextValues.Control_HelpSupport),
				aboutFieldBackColor, HelpButton_Click, false);
			AndroidSupport.ApplyLabelSettings (aboutPage, "GenericSettingsLabel",
				Localization.GetDefaultText (LzDefaultTextValues.Control_GenericSettings),
				ASLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (aboutPage, "RestartTipLabel",
				Localization.GetDefaultText (LzDefaultTextValues.Message_RestartRequired),
				ASLabelTypes.Tip);

			AndroidSupport.ApplyLabelSettings (aboutPage, "LanguageLabel",
				Localization.GetDefaultText (LzDefaultTextValues.Control_InterfaceLanguage),
				ASLabelTypes.DefaultLeft);
			languageButton = AndroidSupport.ApplyButtonSettings (aboutPage, "LanguageSelector",
				Localization.LanguagesNames[(int)Localization.CurrentLanguage],
				aboutFieldBackColor, SelectLanguage_Clicked, false);

			AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeLabel",
				Localization.GetDefaultText (LzDefaultTextValues.Control_InterfaceFontSize),
				ASLabelTypes.DefaultLeft);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeInc",
				ASButtonDefaultTypes.Increase, aboutFieldBackColor, FontSizeButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeDec",
				ASButtonDefaultTypes.Decrease, aboutFieldBackColor, FontSizeButton_Clicked);
			aboutFontSizeField = AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeField",
				" ", ASLabelTypes.DefaultCenter);

			FontSizeButton_Clicked (null, null);

			#endregion

			// Отображение подсказок первого старта
			ShowTips (Huawei ? 0u : 1u);
			}

		// Метод отображает подсказки при первом запуске
		private async void ShowTips (uint TipsNumber)
			{
			// Контроль XPUN
			await AndroidSupport.XPUNLoop (TipsNumber == 0);

			// Защита
			if (firstStart)
				{
				switch (TipsNumber)
					{
					case 0:
					case 1:
						// Требование принятия Политики
						await AndroidSupport.PolicyLoop ();
						RDGenerics.SetAppSettingsValue (firstStartRegKey, ProgramDescription.AssemblyVersion);

						// Первая подсказка
						await AndroidSupport.ShowMessage (Localization.GetText ("Tip00"),
							Localization.GetDefaultText (LzDefaultTextValues.Button_Next));
						await AndroidSupport.ShowMessage (string.Format (Localization.GetText ("Tip01"),
							masterLinesCount), Localization.GetDefaultText (LzDefaultTextValues.Button_OK));
						break;

					case 2:
					case 3:
					case 4:
						await AndroidSupport.ShowMessage (string.Format (Localization.GetText ("Tip0" +
							TipsNumber.ToString ()), masterLinesCount),
							Localization.GetDefaultText (LzDefaultTextValues.Button_OK));
						break;

					case 5:
						await AndroidSupport.ShowMessage (Localization.GetText ("Tip05"),
							Localization.GetDefaultText (LzDefaultTextValues.Button_Next));
						await AndroidSupport.ShowMessage (Localization.GetText ("Tip06"),
							Localization.GetDefaultText (LzDefaultTextValues.Button_OK));

						firstStart = false;
						break;
					}
				}

			// Подсказка о размере шрифта интерфейса
			if ((TipsNumber == 1) && AndroidSupport.AllowFontSizeTip)
				{
				await AndroidSupport.ShowMessage (
					Localization.GetDefaultText (LzDefaultTextValues.Message_FontSizeAvailable),
					Localization.GetDefaultText (LzDefaultTextValues.Button_OK));
				}
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
			await AndroidSupport.CallHelpMaterials (HelpMaterialsSets.ReferenceMaterials);
			}

		private async void HelpButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.CallHelpMaterials (HelpMaterialsSets.HelpAndSupport);
			}

		// Изменение размера шрифта интерфейса
		private void FontSizeButton_Clicked (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Xamarin.Forms.Button b = (Xamarin.Forms.Button)sender;
				if (AndroidSupport.IsNameDefault (b.Text, ASButtonDefaultTypes.Increase))
					AndroidSupport.MasterFontSize += 0.5;
				else if (AndroidSupport.IsNameDefault (b.Text, ASButtonDefaultTypes.Decrease))
					AndroidSupport.MasterFontSize -= 0.5;
				}

			aboutFontSizeField.Text = AndroidSupport.MasterFontSize.ToString ("F1");
			aboutFontSizeField.FontSize = AndroidSupport.MasterFontSize;
			}

		#endregion

		#region Рабочая зона

		// Сброс на исходное состояние
		private async void ResetButton_Clicked (object sender, EventArgs e)
			{
			if (!await AndroidSupport.ShowMessage (Localization.GetText ("ResetMessage"),
				Localization.GetDefaultText (LzDefaultTextValues.Button_Yes),
				Localization.GetDefaultText (LzDefaultTextValues.Button_No)))
				return;

			ResetApp (true);
			}

		// Запуск с начала
		private async void RestartButton_Clicked (object sender, EventArgs e)
			{
			if (!await AndroidSupport.ShowMessage (Localization.GetText ("RestartMessage"),
				Localization.GetDefaultText (LzDefaultTextValues.Button_Yes),
				Localization.GetDefaultText (LzDefaultTextValues.Button_No)))
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
			actLabel.Text = Localization.GetText ("ActivityLabelText01");
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
		private void NextButton_Clicked (object sender, EventArgs e)
			{
			switch (phase)
				{
				// Переход к указанию критериев сравнения
				case 1:
					// Контроль достаточности объектов
					if (!objectsFields[2].IsVisible)    // Возникает при заполнении первых двух строк
						{
						AndroidSupport.ShowBalloon (Localization.GetText ("NotEnoughVariants"), true);
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

					actLabel.Text = Localization.GetText ("ActivityLabelText02");

					// Принудительный вызов на случай уже имеющихся значений полей
					CriteriaName_TextChanged (null, null);

					// Переход далее
					phase++;

					ShowTips (2);

					break;

				// Переход к ранжированию критериев сравнения
				case 2:
					// Контроль достаточности объектов
					if (!textFields[2].IsVisible)    // Возникает при заполнении первых двух строк
						{
						AndroidSupport.ShowBalloon (Localization.GetText ("NotEnoughCriteria"), true);
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

					actLabel.Text = string.Format (Localization.GetText ("ActivityLabelText03"), criteria[0]);

					// Переход далее
					phase++;

					ShowTips (3);

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

						actLabel.Text = string.Format (Localization.GetText ("ActivityLabelText03"),
							criteria[objectsMaths.Count]);

						if (objectsMaths.Count == 1)
							ShowTips (4);
						}

					// Переход к результату
					else
						{
						// Расчёт
						List<double> result = MakeDecisionMath.EvaluateHierarchy (criteriaMath, objectsMaths);
						actLabel.Text = Localization.GetText ("ActivityLabelText04");

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
								resultLabel.Text += Localization.RN;
							}

						// Завершение
						for (int i = 0; i < masterLinesCount; i++)
							textFields[i].IsVisible = valueFields[i].IsVisible = valueLabels[i].IsVisible = false;

						phase++;

						ShowTips (5);
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
			string text = ProgramDescription.AssemblyVisibleName + Localization.RNRN;
			text += (Localization.GetText ("ComparisonObjects") + Localization.RN);
			for (int i = 0; i < objects.Count; i++)
				text += ("• " + objects[i] + Localization.RN);
			text += (Localization.RN + Localization.GetText ("ComparisonCriteria") + Localization.RN);
			for (int i = 0; i < criteria.Count; i++)
				text += ("• " + criteria[i] + Localization.RN);
			text += (Localization.RN + actLabel.Text + Localization.RN + resultLabel.Text);

			// Отправка
			await Share.RequestAsync (text, ProgramDescription.AssemblyVisibleName);
			}

		#endregion
		}
	}
