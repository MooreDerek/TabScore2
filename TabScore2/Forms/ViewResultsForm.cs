// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.Extensions.Localization;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Resources;

namespace TabScore2.Forms
{
    public partial class ViewResultsForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly IDatabase database;

        private readonly List<FullResult> fullResultsList;

        public ViewResultsForm(IServiceProvider iServiceProvider, IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, Point location)
        {
            serviceProvider = iServiceProvider;
            localizer = iLocalizer;
            database = iDatabase;
            InitializeComponent();
            dataGridViewResults.SortCompare += new DataGridViewSortCompareEventHandler(DataGridViewResults_SortCompare);
            Location = location;
            fullResultsList = database.GetFullResultsList();
        }

        private void ViewResultsForm_Load(object sender, EventArgs e)
        {
            
            dataGridViewResults.AutoGenerateColumns = false;
            dataGridViewResults.Columns[0].DataPropertyName = localizer["Section"];
            dataGridViewResults.Columns[1].DataPropertyName = localizer["Table"];
            dataGridViewResults.Columns[2].DataPropertyName = localizer["Round"];
            dataGridViewResults.Columns[3].DataPropertyName = localizer["Board"];
            dataGridViewResults.Columns[4].DataPropertyName = localizer["PairNS"];
            dataGridViewResults.Columns[5].DataPropertyName = localizer["PairEW"];
            foreach (FullResult fullResult in fullResultsList)
            {
                dataGridViewResults.Rows.Add(fullResult.SectionLetter, fullResult.TableNumber, fullResult.RoundNumber, fullResult.BoardNumber, fullResult.NumberNorth, fullResult.NumberEast);
            }
            dataGridViewResults.Sort(dataGridViewResults.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
            if (dataGridViewResults.SelectedRows.Count == 0) EditResultButton.Enabled = false;
        }

        private void EditResultButton_Click(object sender, EventArgs e)
        {
            if (dataGridViewResults.SelectedRows.Count == 0) return;
            DataGridViewCellCollection selectedRowCells = dataGridViewResults.SelectedRows[0].Cells;
            FullResult selectedResult = fullResultsList.First(x =>
                x.SectionLetter == Convert.ToString(selectedRowCells[0].Value) &&
                x.TableNumber == Convert.ToInt32(selectedRowCells[1].Value) &&
                x.RoundNumber == Convert.ToInt32(selectedRowCells[2].Value) &&
                x.BoardNumber == Convert.ToInt32(selectedRowCells[3].Value) &&
                x.NumberNorth == Convert.ToInt32(selectedRowCells[4].Value) &&
                x.NumberEast == Convert.ToInt32(selectedRowCells[5].Value)
                );
            // Two-step process to inject ViewResultsForm with free parameters
            Func<FullResult, Point, EditResultForm> editResultFormTemplate = serviceProvider.GetRequiredService<Func<FullResult, Point, EditResultForm>>();
            EditResultForm editResultForm = editResultFormTemplate(selectedResult, new Point(Location.X + 30, Location.Y + 30));
            editResultForm.ShowDialog();
        }

        private void DataGridViewResults_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewResults.SelectedRows.Count > 0) EditResultButton.Enabled = true;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DataGridViewResults_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
        {
            // Compares based on Section, clicked column, Board, PairNS in that order
            string sectionLetter1 = Convert.ToString(dataGridViewResults.Rows[e.RowIndex1].Cells[0].Value) ?? string.Empty;
            string sectionLetter2 = Convert.ToString(dataGridViewResults.Rows[e.RowIndex2].Cells[0].Value) ?? string.Empty;
            e.SortResult = sectionLetter1.CompareTo(sectionLetter2);
            if (e.SortResult == 0)
            {
                if (e.CellValue1 is string)
                {
                    string cellValue1 = Convert.ToString(e.CellValue1) ?? string.Empty;
                    string cellValue2 = Convert.ToString(e.CellValue2) ?? string.Empty;
                    e.SortResult = cellValue1.CompareTo(cellValue2);
                }
                else
                {
                    int cellValue1 = Convert.ToInt32(e.CellValue1);
                    int cellValue2 = Convert.ToInt32(e.CellValue2);
                    e.SortResult = cellValue1.CompareTo(cellValue2);
                }
                if (e.SortResult == 0)
                {
                    int board1 = Convert.ToInt32(dataGridViewResults.Rows[e.RowIndex1].Cells[3].Value);
                    int board2 = Convert.ToInt32(dataGridViewResults.Rows[e.RowIndex2].Cells[3].Value);
                    e.SortResult = board1.CompareTo(board2);
                    if (e.SortResult == 0)
                    {
                        int pairNS1 = Convert.ToInt32(dataGridViewResults.Rows[e.RowIndex1].Cells[4].Value);
                        int pairNS2 = Convert.ToInt32(dataGridViewResults.Rows[e.RowIndex2].Cells[4].Value);
                        e.SortResult = pairNS1.CompareTo(pairNS2);
                    }
                }
            }
            e.Handled = true;
        }
    }
}
