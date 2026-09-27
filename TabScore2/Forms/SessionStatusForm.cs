// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.Extensions.Localization;
using System.Net;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Resources;

namespace TabScore2.Forms
{
    // Read-only admin view of the state of play.  Opened non-modally from MainForm, so it can stay open while play starts.
    // Device registrations are re-checked every few seconds; player names come from the database and are refreshed
    // every 30 seconds by the SessionMonitor cache, or immediately with the Refresh button.
    public partial class SessionStatusForm : Form
    {
        private static readonly Color OkColour = Color.FromArgb(220, 245, 220);
        private static readonly Color ProblemColour = Color.FromArgb(255, 215, 210);

        private readonly ISessionMonitor sessionMonitor;
        private readonly IStringLocalizer<Strings> localizer;
        private bool refreshing = false;

        public SessionStatusForm(IServiceProvider iServiceProvider, Point location)
        {
            sessionMonitor = iServiceProvider.GetRequiredService<ISessionMonitor>();
            localizer = iServiceProvider.GetRequiredService<IStringLocalizer<Strings>>();
            Location = location;
            InitializeComponent();
        }

        private async void SessionStatusForm_Load(object sender, EventArgs e)
        {
            Text = $"TabScore2 - {localizer["MonitorTitle"]}";
            tabPageStartOfPlay.Text = localizer["MonitorStartOfPlay"];
            columnSection.HeaderText = localizer["Section"];
            columnTable.HeaderText = localizer["Table"];
            columnNorthSouth.HeaderText = localizer["MonitorNS"];
            columnEastWest.HeaderText = localizer["MonitorEW"];
            columnRegistration.HeaderText = localizer["MonitorDevice"];
            columnNamesNorthSouth.HeaderText = localizer["MonitorNamesNS"];
            columnNamesEastWest.HeaderText = localizer["MonitorNamesEW"];
            buttonRefreshNames.Text = localizer["MonitorRefreshNames"];
            buttonClose.Text = localizer["MonitorClose"];
            labelScorerPage.Text = ScorerPageText();

            await RefreshStatus(reloadLayout: true);
            timerRefresh.Start();
        }

        private async void TimerRefresh_Tick(object? sender, EventArgs e)
        {
            await RefreshStatus(reloadLayout: false);
        }

        private async void ButtonRefreshNames_Click(object sender, EventArgs e)
        {
            await RefreshStatus(reloadLayout: true);
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SessionStatusForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerRefresh.Stop();
        }

        private async Task RefreshStatus(bool reloadLayout)
        {
            if (refreshing) return;  // A slow database read is still in progress; skip this tick
            refreshing = true;
            buttonRefreshNames.Enabled = false;
            try
            {
                // Run on a worker thread: a layout reload makes gRPC/database calls, and the UI must stay responsive
                SessionCheck check = await Task.Run(() => sessionMonitor.GetStartOfPlayCheck(reloadLayout));
                if (IsDisposed) return;
                Display(check);
            }
            catch (Exception)
            {
                if (IsDisposed) return;
                labelSummary.Text = localizer["MonitorDatabaseError"];
                labelSummary.ForeColor = Color.DarkRed;
            }
            finally
            {
                refreshing = false;
                if (!IsDisposed) buttonRefreshNames.Enabled = true;
            }
        }

        private void Display(SessionCheck check)
        {
            labelSummary.Text = SessionCheckText.Summary(check, localizer);
            labelSummary.ForeColor = (check.ErrorMessage == null && check.Rows.Count > 0 && check.TablesNeedingAttention == 0) ? Color.Green : Color.DarkRed;
            labelNamesChecked.Text = check.ErrorMessage == null ? localizer["MonitorNamesChecked", check.LayoutLoadedAt.ToString("T")] : string.Empty;

            string unexpected = SessionCheckText.Unexpected(check, localizer);
            labelUnexpected.Text = unexpected;
            labelUnexpected.Visible = unexpected != string.Empty;

            // Update rows in place when the table list hasn't changed, so the selection and scroll position are kept
            if (dataGridViewTables.Rows.Count != check.Rows.Count)
            {
                dataGridViewTables.Rows.Clear();
                if (check.Rows.Count > 0) dataGridViewTables.Rows.Add(check.Rows.Count);
            }
            for (int i = 0; i < check.Rows.Count; i++)
            {
                SetRow(dataGridViewTables.Rows[i], check.Rows[i]);
            }
        }

        private void SetRow(DataGridViewRow gridRow, TableCheckRow row)
        {
            gridRow.Cells[columnSection.Index].Value = row.Table.SectionLetter;
            gridRow.Cells[columnTable.Index].Value = row.Table.TableNumber;
            gridRow.Cells[columnNorthSouth.Index].Value = SessionCheckText.PairNumbers(row, northSouth: true);
            gridRow.Cells[columnEastWest.Index].Value = SessionCheckText.PairNumbers(row, northSouth: false);

            DataGridViewCell registrationCell = gridRow.Cells[columnRegistration.Index];
            registrationCell.Value = SessionCheckText.Registration(row, localizer);
            registrationCell.Style.BackColor = row.AllDevicesRegistered ? OkColour : ProblemColour;

            SetNamesCell(gridRow.Cells[columnNamesNorthSouth.Index], row, northSouth: true);
            SetNamesCell(gridRow.Cells[columnNamesEastWest.Index], row, northSouth: false);
        }

        private void SetNamesCell(DataGridViewCell cell, TableCheckRow row, bool northSouth)
        {
            cell.Value = SessionCheckText.Names(row, northSouth, localizer);
            NamesState state = northSouth ? row.NorthSouthNames : row.EastWestNames;
            cell.Style.BackColor = state switch
            {
                NamesState.Complete => OkColour,
                NamesState.Missing => ProblemColour,
                _ => SystemColors.Window
            };
        }

        // The scorer can open the same check on their own device, from the TabScore2 web server
        private string ScorerPageText()
        {
            try
            {
                IPAddress? ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ipAddress == null) return string.Empty;
                return localizer["MonitorScorerPage", $"http://{ipAddress}:{Program.WebAppPort}/SessionStatus"];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
