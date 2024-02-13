// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class Result
    {
        public int SectionID { get; set; }
        public string SectionLetter { get; set; } = string.Empty;
        public int TableNumber { get; set; }
        public int RoundNumber { get; set; }
        public int BoardNumber { get; set; } = 0;
        public int NumberNorth { get; set; }
        public int NumberEast { get; set; }
        public int NumberSouth { get; set; } = 0;
        public int NumberWest { get; set; } = 0;
        public string DeclarerNSEW { get; set; } = string.Empty;
        public bool Vulnerable { get; set; }
        public int ContractLevel { get; set; } = -999;
        public string ContractSuit { get; set; } = string.Empty;
        public string ContractX { get; set; } = string.Empty;
        public string LeadCard { get; set; } = string.Empty;
        public int TricksTaken { get; set; } = -1;
        public string TricksTakenSymbol { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public int Score { get; set; } = 0;
        public double MatchpointsNS { get; set; }
        public double MatchpointsEW { get; set; }

        public void CalculateScore()
        {
            if (DeclarerNSEW == null) return;
            if (ContractLevel <= 0) return;
            if (DeclarerNSEW == "N" || DeclarerNSEW == "S")
            {
                Vulnerable = Global.IsNSVulnerable(BoardNumber);
            }
            else
            {
                Vulnerable = Global.IsEWVulnerable(BoardNumber);
            }

            int score;
            int diff = TricksTaken - ContractLevel - 6;
            if (diff < 0)      // Contract not made
            {
                if (ContractX == string.Empty)
                {
                    if (Vulnerable)
                    {
                        score = 100 * diff;
                    }
                    else
                    {
                        score = 50 * diff;
                    }
                }
                else if (ContractX == "x")
                {
                    if (Vulnerable)
                    {
                        score = 300 * diff + 100;
                    }
                    else
                    {
                        score = 300 * diff + 400;
                        if (diff == -1) score -= 200;
                        if (diff == -2) score -= 100;
                    }
                }
                else  // ContractX = "xx"
                {
                    if (Vulnerable)
                    {
                        score = 600 * diff + 200;
                    }
                    else
                    {
                        score = 600 * diff + 800;
                        if (diff == -1) score -= 400;
                        if (diff == -2) score -= 200;
                    }
                }
            }
            else      // Contract made
            {
                // Basic score, game/part-score bonuses and making x/xx contract bonuses
                if (ContractSuit == "C" || ContractSuit == "D")
                {
                    if (ContractX == string.Empty)
                    {
                        score = 20 * (TricksTaken - 6);
                        if (ContractLevel <= 4)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                    else if (ContractX == "x")
                    {
                        score = 40 * ContractLevel + 50;
                        if (Vulnerable) score += 200 * diff;
                        else score += 100 * diff;
                        if (ContractLevel <= 2)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                    else    // ContractX = "xx"
                    {
                        score = 80 * ContractLevel + 100;
                        if (Vulnerable) score += 400 * diff;
                        else score += 200 * diff;
                        if (ContractLevel == 1)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                }
                else   // Major suits and NT
                {
                    if (ContractX == string.Empty)
                    {
                        score = 30 * (TricksTaken - 6);
                        if (ContractSuit == "NT")
                        {
                            score += 10;
                            if (ContractLevel <= 2)
                            {
                                score += 50;
                            }
                            else
                            {
                                if (Vulnerable) score += 500;
                                else score += 300;
                            }
                        }
                        else    // Major suit
                        {
                            if (ContractLevel <= 3)
                            {
                                score += 50;
                            }
                            else
                            {
                                if (Vulnerable) score += 500;
                                else score += 300;
                            }
                        }
                    }
                    else if (ContractX == "x")
                    {
                        score = 60 * ContractLevel + 50;
                        if (ContractSuit == "NT") score += 20;
                        if (Vulnerable) score += 200 * diff;
                        else score += 100 * diff;
                        if (ContractLevel <= 1)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                    else    // ContractX = "xx"
                    {
                        score = 120 * ContractLevel + 100;
                        if (ContractSuit == "NT") score += 40;
                        if (Vulnerable) score += 400 * diff + 500;
                        else score += 200 * diff + 300;
                    }
                }
                // Slam bonuses
                if (ContractLevel == 6)
                {
                    if (Vulnerable) score += 750;
                    else score += 500;
                }
                else if (ContractLevel == 7)
                {
                    if (Vulnerable) score += 1500;
                    else score += 1000;
                }
            }
            if (DeclarerNSEW == "E" || DeclarerNSEW == "W") score = -score;
            Score = score;
        }
    }
}
