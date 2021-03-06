﻿using Android.Widget;
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
		private SupportedLanguages al = Localization.CurrentLanguage;

		private List<string> objects = new List<string> (),
			criteria = new List<string> ();
		private List<int> values = new List<int> ();
		private MakeDecisionMath criteriaMath;
		private List<MakeDecisionMath> objectsMaths = new List<MakeDecisionMath> ();

		private readonly Color
			solutionMasterBackColor = Color.FromHex ("#F0FFF0"),
			solutionFieldBackColor = Color.FromHex ("#D0FFD0"),

			aboutMasterBackColor = Color.FromHex ("#F0FFF0"),
			aboutFieldBackColor = Color.FromHex ("#D0FFD0");

		private const string objectsRegKey = "Object";
		private const string criteriaRegKey = "Criteria";
		private const string valuesRegKey = "Value";
		private const string firstStartRegKey = "HelpShownAt";

		#endregion

		#region Переменные страниц

		private ContentPage solutionPage, aboutPage;

		private Label aboutLabel, activityLabel;
		private Editor[] textFields = new Editor[masterLinesCount],
			objectsFields = new Editor[masterLinesCount];
		private Slider[] valueFields = new Slider[masterLinesCount];
		private Xamarin.Forms.Button restartButton, shareButton;

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

			solutionPage = AndroidSupport.ApplyPageSettings (MainPage, "SolutionPage",
				Localization.GetText ("SolutionPage", al), solutionMasterBackColor);
			aboutPage = AndroidSupport.ApplyPageSettings (MainPage, "AboutPage",
				Localization.GetText ("AboutPage", al), aboutMasterBackColor);

			#region Основная страница

			AndroidSupport.ApplyButtonSettings (solutionPage, "ResetButton",
				AndroidSupport.GetDefaultButtonName (AndroidSupport.ButtonsDefaultNames.Delete),
				solutionFieldBackColor, ResetButton_Clicked);
			restartButton = AndroidSupport.ApplyButtonSettings (solutionPage, "RestartButton",
				AndroidSupport.GetDefaultButtonName (AndroidSupport.ButtonsDefaultNames.Refresh),
				solutionFieldBackColor, RestartButton_Clicked);
			AndroidSupport.ApplyButtonSettings (solutionPage, "NextButton",
				AndroidSupport.GetDefaultButtonName (AndroidSupport.ButtonsDefaultNames.Start),
				solutionFieldBackColor, NextButton_Clicked);
			shareButton = AndroidSupport.ApplyButtonSettings (solutionPage, "ShareButton",
				AndroidSupport.GetDefaultButtonName (AndroidSupport.ButtonsDefaultNames.Share),
				solutionFieldBackColor, ShareResults);

			activityLabel = AndroidSupport.ApplyLabelSettings (solutionPage, "ActivityLabel");

			for (int i = 0; i < masterLinesCount; i++)
				{
				objectsFields[i] = AndroidSupport.ApplyEditorSettings (solutionPage, "ObjectField" + i.ToString ("D02"),
					solutionFieldBackColor, Keyboard.Default, 50, "", ObjectName_TextChanged);
				textFields[i] = AndroidSupport.ApplyEditorSettings (solutionPage, "TextField" + i.ToString ("D02"),
					solutionFieldBackColor, Keyboard.Default, 50, "", CriteriaName_TextChanged);
				valueFields[i] = AndroidSupport.ApplySliderSettings (solutionPage, "ValueField" + i.ToString ("D02"));
				}

			// Получение настроек перед инициализацией
			try
				{
				for (int i = 0; i < masterLinesCount; i++)
					{
					objects.Add (Preferences.Get (objectsRegKey + i.ToString ("D2"), ""));
					criteria.Add (Preferences.Get (criteriaRegKey + i.ToString ("D2"), ""));
					values.Add (int.Parse (Preferences.Get (valuesRegKey + i.ToString ("D2"), "1")));
					}
				firstStart = Preferences.Get (firstStartRegKey, "") == "";
				}
			catch
				{
				}

			// Инициализация зависимых полей
			ResetApp (false);

			#endregion

			#region Страница "О программе"

			aboutLabel = AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				ProgramDescription.AssemblyTitle + "\n" +
				ProgramDescription.AssemblyDescription + "\n\n" +
				ProgramDescription.AssemblyCopyright + "\nv " +
				ProgramDescription.AssemblyVersion +
				"; " + ProgramDescription.AssemblyLastUpdate,
				Color.FromHex ("#000080"));

			AndroidSupport.ApplyButtonSettings (aboutPage, "AppPage", Localization.GetText ("AppPage", al),
				aboutFieldBackColor, AppButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "ADPPage", Localization.GetText ("ADPPage", al),
				aboutFieldBackColor, ADPButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "CommunityPage",
				AndroidSupport.MasterLabName, aboutFieldBackColor, CommunityButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "DevPage", Localization.GetText ("DevPage", al),
				aboutFieldBackColor, DevButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "SolutionAboutPage", Localization.GetText ("SolutionAboutPage", al),
				aboutFieldBackColor, SolutionAboutButton_Clicked);

			AndroidSupport.ApplyButtonSettings (aboutPage, "LanguageSelector", Localization.LanguagesNames[(int)al],
				aboutFieldBackColor, SelectLanguage_Clicked);
			AndroidSupport.ApplyLabelSettings (aboutPage, "LanguageLabel", Localization.GetText ("LanguageLabel", al));

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
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("RestartApp", al),
					ToastLength.Long).Show ();
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
					// Требование принятия Политики
					while (!await solutionPage.DisplayAlert (ProgramDescription.AssemblyTitle,
										Localization.GetText ("PolicyMessage", al),
										Localization.GetText ("AcceptButton", al),
										Localization.GetText ("DeclineButton", al)))
						{
						ADPButton_Clicked (null, null);
						}
					Preferences.Set (firstStartRegKey, ProgramDescription.AssemblyVersion); // Только после принятия

					// Первая подсказка
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
		private async void AppButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (AndroidSupport.MasterGitLink + "MakeDecision");
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable", al),
					ToastLength.Long).Show ();
				}
			}

		// Страница лаборатории
		private async void CommunityButton_Clicked (object sender, EventArgs e)
			{
			List<string> comm = new List<string> {
				Localization.GetText ("CommunityVK", al), Localization.GetText ("CommunityTG", al) };
			string res = await aboutPage.DisplayActionSheet (Localization.GetText ("CommunitySelect", al),
				Localization.GetText ("CancelButton", al), null, comm.ToArray ());

			if (!comm.Contains (res))
				return;

			try
				{
				if (comm.IndexOf (res) == 0)
					await Launcher.OpenAsync (AndroidSupport.MasterCommunityLink);
				else
					await Launcher.OpenAsync (AndroidSupport.CommunityInTelegram);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable", al),
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
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable", al),
					ToastLength.Long).Show ();
				}
			}

		// Страница политики и EULA
		private async void ADPButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (AndroidSupport.ADPLink);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("WebIsUnavailable", al),
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
					To = new List<string> () { AndroidSupport.MasterDeveloperLink }
					};
				await Email.ComposeAsync (message);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, Localization.GetText ("EmailsAreUnavailable", al),
					ToastLength.Long).Show ();
				}
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
			restartButton.IsEnabled = shareButton.IsEnabled = false;

			for (int i = 0; i < masterLinesCount; i++)
				{
				objectsFields[i].IsVisible = (i == 0);
				objectsFields[i].Text = "";

				textFields[i].IsVisible = textFields[i].IsReadOnly = false;
				textFields[i].Text = "";

				valueFields[i].IsVisible = false;
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
						Toast.MakeText (Android.App.Application.Context,
							Localization.GetText ("NotEnoughVariants", al), ToastLength.Long).Show ();
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
						Toast.MakeText (Android.App.Application.Context,
							Localization.GetText ("NotEnoughCriteria", al), ToastLength.Long).Show ();
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
				textFields[i].IsVisible = valueFields[i].IsVisible =
					(textFields[i - 1].Text != "") && textFields[i - 1].IsVisible;
			}

		// Метод формирует и отправляет результаты
		private async void ShareResults (object sender, EventArgs e)
			{
			// Сборка результата
			string text = ProgramDescription.AssemblyTitle + "\n\n";
			text += (Localization.GetText ("ComparisonObjects", al) + "\n");
			for (int i = 0; i < objects.Count; i++)
				text += ("• " + objects[i] + "\n");
			text += ("\n" + Localization.GetText ("ComparisonCriteria", al) + "\n");
			for (int i = 0; i < criteria.Count; i++)
				text += ("• " + criteria[i] + "\n");
			text += ("\n" + activityLabel.Text);

			// Отправка
			await Share.RequestAsync (new ShareTextRequest
				{
				Text = text,
				Title = ProgramDescription.AssemblyTitle
				});
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
						Preferences.Set (objectsRegKey + i.ToString ("D2"), objectsFields[i].Text);
						Preferences.Set (criteriaRegKey + i.ToString ("D2"), textFields[i].Text);
						Preferences.Set (valuesRegKey + i.ToString ("D2"), ((int)valueFields[i].Value).ToString ());
						}
					else
						{
						Preferences.Set (objectsRegKey + i.ToString ("D2"), ((i < objects.Count) ? objects[i] : ""));
						Preferences.Set (criteriaRegKey + i.ToString ("D2"), ((i < criteria.Count) ? criteria[i] : ""));
						Preferences.Set (valuesRegKey + i.ToString ("D2"), ((i < values.Count) ?
							((int)values[i]).ToString () : "1"));
						}
					}

				Localization.CurrentLanguage = al;
				}
			catch { }
			}
		}
	}
