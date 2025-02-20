﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму приложения
	/// </summary>
	public partial class MakeDecisionForm: Form
		{
		// Переменные
		private MakeDecisionMath criteriaMath;
		private List<MakeDecisionMath> objectsMaths = new List<MakeDecisionMath> ();
		private const uint defaultTimeout = 1000;

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		public MakeDecisionForm ()
			{
			// Инициализация
			InitializeComponent ();
			this.Text = ProgramDescription.AssemblyVisibleName;
			RDGenerics.LoadWindowDimensions (this);

			LanguageCombo.Items.AddRange (RDLocale.LanguagesNames);
			try
				{
				LanguageCombo.SelectedIndex = (int)RDLocale.CurrentLanguage;
				}
			catch
				{
				LanguageCombo.SelectedIndex = (int)RDLanguages.en_us;
				}

			BReset_Click (null, null);
			}

		/// <summary>
		/// Метод переопределяет обработку клавиатуры формой
		/// </summary>
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
			{
			switch (keyData)
				{
				// Основные вызовы
				case Keys.Home:
					BReset_Click (null, null);
					return true;

				case Keys.Return | Keys.Shift:
					BNext_Click (null, null);
					return true;

				// Отображение справки
				case Keys.F1:
					RDInterface.ShowAbout (false);
					return true;

				// Остальные клавиши обрабатываются стандартной процедурой
				default:
					return base.ProcessCmdKey (ref msg, keyData);
				}
			}

		// Локализация формы
		private void LanguageCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Сохранение языка
			RDLocale.CurrentLanguage = (RDLanguages)LanguageCombo.SelectedIndex;

			// Локализация
			RDLocale.SetControlsText (this);
			for (int i = 0; i < MainTabControl.TabCount; i++)
				RDLocale.SetControlsText (MainTabControl.TabPages[i]);

			BExit.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit);
			BNext.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next);
			BReset.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Retry);
			}

		// Закрытие приложения
		private void BExit_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		private void MakeDecisionForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			if (RDInterface.LocalizedMessageBox (RDMessageTypes.Question_Center, "AppExit",
				RDLDefaultTexts.Button_Yes, RDLDefaultTexts.Button_No) == RDMessageButtons.ButtonTwo)
				{
				e.Cancel = true;
				return;
				}

			RDGenerics.SaveWindowDimensions (this);
			}

		// Переход на следующую страницу
		private void BNext_Click (object sender, EventArgs e)
			{
			// Поиск активной вкладки
			int currentTab;
			for (currentTab = 0; currentTab < MainTabControl.TabCount; currentTab++)
				if (MainTabControl.TabPages[currentTab].Enabled)
					break;

			// Проверка дополнительных условий
			List<int> cVector;
			switch (currentTab)
				{
				// Контроль на выходе со списка объектов
				case 1:
					if (ObjectsList.Items.Count < 2)
						{
						RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center, "NotEnoughObjects",
							defaultTimeout);
						return;
						}
					break;

				// Контроль на выходе со списка критериев
				case 2:
					if (CriteriaList.Items.Count < 2)
						{
						RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center, "NotEnoughCriteria",
							defaultTimeout);
						return;
						}

					else
						{
						// Реинициализация основной матрицы сравнений
						cVector = new List<int> ();
						for (int i = 0; i < CriteriaValuesList.Items.Count; i++)
							cVector.Add (int.Parse (CriteriaValuesList.Items[i].ToString ()));

						criteriaMath = new MakeDecisionMath (cVector);

						// Сборка текстового представления
						int length = 0;
						for (int i = 0; i < CriteriaList.Items.Count; i++)
							if (CriteriaList.Items[i].ToString ().Length > length)
								length = CriteriaList.Items[i].ToString ().Length;

						ComparisonMatrix.Text = "";
						for (int r = 0; r < CriteriaList.Items.Count; r++)
							{
							ComparisonMatrix.Text += CriteriaList.Items[r].ToString ().PadRight (length);

							for (int c = 0; c < CriteriaList.Items.Count; c++)
								ComparisonMatrix.Text += ("\t" + criteriaMath.ComparisonMatrix[c][r].ToString ("0.0###"));

							ComparisonMatrix.Text += "\r\n";
							}

						// Создание таблицы оценок
						ValuesGrid.Rows.Clear ();
						ValuesGrid.Columns.Clear ();

						for (int c = 0; c < CriteriaList.Items.Count; c++)
							{
							ValuesGrid.Columns.Add (CriteriaList.Items[c].ToString (), CriteriaList.Items[c].ToString ());
							ValuesGrid.Columns[c].ValueType = Type.GetType ("System.Byte");
							ValuesGrid.Columns[c].SortMode = DataGridViewColumnSortMode.NotSortable;
							}

						for (int r = 0; r < ObjectsList.Items.Count; r++)
							{
							ValuesGrid.Rows.Add ();
							ValuesGrid.Rows[r].HeaderCell.Value = ObjectsList.Items[r].ToString ();
							}
						}
					break;

				case 4:
					// Контроль данных на выходе с интерфейса ввода оценок
					for (int r = 0; r < ValuesGrid.Rows.Count; r++)
						for (int c = 0; c < ValuesGrid.Columns.Count; c++)
							if ((ValuesGrid.Rows[r].Cells[c].Value == null) ||
								(ValuesGrid.Rows[r].Cells[c].Value.ToString () == "") ||
								(ValuesGrid.Rows[r].Cells[c].Value.ToString () == "0"))
								{
								RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center, "NotEnoughData",
									defaultTimeout);
								return;
								}

					// Ретрансляция оценок
					cVector = new List<int> ();
					objectsMaths.Clear ();

					for (int c = 0; c < ValuesGrid.Columns.Count; c++)
						{
						cVector.Clear ();
						for (int r = 0; r < ValuesGrid.Rows.Count; r++)
							cVector.Add ((byte)ValuesGrid.Rows[r].Cells[c].Value);

						objectsMaths.Add (new MakeDecisionMath (cVector));
						}

					// Определение результата
					List<double> result = MakeDecisionMath.EvaluateHierarchy (criteriaMath, objectsMaths);

					// Расчёт максимума
					double max = result[0];
					ResultsList.Items.Clear ();

					for (int i = 1; i < result.Count; i++)
						{
						if (max < result[i])
							max = result[i];
						}

					// Сортировка
					List<string> sortedObjects = new List<string> ();
					for (int i = 0; i < ObjectsList.Items.Count; i++)
						sortedObjects.Add (ObjectsList.Items[i].ToString ());
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
						ResultsList.Items.Add (((i + 1).ToString () + ". " + sortedObjects[i] + " (" +
							((int)(100.0 * result[i] / max)).ToString () + " / 100)\n"));

					// Завершение
					break;
				}

			// Переход
			MainTabControl.TabPages[currentTab].Enabled = false;
			MainTabControl.TabPages[currentTab].Text = "";

			if (SkipUnnecessarySteps.Checked && ((currentTab == 2) || (currentTab == 4)))
				{
				currentTab += 2;
				}
			else if (++currentTab >= MainTabControl.TabCount)
				{
				currentTab = 0;
				}

			MainTabControl.TabPages[currentTab].Enabled = true;
			MainTabControl.TabPages[currentTab].Text = (currentTab + 1).ToString ();
			MainTabControl.SelectedIndex = currentTab;
			}

		// Сброс
		private void BReset_Click (object sender, EventArgs e)
			{
			MainTabControl.TabPages[MainTabControl.TabCount - 1].Enabled = true;

			for (int i = 0; i < MainTabControl.TabCount - 1; i++)
				{
				MainTabControl.TabPages[i].Enabled = false;
				MainTabControl.TabPages[i].Text = "";
				}

			BNext_Click (null, null);
			}

		// Добавление объекта сравнения
		private void AddObject_Click (object sender, EventArgs e)
			{
			if (ObjectsList.Items.Count >= 100)
				{
				RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center, "TooManyObjects",
					defaultTimeout);
				return;
				}

			if (ObjectName.Text != "")
				{
				ObjectsList.Items.Add (ObjectName.Text);
				ObjectName.Text = "";
				}
			}

		private void ObjectName_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				AddObject_Click (null, null);
			}

		// Удаление объекта
		private void RemoveObject_Click (object sender, EventArgs e)
			{
			if (ObjectsList.SelectedIndex >= 0)
				ObjectsList.Items.RemoveAt (ObjectsList.SelectedIndex);
			}

		private void ObjectsList_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Delete)
				RemoveObject_Click (null, null);
			}

		// Перемещение по списку критериев
		private void CriteriaList_SelectedIndexChanged (object sender, EventArgs e)
			{
			if ((CriteriaList.SelectedIndex >= 0) && (CriteriaValuesList.SelectedIndex != CriteriaList.SelectedIndex))
				CriteriaValuesList.SelectedIndex = CriteriaList.SelectedIndex;
			}

		private void CriteriaValuesList_SelectedIndexChanged (object sender, EventArgs e)
			{
			if ((CriteriaValuesList.SelectedIndex >= 0) && (CriteriaList.SelectedIndex != CriteriaValuesList.SelectedIndex))
				CriteriaList.SelectedIndex = CriteriaValuesList.SelectedIndex;
			}

		// Добавление критерия
		private void AddCriteria_Click (object sender, EventArgs e)
			{
			if (CriteriaList.Items.Count >= 100)
				{
				RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center, "TooManyCriteria",
					defaultTimeout);
				return;
				}

			if (CriteriaName.Text != "")
				{
				CriteriaList.Items.Add (CriteriaName.Text);
				CriteriaValuesList.Items.Add (((int)CriteriaValue.Value).ToString ());
				CriteriaName.Text = "";
				}
			}

		private void CriteriaName_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				{
				CriteriaValue.Focus ();
				CriteriaValue.Select (0, CriteriaValue.Value.ToString ().Length);
				}
			}

		private void CriteriaValue_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				{
				AddCriteria_Click (null, null);
				CriteriaName.Focus ();
				}
			}

		// Удаление критерия
		private void RemoveCriteria_Click (object sender, EventArgs e)
			{
			if (CriteriaList.SelectedIndex >= 0)
				{
				CriteriaValuesList.Items.RemoveAt (CriteriaList.SelectedIndex);
				CriteriaList.Items.RemoveAt (CriteriaList.SelectedIndex);
				}
			}

		private void CriteriaList_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Delete)
				RemoveCriteria_Click (null, null);
			}

		private void CriteriaValuesList_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Delete)
				RemoveCriteria_Click (null, null);
			}

		// Контроль значений в таблице оценки
		private void ValuesGrid_DataError (object sender, DataGridViewDataErrorEventArgs e)
			{
			RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center, "IncorrectValueError",
				defaultTimeout);
			}

		// Запрос справки
		private void BAbout_Click (object sender, EventArgs e)
			{
			RDInterface.ShowAbout (false);
			}
		}
	}
