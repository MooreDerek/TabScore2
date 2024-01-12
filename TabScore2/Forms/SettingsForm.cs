// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.DataServices;

namespace TabScore2.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ISettings settings;

        public SettingsForm(ISettings iSettings, Point location)
        {
            settings = iSettings;
            InitializeComponent();
            Location = location;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            settings.GetFromDatabase(9999);  // Force settings refresh without updating settings round number
            ShowTravellerCheckbox.Checked = settings.ShowTraveller;
            ShowPercentageCheckbox.Checked = settings.ShowPercentage;
            ShowHandRecordCheckbox.Checked = settings.ShowHandRecord;
            HandRecordReversePerspectiveCheckbox.Checked = settings.HandRecordReversePerspective;
            ShowRankingCombobox.SelectedIndex = settings.ShowRanking;
            EnterLeadCardCheckbox.Checked = settings.EnterLeadCard;
            ValidateLeadCardCheckbox.Checked = settings.ValidateLeadCard;
            ManualHandEntryCheckbox.Checked = settings.ManualHandRecordEntry;
            DoubleDummyCheckbox.Checked = settings.DoubleDummy;
            NameSourceCombobox.SelectedIndex = settings.NameSource;
            NumberEntryEachRoundCheckbox.Checked = settings.NumberEntryEachRound;
            EnterResultsMethodCombobox.SelectedIndex = settings.EnterResultsMethod;
            TabletModePersonalRadioButton.Checked = settings.TabletsMove;
            TabletModeTraditionalRadioButton.Checked = !settings.TabletsMove;
            ShowTimerCheckbox.Checked = settings.ShowTimer;
            MinutesPerBoardNud.Value = Convert.ToDecimal(settings.SecondsPerBoard) / 60;
            AdditionalMinutesPerRoundNud.Value = Convert.ToDecimal(settings.AdditionalSecondsPerRound) / 60;
            SuppressRankingListNud.Value = settings.SuppressRankingList;

            ShowPercentageCheckbox.Enabled = ShowTravellerCheckbox.Checked;
            ShowHandRecordCheckbox.Enabled = ShowTravellerCheckbox.Checked;
            HandRecordReversePerspectiveCheckbox.Enabled = ShowTravellerCheckbox.Checked && ShowHandRecordCheckbox.Checked;
            ValidateLeadCardCheckbox.Enabled = EnterLeadCardCheckbox.Checked;
            DoubleDummyCheckbox.Enabled = ManualHandEntryCheckbox.Checked;
            NumberEntryEachRoundCheckbox.Enabled = !(NameSourceCombobox.SelectedIndex == 2);
            MinutesPerBoardNud.Enabled = AdditionalMinutesPerRoundNud.Enabled = MinutesPerBoardLabel.Enabled = AdditionalMinutesPerRoundLabel.Enabled = ShowTimerCheckbox.Checked;
            SuppressRankingListLabel.Enabled = SuppressRankingListNud.Enabled = (ShowRankingCombobox.SelectedIndex == 1);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            settings.ShowTraveller = ShowTravellerCheckbox.Checked;
            settings.ShowPercentage = settings.ShowHandRecord = ShowHandRecordCheckbox.Checked;
            settings.HandRecordReversePerspective = HandRecordReversePerspectiveCheckbox.Checked;
            settings.ShowRanking = ShowRankingCombobox.SelectedIndex;
            settings.EnterLeadCard = EnterLeadCardCheckbox.Checked;
            settings.ValidateLeadCard = ValidateLeadCardCheckbox.Checked;
            settings.ManualHandRecordEntry = ManualHandEntryCheckbox.Checked;
            settings.DoubleDummy = DoubleDummyCheckbox.Checked;
            settings.NameSource = NameSourceCombobox.SelectedIndex;
            settings.NumberEntryEachRound = NumberEntryEachRoundCheckbox.Checked;
            settings.EnterResultsMethod = EnterResultsMethodCombobox.SelectedIndex;
            settings.TabletsMove = TabletModePersonalRadioButton.Checked;
            settings.ShowTimer = ShowTimerCheckbox.Checked;
            settings.SecondsPerBoard = Convert.ToInt32(MinutesPerBoardNud.Value * 60);
            settings.AdditionalSecondsPerRound = Convert.ToInt32(AdditionalMinutesPerRoundNud.Value * 60);
            settings.SuppressRankingList = Convert.ToInt32(SuppressRankingListNud.Value);
            settings.UpdateDatabase();
            Close();
        }

        private void ShowTraveller_CheckedChanged(object sender, EventArgs e)
        {
            ShowPercentageCheckbox.Enabled = ShowHandRecordCheckbox.Enabled = ShowTravellerCheckbox.Checked;
            HandRecordReversePerspectiveCheckbox.Enabled = ShowTravellerCheckbox.Checked && ShowHandRecordCheckbox.Checked;
        }

        private void ShowHandRecord_CheckedChanged(object sender, EventArgs e)
        {
            HandRecordReversePerspectiveCheckbox.Enabled = ShowTravellerCheckbox.Checked && ShowHandRecordCheckbox.Checked;
        }

        private void EnterLeadCard_CheckedChanged(object sender, EventArgs e)
        {
            ValidateLeadCardCheckbox.Enabled = EnterLeadCardCheckbox.Checked;
        }

        private void ManualHandRecordEntryCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            DoubleDummyCheckbox.Enabled = ManualHandEntryCheckbox.Checked;
        }

        private void ShowTimerCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            MinutesPerBoardNud.Enabled = AdditionalMinutesPerRoundNud.Enabled = MinutesPerBoardLabel.Enabled = AdditionalMinutesPerRoundLabel.Enabled = ShowTimerCheckbox.Checked;
        }

        private void ShowRankingCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuppressRankingListLabel.Enabled = SuppressRankingListNud.Enabled = (ShowRankingCombobox.SelectedIndex == 1);
        }

        private void NameSourceCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            NumberEntryEachRoundCheckbox.Enabled = !(NameSourceCombobox.SelectedIndex == 2);
        }
    }
}

