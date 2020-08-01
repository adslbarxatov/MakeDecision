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
	public partial class App:Application
		{
		#region Общие переменные и константы

		private const int masterFontSize = 14;
		private Thickness margin = new Thickness (6);
		private const int masterLinesCount = 10;
		private uint phase = 1;
		private bool firstStart = true;
		private SupportedLanguages al = Localization.CurrentLanguage;

		private List<string> objects = new List<string> (),
			criteria = new List<string> ();
		private List<int> values = new List<int> ();
		private MakeDecisionMath criteriaMath;
		private List<MakeDecisionMath> objectsMaths = new List<MakeDecisionMath> ();

		private readonly Color
			solutionMasterBackColor = Color.FromHex ("#FFFFF0"),
			solutionFieldBackColor = Color.FromHex ("#FFFFD0"),

			aboutMasterBackColor = Color.FromHex ("#F0FFF0"),
			aboutFieldBackColor = Color.FromHex ("#D0FFD0"),

			masterTextColor = Color.FromHex ("#000080"),
			masterHeaderColor = Color.FromHex ("#202020");

		#endregion

		#region Переменные страниц

		private ContentPage solutionPage, aboutPage;

		private Label aboutLabel, activityLabel;
		private Editor[] textFields = new Editor[masterLinesCount],
			objectsFields = new Editor[masterLinesCount];
		private Slider[] valueFields = new Slider[masterLinesCount];
		//private Button resetButton, restartButton, nextButton;

		#endregion

		#region Вспомогательные методы

		private ContentPage ApplyPageSettings (string PageName, Color PageBackColor)
			{
			// Инициализация страницы
			ContentPage page = (ContentPage)MainPage.FindByName (PageName);
			page.Title = Localization.GetText (PageName, al);
			page.BackgroundColor = PageBackColor;

			ApplyHeaderLabelSettings (page, page.Title, PageBackColor);

			return page;
			}

		private Label ApplyLabelSettings (ContentPage ParentPage, string LabelName,
			string LabelTitle, Color LabelTextColor)
			{
			Label childLabel = (Label)ParentPage.FindByName (LabelName);

			childLabel.Text = LabelTitle;
			childLabel.HorizontalOptions = LayoutOptions.Center;
			childLabel.FontAttributes = FontAttributes.Bold;
			childLabel.FontSize = masterFontSize;
			childLabel.TextColor = LabelTextColor;
			childLabel.Margin = margin;

			return childLabel;
			}

		private Button ApplyButtonSettings (ContentPage ParentPage, string ButtonName,
			string ButtonTitle, Color ButtonColor, EventHandler ButtonMethod)
			{
			Button childButton = (Button)ParentPage.FindByName (ButtonName);

			childButton.BackgroundColor = ButtonColor;
			childButton.FontAttributes = FontAttributes.None;
			childButton.FontSize = masterFontSize;
			childButton.TextColor = masterTextColor;
			childButton.Margin = margin;
			childButton.Text = ButtonTitle;
			if (ButtonMethod != null)
				childButton.Clicked += ButtonMethod;

			return childButton;
			}

		private Editor ApplyEditorSettings (ContentPage ParentPage, string EditorName,
			Color EditorColor, Keyboard EditorKeyboard, uint MaxLength,
			string InitialText, EventHandler<TextChangedEventArgs> EditMethod)
			{
			Editor childEditor = (Editor)ParentPage.FindByName (EditorName);

			childEditor.AutoSize = EditorAutoSizeOption.TextChanges;
			childEditor.BackgroundColor = EditorColor;
			childEditor.FontAttributes = FontAttributes.None;
			childEditor.FontFamily = "Serif";
			childEditor.FontSize = masterFontSize;
			childEditor.Keyboard = EditorKeyboard;
			childEditor.MaxLength = (int)MaxLength;
			//childEditor.Placeholder = "...";
			//childEditor.PlaceholderColor = Color.FromRgb (255, 255, 0);
			childEditor.TextColor = masterTextColor;
			childEditor.Margin = margin;

			childEditor.Text = InitialText;
			childEditor.TextChanged += EditMethod;

			return childEditor;
			}

		private void ApplyHeaderLabelSettings (ContentPage ParentPage, string LabelTitle, Color BackColor)
			{
			Label childLabel = (Label)ParentPage.FindByName ("HeaderLabel");

			childLabel.BackgroundColor = masterHeaderColor;
			childLabel.FontAttributes = FontAttributes.Bold;
			childLabel.FontSize = masterFontSize;
			childLabel.HorizontalTextAlignment = TextAlignment.Center;
			childLabel.HorizontalOptions = LayoutOptions.Fill;
			childLabel.Padding = margin;
			childLabel.Text = LabelTitle;
			childLabel.TextColor = BackColor;
			}

		private Slider ApplySliderSettings (ContentPage ParentPage, string SliderName)
			{
			Slider childSlider = (Slider)ParentPage.FindByName (SliderName);

			childSlider.Maximum = 100;
			childSlider.Minimum = 1;
			childSlider.MaximumTrackColor = childSlider.MinimumTrackColor =
				childSlider.ThumbColor = masterTextColor;
			childSlider.Value = 1;

			return childSlider;
			}

		#endregion

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App ()
			{
			// Инициализация
			InitializeComponent ();

			// Общая конструкция страниц приложения
			MainPage = new MasterPage ();

			solutionPage = ApplyPageSettings ("SolutionPage", solutionMasterBackColor);
			aboutPage = ApplyPageSettings ("AboutPage", aboutMasterBackColor);

			#region Основная страница

			/*resetButton =*/ ApplyButtonSettings (solutionPage, "ResetButton", Localization.GetText ("ResetButton", al),
				solutionFieldBackColor, ResetButton_Clicked);
			/*restartButton =*/ ApplyButtonSettings (solutionPage, "RestartButton", Localization.GetText ("RestartButton", al),
				solutionFieldBackColor, RestartButton_Clicked);
			/*nextButton =*/ ApplyButtonSettings (solutionPage, "NextButton", Localization.GetText ("NextButton", al),
				solutionFieldBackColor, NextButton_Clicked);

			activityLabel = ApplyLabelSettings (solutionPage, "ActivityLabel", "", masterTextColor);

			for (int i = 0; i < masterLinesCount; i++)
				{
				objectsFields[i] = ApplyEditorSettings (solutionPage, "ObjectField" + i.ToString ("D02"),
					solutionFieldBackColor, Keyboard.Default, 50, "", ObjectName_TextChanged);
				textFields[i] = ApplyEditorSettings (solutionPage, "TextField" + i.ToString ("D02"),
					solutionFieldBackColor, Keyboard.Default, 50, "", CriteriaName_TextChanged);
				valueFields[i] = ApplySliderSettings (solutionPage, "ValueField" + i.ToString ("D02"));
				}

			// Получение настроек перед инициализацией
			try
				{
				for (int i = 0; i < masterLinesCount; i++)
					{
					objects.Add (Preferences.Get ("Object" + i.ToString ("D2"), ""));
					criteria.Add (Preferences.Get ("Criteria" + i.ToString ("D2"), ""));
					values.Add (int.Parse (Preferences.Get ("Value" + i.ToString ("D2"), "1")));
					}
				firstStart = Preferences.Get ("FirstStart", "") == "";
				}
			catch
				{
				}

			// Инициализация зависимых полей
			ResetApp (false);

			#endregion

			#region Страница "О программе"

			aboutLabel = ApplyLabelSettings (aboutPage, "AboutLabel",
				ProgramDescription.AssemblyTitle + "\n" +
				ProgramDescription.AssemblyDescription + "\n\n" +
				ProgramDescription.AssemblyCopyright + "\nv " +
				ProgramDescription.AssemblyVersion +
				"; " + ProgramDescription.AssemblyLastUpdate,
				Color.FromHex ("#000080"));
			aboutLabel.FontAttributes = FontAttributes.Bold;
			aboutLabel.HorizontalOptions = LayoutOptions.Fill;
			aboutLabel.HorizontalTextAlignment = TextAlignment.Center;

			ApplyButtonSettings (aboutPage, "AppPage", Localization.GetText ("AppPage", al),
				aboutFieldBackColor, AppButton_Clicked);
			ApplyButtonSettings (aboutPage, "ADPPage", Localization.GetText ("ADPPage", al),
				aboutFieldBackColor, ADPButton_Clicked);
			ApplyButtonSettings (aboutPage, "CommunityPage",
				"RD AAOW Free utilities production lab", aboutFieldBackColor, CommunityButton_Clicked);
			ApplyButtonSettings (aboutPage, "SolutionAboutPage", Localization.GetText ("SolutionAboutPage", al),
				aboutFieldBackColor, SolutionAboutButton_Clicked);

			ApplyButtonSettings (aboutPage, "LanguageSelector", Localization.LanguagesNames[(int)al],
				aboutFieldBackColor, SelectLanguage_Clicked);

			#endregion

			// Отображение подсказок первого старта
			ShowTips (1);
			}

		// Выбор языка приложения
		private async void SelectLanguage_Clicked (object sender, EventArgs e)
			{
			// Запрос
			string res = await aboutPage.DisplayActionSheet (Localization.GetText ("SelectLanguage", al),
				Localization.GetText ("CancelButton", al), null, Localization.LanguagesNames);

			// Сохранение
			List<string> lngs = new List<string> (Localization.LanguagesNames);
			if (lngs.Contains (res))
				{
				al = (SupportedLanguages)lngs.IndexOf (res);
				await aboutPage.DisplayAlert (ProgramDescription.AssemblyTitle,
					Localization.GetText ("RestartApp", al), "OK");
				}
			}

		// Метод отображает подсказки при первом запуске
		private async void ShowTips (uint TipsNumber)
			{
			if (!firstStart)
				return;

			switch (TipsNumber)
				{
				case 1:
					await solutionPage.DisplayAlert (Localization.GetText ("TipHeader01", al),
						Localization.GetText ("Tip00", al), Localization.GetText ("NextButton", al));
					await solutionPage.DisplayAlert (Localization.GetText ("TipHeader02", al) + "1",
						string.Format (Localization.GetText ("Tip01", al), masterLinesCount), "OK");
					break;

				case 2:
					await solutionPage.DisplayAlert (Localization.GetText ("TipHeader02", al) + "2",
						string.Format (Localization.GetText ("Tip02", al), masterLinesCount), "OK");
					break;

				case 3:
					await solutionPage.DisplayAlert (Localization.GetText ("TipHeader02", al) + "3",
						Localization.GetText ("Tip03", al), "OK");
					break;

				case 4:
					await solutionPage.DisplayAlert (Localization.GetText ("TipHeader02", al) + "4",
						Localization.GetText ("Tip04", al), "OK");
					break;

				case 5:
					await solutionPage.DisplayAlert (Localization.GetText ("TipHeader02", al) + "5",
						Localization.GetText ("Tip05", al), Localization.GetText ("NextButton", al));
					await solutionPage.DisplayAlert (Localization.GetText ("TipHeader02", al) + "6",
						Localization.GetText ("Tip06", al), "OK");
					firstStart = false;
					break;
				}
			}

		// Страница проекта
		private void AppButton_Clicked (object sender, EventArgs e)
			{
			Launcher.OpenAsync ("https://github.com/adslbarxatov/MakeDecision");
			}

		// Страница лаборатории
		private void CommunityButton_Clicked (object sender, EventArgs e)
			{
			Launcher.OpenAsync ("https://vk.com/rdaaow_fupl");
			}

		// Страница метода иерархий
		private void SolutionAboutButton_Clicked (object sender, EventArgs e)
			{
			Launcher.OpenAsync ("https://vk.com/@rdaaow_fupl-makedecision");
			}

		// Страница политики и EULA
		private void ADPButton_Clicked (object sender, EventArgs e)
			{
			Launcher.OpenAsync ("https://vk.com/@rdaaow_fupl-adp");
			}

		// Сброс на исходное состояние
		private void ResetButton_Clicked (object sender, EventArgs e)
			{
			ResetApp (true);
			}

		// Запуск с начала
		private void RestartButton_Clicked (object sender, EventArgs e)
			{
			ResetApp (false);
			}

		private void ResetApp (bool Fully)
			{
			// Сброс состояния
			phase = 1;
			activityLabel.Text = Localization.GetText ("ActivityLabelText01", al);

			for (int i = 0; i < masterLinesCount; i++)
				{
				objectsFields[i].IsVisible = false;
				objectsFields[i].Text = "";

				textFields[i].IsVisible = textFields[i].IsReadOnly = false;
				textFields[i].Text = "";

				valueFields[i].IsVisible = false;
				valueFields[i].Value = valueFields[i].Minimum;
				}
			objectsFields[0].IsVisible = true;

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
				objectsFields[i].IsVisible = (objectsFields[i - 1].Text != "") && objectsFields[i - 1].IsVisible;
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
						solutionPage.DisplayAlert (ProgramDescription.AssemblyTitle,
							Localization.GetText ("NotEnoughVariants", al), "OK");
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
					textFields[0].IsVisible = valueFields[0].IsVisible = true;

					activityLabel.Text = Localization.GetText ("ActivityLabelText02", al);

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
						solutionPage.DisplayAlert (ProgramDescription.AssemblyTitle,
							Localization.GetText ("NotEnoughCriteria", al), "OK");
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
							textFields[i].IsVisible = valueFields[i].IsVisible = true;
							textFields[i].Text = objects[i];
							valueFields[i].Value = valueFields[i].Minimum;
							}
						else
							{
							textFields[i].IsVisible = valueFields[i].IsVisible = false;
							}
						}

					activityLabel.Text = string.Format (Localization.GetText ("ActivityLabelText03", al), criteria[0]);

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
						activityLabel.Text = string.Format (Localization.GetText ("ActivityLabelText03", al),
							criteria[objectsMaths.Count]);

						if (objectsMaths.Count == 1)
							ShowTips (4);
						}

					// Переход к результату
					else
						{
						// Расчёт
						List<double> result = MakeDecisionMath.EvaluateHierarchy (criteriaMath, objectsMaths);

						// Ра
						activityLabel.Text = Localization.GetText ("ActivityLabelText04", al);

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
						for (int i = 0; i < sortedObjects.Count; i++)
							activityLabel.Text += ((i + 1).ToString () + ". " + sortedObjects[i] + " (" +
								((int)(100.0 * result[i] / max)).ToString () + " / 100)\n");

						// Завершение
						for (int i = 0; i < masterLinesCount; i++)
							textFields[i].IsVisible = valueFields[i].IsVisible = false;

						phase++;

						ShowTips (5);
						}
					break;

				// Начать сначала
				case 4:
					ResetApp (false);
					break;
				}
			}

		// Реакция на изменение состава объектов
		private void CriteriaName_TextChanged (object sender, TextChangedEventArgs e)
			{
			// Контроль
			if (phase > 2)
				return;

			// Обновление
			for (int i = 1; i < masterLinesCount; i++)
				textFields[i].IsVisible = valueFields[i].IsVisible =
					(textFields[i - 1].Text != "") && textFields[i - 1].IsVisible;
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
						Preferences.Set ("Object" + i.ToString ("D2"), objectsFields[i].Text);
						Preferences.Set ("Criteria" + i.ToString ("D2"), textFields[i].Text);
						Preferences.Set ("Value" + i.ToString ("D2"), ((int)valueFields[i].Value).ToString ());
						}
					else
						{
						Preferences.Set ("Object" + i.ToString ("D2"), ((i < objects.Count) ? objects[i] : ""));
						Preferences.Set ("Criteria" + i.ToString ("D2"), ((i < criteria.Count) ? criteria[i] : ""));
						Preferences.Set ("Value" + i.ToString ("D2"), ((i < values.Count) ?
							((int)values[i]).ToString () : "1"));
						}
					}

				Preferences.Set ("FirstStart", "No");
				Localization.CurrentLanguage = al;
				}
			catch { }
			}

		#region Стандартные обработчики

		/// <summary>
		/// Обработчик события запуска приложения
		/// </summary>
		protected override void OnStart ()
			{
			}

		/// <summary>
		/// Обработчик события выхода из ждущего режима
		/// </summary>
		protected override void OnResume ()
			{
			}

		#endregion
		}
	}
