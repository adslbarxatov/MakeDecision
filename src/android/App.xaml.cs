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
		#region Настройки стилей отображения

		private int masterFontSize = 18;
		private Thickness margin = new Thickness (6);
		private const int masterLinesCount = 5;
		private uint phase = 1;

		private List<string> objects = new List<string> (),
			criteria = new List<string> ();
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
		private Button resetButton, nextButton;

		#endregion

		#region Вспомогательные методы

		private ContentPage ApplyPageSettings (string PageName, string PageTitle, Color PageBackColor)
			{
			// Инициализация страницы
			ContentPage page = (ContentPage)MainPage.FindByName (PageName);
			page.Title = PageTitle;
			page.BackgroundColor = PageBackColor;

			ApplyHeaderLabelSettings (page, PageTitle, PageBackColor);

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

			childSlider.Maximum = 10;
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

			solutionPage = ApplyPageSettings ("SolutionPage", "Принятие решений", solutionMasterBackColor);
			aboutPage = ApplyPageSettings ("AboutPage", "О приложении", aboutMasterBackColor);

			#region Основная страница

			resetButton = ApplyButtonSettings (solutionPage, "ResetButton", "Заново",
				solutionFieldBackColor, ResetButton_Clicked);
			nextButton = ApplyButtonSettings (solutionPage, "NextButton", "Далее",
				solutionFieldBackColor, NextButton_Clicked);

			activityLabel = ApplyLabelSettings (solutionPage, "ActivityLabel", "",
				masterTextColor);

			for (int i = 0; i < masterLinesCount; i++)
				{
				objectsFields[i] = ApplyEditorSettings (solutionPage, "ObjectField" + i.ToString ("D02"),
					solutionFieldBackColor, Keyboard.Default, 50, "", ObjectName_TextChanged);
				textFields[i] = ApplyEditorSettings (solutionPage, "TextField" + i.ToString ("D02"),
					solutionFieldBackColor, Keyboard.Default, 50, "", CriteriaName_TextChanged);
				valueFields[i] = ApplySliderSettings (solutionPage, "ValueField" + i.ToString ("D02"));
				}

			// Получение настроек перед инициализацией
			for (int i = 0; i < masterLinesCount; i++)
				{
				try
					{
					objects.Add (Preferences.Get ("Object" + i.ToString ("D2"), ""));
					criteria.Add (Preferences.Get ("Criteria" + i.ToString ("D2"), ""));
					}
				catch { }
				}


			ResetButton_Clicked (null, null);

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

			ApplyButtonSettings (aboutPage, "AppPage",
				"Перейти на страницу проекта", aboutFieldBackColor, AppButton_Clicked);
			ApplyButtonSettings (aboutPage, "ADPPage",
				"Политика и EULA", aboutFieldBackColor, ADPButton_Clicked);
			ApplyButtonSettings (aboutPage, "CommunityPage",
				"RD AAOW Free utilities production lab", aboutFieldBackColor, CommunityButton_Clicked);

			#endregion

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

		// Страница политики и EULA
		private void ADPButton_Clicked (object sender, EventArgs e)
			{
			Launcher.OpenAsync ("https://vk.com/@rdaaow_fupl-adp");
			}

		// Сброс на исходное состояние
		private void ResetButton_Clicked (object sender, EventArgs e)
			{
			// Сброс состояния
			phase = 1;
			activityLabel.Text = "Укажите, что необходимо сравнить";

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

			for (int i = 0; i < objects.Count; i++)
				objectsFields[i].Text = objects[i];
			objects.Clear ();

			for (int i = 0; i < criteria.Count; i++)
				textFields[i].Text = criteria[i];
			criteria.Clear ();

			objectsMaths.Clear ();
			}

		// Реакция на изменение состава объектов
		private void ObjectName_TextChanged (object sender, TextChangedEventArgs e)
			{
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
							"Недостаточно вариантов для сравнения", "OK");
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

					activityLabel.Text = "Задайте критерии сравнения и оцените важность каждого из них";

					// Принудительный вызов на случай уже имеющихся значений полей
					CriteriaName_TextChanged (null, null);

					// Переход далее
					phase++;

					break;

				// Переход к ранжированию критериев сравнения
				case 2:
					// Контроль достаточности объектов
					if (!textFields[2].IsVisible)    // Возникает при заполнении первых двух строк
						{
						solutionPage.DisplayAlert (ProgramDescription.AssemblyTitle,
							"Недостаточно критериев для сравнения", "OK");
						return;
						}

					// Перенос
					List<int> criteriaVector = new List<int> ();
					for (int i = 0; i < masterLinesCount; i++)
						{
						if (textFields[i].Text == "")
							{
							break;
							}
						else
							{
							criteria.Add (textFields[i].Text);
							criteriaVector.Add ((int)valueFields[i].Value);
							}
						}
					criteriaMath = new MakeDecisionMath (criteriaVector);

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

					activityLabel.Text = "Оцените варианты по критерию «" + criteria[0] + "»";

					// Переход далее
					phase++;

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
						activityLabel.Text = "Оцените варианты по критерию «" + criteria[objectsMaths.Count] + "»";
						}

					// Переход к результату
					else
						{
						// Расчёт
						List<double> result = MakeDecisionMath.EvaluateHierarchy (criteriaMath, objectsMaths);
						int maxIndex = 0;
						double max = result[maxIndex];

						// Результат
						activityLabel.Text = "Результаты анализа иерархий:\n\n";
						for (int i = 0; i < objects.Count; i++)
							{
							activityLabel.Text += (objects[i] + " – " + result[i].ToString ("0.0##") + "\n");

							if (max < result[i])
								{
								max = result[i];
								maxIndex = i;
								}
							}
						activityLabel.Text += ("\n\nСамый подходящий вариант – " + objects[maxIndex]);

						// Завершение
						for (int i = 0; i < masterLinesCount; i++)
							textFields[i].IsVisible = valueFields[i].IsVisible = false;

						phase++;
						}
					break;

				// Начать сначала
				case 4:
					ResetButton_Clicked (null, null);
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
			for (int i = 0; i < masterLinesCount; i++)
				{
				try
					{
					Preferences.Set ("Object" + i.ToString ("D2"),
						(phase < 2) ? objectsFields[i].Text : objects[i]);
					Preferences.Set ("Criteria" + i.ToString ("D2"),
						(phase < 2) ? textFields[i].Text : criteria[i]);
					}
				catch { }
				}
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
