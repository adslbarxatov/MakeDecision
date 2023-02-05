using Android.Widget;
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

		private const int masterLinesCount = 10;
		private uint phase = 1;
		private bool firstStart = true;
		/*private SupportedLanguages al = Localization.CurrentLanguage;*/

		private List<string> objects = new List<string> (),
			criteria = new List<string> ();
		private List<int> values = new List<int> ();
		private MakeDecisionMath criteriaMath;
		private List<MakeDecisionMath> objectsMaths = new List<MakeDecisionMath> ();

		private readonly Color
			solutionMasterBackColor = Color.FromHex ("#FFDEEF"),
			solutionFieldBackColor = Color.FromHex ("#FFD2E9"),

			aboutMasterBackColor = Color.FromHex ("#F0FFF0"),
			aboutFieldBackColor = Color.FromHex ("#D0FFD0");

		private const string objectsRegKey = "Object";
		private const string criteriaRegKey = "Criteria";
		private const string valuesRegKey = "Value";
		private const string firstStartRegKey = "HelpShownAt";

		#endregion

		#region Переменные страниц

		private ContentPage solutionPage, aboutPage;

		private Label aboutLabel, actLabel, resultLabel;
		private Editor[] textFields = new Editor[masterLinesCount],
			objectsFields = new Editor[masterLinesCount];
		private List<Slider> valueFields = new List<Slider> ();
		private Label[] valueLabels = new Label[masterLinesCount];
		private Xamarin.Forms.Button restartButton, shareButton;

		#endregion

		#region Запуск и настройка

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App ()
			{
			// Инициализация
			InitializeComponent ();

			// Общая конструкция страниц приложения
			MainPage = new MasterPage ();

			solutionPage = AndroidSupport.ApplyPageSettings (MainPage, "SolutionPage",
				Localization.GetText ("SolutionPage"), solutionMasterBackColor);
			aboutPage = AndroidSupport.ApplyPageSettings (MainPage, "AboutPage",
				Localization.GetText ("AboutPage"), aboutMasterBackColor);
			AndroidSupport.SetMainPage (MainPage);

			#region Основная страница

			AndroidSupport.ApplyButtonSettings (solutionPage, "ResetButton",
				AndroidSupport.ButtonsDefaultNames.Delete, solutionFieldBackColor, ResetButton_Clicked);
			restartButton = AndroidSupport.ApplyButtonSettings (solutionPage, "RestartButton",
				AndroidSupport.ButtonsDefaultNames.Refresh, solutionFieldBackColor, RestartButton_Clicked);
			AndroidSupport.ApplyButtonSettings (solutionPage, "NextButton",
				AndroidSupport.ButtonsDefaultNames.Start, solutionFieldBackColor, NextButton_Clicked);
			shareButton = AndroidSupport.ApplyButtonSettings (solutionPage, "ShareButton",
				AndroidSupport.ButtonsDefaultNames.Share, solutionFieldBackColor, ShareResults);

			actLabel = AndroidSupport.ApplyLabelSettings (solutionPage, "ActivityLabel", "",
				AndroidSupport.LabelTypes.HeaderCenter);
			/*activityLabel.HorizontalTextAlignment = TextAlignment.Center;*/
			actLabel.FontSize += 2;

			resultLabel = AndroidSupport.ApplyLabelSettings (solutionPage, "ResultLabel", "",
				AndroidSupport.LabelTypes.FieldMonotype, solutionFieldBackColor);

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
					"", AndroidSupport.LabelTypes.Semaphore, solutionFieldBackColor);
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
				ProgramDescription.AssemblyTitle + "\n" +
				ProgramDescription.AssemblyDescription + "\n\n" +
				RDGenerics.AssemblyCopyright + "\nv " +
				ProgramDescription.AssemblyVersion +
				"; " + ProgramDescription.AssemblyLastUpdate,
				AndroidSupport.LabelTypes.AppAbout);
			/*aboutLabel.FontAttributes = FontAttributes.Bold;
			aboutLabel.HorizontalTextAlignment = TextAlignment.Center;*/

			AndroidSupport.ApplyButtonSettings (aboutPage, "AppPage", Localization.GetText ("AppPage"),
				aboutFieldBackColor, AppButton_Clicked, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "ADPPage", Localization.GetText ("ADPPage"),
				aboutFieldBackColor, ADPButton_Clicked, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "CommunityPage", RDGenerics.AssemblyCompany,
				aboutFieldBackColor, CommunityButton_Clicked, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "DevPage", Localization.GetText ("DevPage"),
				aboutFieldBackColor, DevButton_Clicked, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "SolutionAboutPage",
				Localization.GetText ("SolutionAboutPage"), aboutFieldBackColor, SolutionAboutButton_Clicked,
				false);

			AndroidSupport.ApplyButtonSettings (aboutPage, "LanguageSelector",
				Localization.LanguagesNames[(int)Localization.CurrentLanguage],
				aboutFieldBackColor, SelectLanguage_Clicked, false);
			AndroidSupport.ApplyLabelSettings (aboutPage, "LanguageLabel",
				Localization.GetText ("LanguageLabel"), AndroidSupport.LabelTypes.DefaultLeft);

			if (Localization.CurrentLanguage == SupportedLanguages.ru_ru)
				AndroidSupport.ApplyLabelSettings (aboutPage, "Alert", RDGenerics.RuAlertMessage,
					AndroidSupport.LabelTypes.DefaultLeft);

			#endregion

			// Отображение подсказок первого старта
			ShowTips (1);
			}

		// Метод отображает подсказки при первом запуске
		private async void ShowTips (uint TipsNumber)
			{
			// Контроль XPR
			while (!Localization.IsXPRClassAcceptable)
				await AndroidSupport.ShowMessage (Localization.InacceptableXPRClassMessage, "   ");

			// Защита
			if (!firstStart)
				return;

			switch (TipsNumber)
				{
				case 1:
					// Требование принятия Политики
					while (!await AndroidSupport.ShowMessage (Localization.GetText ("PolicyMessage"),
						Localization.GetDefaultButtonName (Localization.DefaultButtons.Accept),
						Localization.GetText ("DeclineButton")))
						{
						ADPButton_Clicked (null, null);
						}
					RDGenerics.SetAppSettingsValue (firstStartRegKey, ProgramDescription.AssemblyVersion);

					// Первая подсказка
					await AndroidSupport.ShowMessage (Localization.GetText ("Tip00"),
						Localization.GetDefaultButtonName (Localization.DefaultButtons.Next));
					await AndroidSupport.ShowMessage (string.Format (Localization.GetText ("Tip01"),
						masterLinesCount), Localization.GetDefaultButtonName (Localization.DefaultButtons.OK));
					break;

				case 2:
				case 3:
				case 4:
					await AndroidSupport.ShowMessage (string.Format (Localization.GetText ("Tip0" +
						TipsNumber.ToString ()), masterLinesCount),
						Localization.GetDefaultButtonName (Localization.DefaultButtons.OK));
					break;

				case 5:
					await AndroidSupport.ShowMessage (Localization.GetText ("Tip05"),
						Localization.GetDefaultButtonName (Localization.DefaultButtons.Next));
					await AndroidSupport.ShowMessage (Localization.GetText ("Tip06"),
						Localization.GetDefaultButtonName (Localization.DefaultButtons.OK));

					firstStart = false;
					break;
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

				/*Localization.CurrentLanguage = al;*/
				}
			catch { }
			}

		#endregion

		#region О приложении

		// Выбор языка приложения
		private async void SelectLanguage_Clicked (object sender, EventArgs e)
			{
			// Запрос
			string res = await
				AndroidSupport.ShowList (Localization.GetDefaultButtonName (Localization.DefaultButtons.LanguageSelector),
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel),
				Localization.LanguagesNames);

			// Сохранение
			List<string> lngs = new List<string> (Localization.LanguagesNames);
			if (lngs.Contains (res))
				{
				Localization.CurrentLanguage = (SupportedLanguages)lngs.IndexOf (res);
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("RestartApp"),
					ToastLength.Long).Show ();
				}
			}

		// Страница проекта
		private async void AppButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (RDGenerics.AssemblyGitLink + ProgramDescription.AssemblyMainName);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable"),
					ToastLength.Long).Show ();
				}
			}

		// Страница лаборатории
		private async void CommunityButton_Clicked (object sender, EventArgs e)
			{
			bool ru = (Localization.CurrentLanguage == SupportedLanguages.ru_ru);
			string[] comm = RDGenerics.GetCommunitiesNames (!ru);
			string res = await AndroidSupport.ShowList (Localization.GetText ("CommunitySelect"),
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), comm);

			res = RDGenerics.GetCommunityLink (res, !ru);
			if (string.IsNullOrWhiteSpace (res))
				return;

			try
				{
				await Launcher.OpenAsync (res);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable"),
					ToastLength.Long).Show ();
				}
			}

		// Страница метода иерархий
		private async void SolutionAboutButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (ProgramDescription.AssemblyManualLink);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable"),
					ToastLength.Long).Show ();
				}
			}

		// Страница политики и EULA
		private async void ADPButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (RDGenerics.GetADPLink (Localization.CurrentLanguage ==
					SupportedLanguages.ru_ru));
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable"),
					ToastLength.Long).Show ();
				}
			}

		// Страница политики и EULA
		private async void DevButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				EmailMessage message = new EmailMessage
					{
					Subject = "Wish, advice or bug in " + ProgramDescription.AssemblyTitle,
					Body = "",
					To = new List<string> () { RDGenerics.LabMailLink }
					};
				await Email.ComposeAsync (message);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("EmailsAreUnavailable"),
					ToastLength.Long).Show ();
				}
			}

		#endregion

		#region Рабочая зона

		// Сброс на исходное состояние
		private async void ResetButton_Clicked (object sender, EventArgs e)
			{
			if (!await AndroidSupport.ShowMessage (Localization.GetText ("ResetMessage"),
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Yes),
				Localization.GetDefaultButtonName (Localization.DefaultButtons.No)))
				return;

			ResetApp (true);
			}

		// Запуск с начала
		private async void RestartButton_Clicked (object sender, EventArgs e)
			{
			if (!await AndroidSupport.ShowMessage (Localization.GetText ("RestartMessage"),
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Yes),
				Localization.GetDefaultButtonName (Localization.DefaultButtons.No)))
				return;

			ResetApp (false);
			}

		/*private void SetValueField (int FieldIndex, double Value)
			{
			valueFields[FieldIndex].Value = Value;
			valueLabels[FieldIndex].Text = valueFields[FieldIndex].Value.ToString () + "%";
			}*/

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
				/*valueFields[i].Value = valueFields[i].Minimum;
				valueLabels[i].Text = valueFields[i].Value.ToString () + "%";*/
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
					{
					/*valueFields[i].Value = values[i];
					valueLabels[i].Text = valueFields[i].Value.ToString () + "%";*/
					valueFields[i].Value = values[i];
					}
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
						Toast.MakeText (Android.App.Application.Context,
							Localization.GetText ("NotEnoughVariants"), ToastLength.Long).Show ();
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
						Toast.MakeText (Android.App.Application.Context,
							Localization.GetText ("NotEnoughCriteria"), ToastLength.Long).Show ();
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

							/*valueFields[i].Value = valueFields[i].Minimum;
							valueLabels[i].Text = valueFields[i].Value.ToString () + "%";*/
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
							{
							/*valueFields[i].Value = valueFields[i].Minimum;
							valueLabels[i].Text = valueFields[i].Value.ToString () + "%";*/
							valueFields[i].Value = valueFields[i].Minimum;
							}
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
								resultLabel.Text += "\n";
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
			string text = ProgramDescription.AssemblyVisibleName + "\n\n";
			text += (Localization.GetText ("ComparisonObjects") + "\n");
			for (int i = 0; i < objects.Count; i++)
				text += ("• " + objects[i] + "\n");
			text += ("\n" + Localization.GetText ("ComparisonCriteria") + "\n");
			for (int i = 0; i < criteria.Count; i++)
				text += ("• " + criteria[i] + "\n");
			text += ("\n" + actLabel.Text + "\n" + resultLabel.Text);

			// Отправка
			await Share.RequestAsync (text, ProgramDescription.AssemblyVisibleName);
			}

		#endregion
		}
	}
