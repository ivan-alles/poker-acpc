/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ai.pkr.metabots;
using ai.pkr.metagame;
using System.Diagnostics;

namespace ai.pkr.bifaces.agentgui_exe
{
    public partial class MainWindow : Form
    {
        #region Public

        public MainWindow()
        {
            InitializeComponent();
        }

        public IPlayer Player
        {
            set;
            get;
        }

        public GameDefinition GameDef
        {
            set;
            get;
        }

        #endregion

        #region Private types


        /// <summary>
        /// Game history.
        /// </summary>
        class History
        {
            public History(GameRecord gr, int gameHistoryId, int position)
            {
                GameRecord = new GameRecord(gr.ToGameString());
                AgentPosition = position;
                GameHistoryId = gameHistoryId;
                Debug.WriteLine(string.Format("GameHistoryId {0}", GameHistoryId));
            }

            public readonly GameRecord GameRecord;
            public readonly int AgentPosition;
            public readonly int GameHistoryId;

            public GameState GetGameState(GameDefinition gd)
            {
                return new GameState(GameRecord, gd);
            }

            public CardSet GetDealtCards()
            {
                CardSet c = new CardSet();
                foreach(PokerAction pa in GameRecord.Actions)
                {
                    if(pa.Kind == Ak.d)
                    {
                        c |= StdDeck.Descriptor.GetCardSet(pa.Cards);
                    }
                }
                return c;
            }

            public override string ToString()
            {
                return GameRecord.ToGameString();
            }
        }

        #endregion
        #region Event Handlers

        private void btnNext_Click(object sender, EventArgs e)
        {
            _position = (_position + 1) % 2;
            NewGame();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            ProcessUserInput();
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            _position = 0;
            NewGame();
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            _position = 1;
            NewGame();
        }

        #endregion
        #region Overridables

        protected override bool ProcessCmdKey( ref Message msg, 
                                               Keys keyData )
        {
            switch (keyData)
            {
                case Keys.Tab:
                case Keys.Enter:
                    ProcessUserInput();
                    return true;
                // Use control for arrows, using Alt changes sometimes the next characters.
                case Keys.Left | Keys.Control:
                    MoveInHistory(-1);
                    return true;
                case Keys.Right | Keys.Control:
                    MoveInHistory(+1);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _playerInfo = Player.OnServerConnect();
            Player.OnSessionBegin("GuiSession", null, null);
            rePlayerInfo.AppendText(string.Format("Name: {0}\n", _playerInfo.Name));
            rePlayerInfo.AppendText(string.Format("Version: {0}\n", _playerInfo.Version.ToString()));
            rePlayerInfo.AppendText("Properties:\n");
            foreach (string propName in _playerInfo.Properties.Names)
            {
                rePlayerInfo.AppendText(string.Format("{0}: {1}\n", propName, _playerInfo.Properties.Get(propName)));
            }
            NewGame();
        }

        #endregion
        #region Private functions

        /// <summary>
        /// Starts a new game. Game history is always preserved, new history item is inserted to the end,
        /// regardless to the current postion.
        /// </summary>
        void NewGame()
        {
            _gameHistoryId = _history.Count == 0 ? 0 : _history[_history.Count-1].GameHistoryId + 1;


            _gameState = new GameState(GameDef, 2);
            _gameRecord = new GameRecord();
            _gameRecord.Id = _gameHistoryId.ToString(); // Use this to show game history ID.

            _gameRecord.Players.Add(new GameRecord.Player());
            _gameRecord.Players.Add(new GameRecord.Player());

            _gameRecord.Players[_position].Name = _playerInfo.Name;
            _gameRecord.Players[1 - _position].Name = "O";

            for (int p = 0; p < _gameRecord.Players.Count; ++p)
            {
                _gameRecord.Players[p].Blind = _gameState.Players[p].InPot;
            }

            _dealtCards.Clear();

            reUserInput.Text = "";
            PushHistory();

            ShowGameState();
            reUserInput.Select();

            // Do not call Player.OnGameBegin() here to be able to undo a new game started by mistake.
            // It will be done later, when the first action from the player is required.
        }

        private void ProcessUserInput()
        {
            if (_gameHistoryId == -1)
            {
                ShowMessage("Game is not started");
                return;
            }

            if (_gameState.IsGameOver)
            {
                ShowMessage("Game over");
                return;
            }

            if (string.IsNullOrEmpty(reUserInput.Text.Trim()))
            {
                ShowMessage("No input provided");
                return;
            }

            if (!ChangeHistory())
            {
                return;
            }

            bool result = true;
            string currentDeal = "";
            string text = reUserInput.Text;
            for(int p = 0; result && p < text.Length; ++p)
            {
                if (text[p] == ' ')
                {
                    continue;
                }
                if (text[p] == 'f')
                {
                    result = FinalizeDeal(ref currentDeal) && ProcessAction(Ak.f);
                }
                else if (text[p] == 'c')
                {
                    result = FinalizeDeal(ref currentDeal)&& ProcessAction(Ak.c);
                }
                else if (text[p] == 'r')
                {
                    result = FinalizeDeal(ref currentDeal) && ProcessAction(Ak.r);
                }
                else 
                {
                    result = ProcessDeal(text, ref p, ref currentDeal);
                    p--;
                }
            }
            result = result && FinalizeDeal(ref currentDeal);
            if (!result)
            {
                ApplyHistory(_history[_history.Count-1]);
                return;
            }

            if (_gameState.IsPlayerActing(_position))
            {
                PokerAction response = GetAgentResponse();
                _gameRecord.Actions.Add(response);
                _gameState.UpdateByAction(response, GameDef);
            }
            _gameRecord.IsGameOver = _gameState.IsGameOver;

            // User input has been accepted. Therefore first clear and then push a history.
            reUserInput.Text = "";
            PushHistory();
            ShowMessage("");

            ShowGameState();
        }

        bool HaveAgentActed()
        {
            foreach (PokerAction pa in _gameRecord.Actions)
            {
                if (pa.IsPlayerAction(_position))
                {
                    return true;
                }
            }
            return false;
        }

        private PokerAction GetAgentResponse()
        {
            // Check first if we have to call OnGameBegin().
            // We do it as late as possible to enable undo.
            // Only OnGameBegin() and OnActionRequired() are used
            // to interact with the bot during the game,
            // all interaction is currently done from this function.

            Cursor.Current = Cursors.WaitCursor;
            if (!HaveAgentActed())
            {
                GameRecord startGameRecord = new GameRecord(_gameRecord.ToGameString());
                startGameRecord.Actions.Clear();
                Player.OnGameBegin(startGameRecord.ToGameString());
            }
            PokerAction response = Player.OnActionRequired(_gameRecord.ToGameString());
            Cursor.Current = Cursors.Default;
            return response;
        }

        private bool ChangeHistory()
        {
            Debug.Assert(_historyPoint <= _history.Count);
            if (_historyPoint == _history.Count - 1)
            {
                // We are at the end of the history, nothing can be changed.
                return true;
            }
            // Check if we are about to remove some actions of the agent.
            // If yes, cancel the operation, because we can not restore the state of the agent.
            // Event if we replay the whole game, the agent can take different actions.
            for (int hp = _historyPoint + 1; hp < _history.Count; ++hp)
            {
                if(_history[hp-1].GameHistoryId != _history[hp].GameHistoryId)
                {
                    // Skip game bounds
                    continue;
                }
                int agentPos = _history[hp-1].AgentPosition;
                for (int a = _history[hp-1].GameRecord.Actions.Count; a < _history[hp].GameRecord.Actions.Count; ++a)
                {
                    if (_history[hp].GameRecord.Actions[a].IsPlayerAction(agentPos))
                    {
                        ShowMessage("Cannot undo agent's actions");
                        return false;
                    }
                }
            }
            _history.RemoveRange(_historyPoint + 1, _history.Count - _historyPoint - 1);
            return true;
        }

        private void MoveInHistory(int step)
        {
            int cur = _historyPoint;
            _historyPoint += step;
            _historyPoint = Math.Max(0, _historyPoint);
            _historyPoint = Math.Min(_history.Count - 1, _historyPoint);
            if (cur == _historyPoint)
            {
                return;
            }
            ApplyHistory(_history[_historyPoint]);
            reUserInput.Text = "";
        }

        void PushHistory()
        {
            History h = new History(_gameRecord, _gameHistoryId, _position);
            _history.Add(h);
            _historyPoint = _history.Count - 1;
        }

        void ApplyHistory(History h)
        {
            _position = h.AgentPosition;
            _gameState = h.GetGameState(GameDef);
            _gameRecord = new GameRecord(h.GameRecord.ToGameString());
            _dealtCards = h.GetDealtCards();
            _gameHistoryId = h.GameHistoryId;
            ShowGameState();
        }

        bool FinalizeDeal(ref string currentDeal)
        {
            if (currentDeal == "")
            {
                return true;
            }
            if (_gameState.IsPlayerActing(_position))
            {
                ShowMessage("Is's agents turn");
                return false;
            }
            List<Ak> allowedActions = _gameState.GetAllowedActions(GameDef);
            if (!allowedActions.Contains(Ak.d))
            {
                ShowMessage(string.Format("Wrong action: 'd', expected: '{0}'", ActionKindListToString(allowedActions)));
                return false;
            }
            CardSet cs = StdDeck.Descriptor.GetCardSet(currentDeal);

            int dealRound = _gameState.Round +1;
            int expectedCardsCount = dealRound == 0 ? GameDef.PrivateCardsCount[dealRound] : GameDef.SharedCardsCount[dealRound];
            if (cs.CountCards() != expectedCardsCount)
            {
                ShowMessage(string.Format("Wrong cards count: {0}, expected: {1}", cs.CountCards(), expectedCardsCount));
                return false;
            }

            if(cs.IsIntersectingWith(_dealtCards))
            {
                ShowMessage(string.Format("Some of cards {0} are already dealt", currentDeal));
                return false;
            }
            _dealtCards |= cs;

            PokerAction pa = new PokerAction {Kind = Ak.d, Cards = currentDeal};
            currentDeal = "";
            if (_gameState.Round == -1)
            {
                PokerAction oppPrivateDeal = new PokerAction { Kind = Ak.d, Cards = "" };
                if (_position == 0)
                {
                    pa.Position = 0;
                    oppPrivateDeal.Position = 1;
                    _gameRecord.Actions.Add(pa);
                    _gameRecord.Actions.Add(oppPrivateDeal);
                }
                else
                {
                    pa.Position = 1;
                    oppPrivateDeal.Position = 0;
                    _gameRecord.Actions.Add(oppPrivateDeal);
                    _gameState.UpdateByAction(oppPrivateDeal, GameDef);
                    _gameRecord.Actions.Add(pa);
                }
                _gameState.UpdateByAction(_gameRecord.Actions[_gameRecord.Actions.Count - 2], GameDef);
                _gameState.UpdateByAction(_gameRecord.Actions[_gameRecord.Actions.Count - 1], GameDef);

            }
            else
            {
                pa.Position = -1;
                _gameRecord.Actions.Add(pa);
                _gameState.UpdateByAction(pa, GameDef);
            }
            return true;
        }

        private bool ProcessDeal(string userInput, ref int startPos, ref string currentDeal)
        {
            if(startPos + 2 > userInput.Length)
            {
                ShowMessage(string.Format("Not enough characteds for a card"));
                return false;
            }
            string card = userInput.Substring(startPos, 2);
            startPos+=2;
            if (card.Length != 2)
            {
                ShowMessage(string.Format("Wrong card {0}", card));
                return false;
            }
            card = card[0].ToString().ToUpperInvariant() + card[1].ToString().ToLowerInvariant();
            if (!_ranks.Contains(card[0]))
            {
                ShowMessage(string.Format("Wrong rank in {0}", card));
                return false;
            }
            if (!_suits.Contains(card[1]))
            {
                ShowMessage(string.Format("Wrong suit in {0}", card));
                return false;
            }
            if(currentDeal != "")
            {
                currentDeal += " ";
            }
            currentDeal += card;
            return true;
        }

        private bool ProcessAction(Ak ak)
        {
            if (_gameState.IsPlayerActing(_position))
            {
                ShowMessage("Not your turn");
                return false;
            }
            List<Ak> allowedActions = _gameState.GetAllowedActions(GameDef);
            if (!allowedActions.Contains(ak))
            {
                ShowMessage(string.Format("Wrong action: '{0}', expected: '{1}'", ak, ActionKindListToString(allowedActions)));
                return false;
            }
            PokerAction pa = new PokerAction();
            pa.Kind = ak;
            pa.Position = _gameState.CurrentActor;
            if(ak == Ak.r)
            {
                pa.Amount = GameDef.BetStructure[_gameState.Round];
            }
            _gameRecord.Actions.Add(pa);
            _gameState.UpdateByAction(pa, GameDef);
            return true;
        }

        void ShowMessage(string message)
        {
            this.reMessages.Text = message;
        }

        void ShowGameState()
        {
            lblAgentPos.Text = _position == 0 ? "A 0" : "A 1";
            bool btn0Checked = _position == 0;
            btn0.Checked = btn0Checked;
            btn1.Checked = !btn0Checked;

            string gs = _gameRecord.ToGameString();
            GameRecord gr = new GameRecord(gs);
            PokerAction stressedAgentAction = null;
            if (gr.Actions.Count > 0 && gr.Actions[gr.Actions.Count - 1].IsPlayerAction(_position))
            {
                stressedAgentAction = gr.Actions[gr.Actions.Count - 1];
                gr.Actions.RemoveAt(gr.Actions.Count - 1);
                gs = gr.ToGameString();
            }

            int p = gs.IndexOf(';', 1);
            p = gs.IndexOf(';', p+1);

            reGameInfo.Text = gs.Substring(0, p);
            string actions = gs.Substring(p + 1);
            reGameRecord.Text = "";
            while(actions != "")
            {
                string roundActions = "";
                p = actions.IndexOf(" d{", Math.Min(2, actions.Length-1));
                if (p == -1)
                {
                    roundActions = actions.Trim();
                    actions = "";
                }
                else
                {
                    roundActions = actions.Substring(0, p).Trim() + "\n";
                    actions = actions.Substring(p);
                }
                reGameRecord.SelectedText = roundActions;
            }

            if (stressedAgentAction != null)
            {
                System.Drawing.Font currentFont = reGameRecord.SelectionFont;
                System.Drawing.FontStyle newFontStyle;
                newFontStyle = FontStyle.Bold;
                reGameRecord.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);
                reGameRecord.SelectedText = " we " + ActionToClearText(stressedAgentAction);
                reGameRecord.SelectionFont = currentFont;
            }
            if (!_gameState.IsGameOver)
            {
                GameState predictorState = new GameState(_gameState);
                string waitingText = "";

                while(!predictorState.IsGameOver)
                {
                    PokerAction nextAction = new PokerAction();
                    if (predictorState.IsDealerActing)
                    {
                        if (predictorState.CurrentActor != 1)
                        {
                            waitingText += (waitingText == "" ? "" : ",") + " dlr";
                        }
                        nextAction.Kind = Ak.d;
                        nextAction.Position = predictorState.Round < 0 ? 0 : -1;
                        nextAction.Cards = "?";
                    }
                    else if(predictorState.IsPlayerActing(1 - _position))
                    {
                        waitingText += (waitingText == "" ? "" : ",") + " opp";
                        // Do not predict further, because the future depends on the action of the opponent 
                        // For example: f: stop game, r - continue round, c - possibly deal.
                        break;
                    }
                    else
                    {
                        break;
                    }
                    predictorState.UpdateByAction(nextAction, GameDef);
                }
                reGameRecord.SelectedText = " waiting:";
                System.Drawing.Font currentFont = reGameRecord.SelectionFont;
                System.Drawing.FontStyle newFontStyle;
                newFontStyle = FontStyle.Bold;
                reGameRecord.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);
                reGameRecord.SelectedText = waitingText;
                reGameRecord.SelectionFont = currentFont;
            }


            string state = "";
            if (!_gameState.IsGameOver)
            {
                state += "Actor: ";
                if (_gameState.IsDealerActing)
                {
                    state += "dealer";
                }
                else
                {
                    state += _gameRecord.Players[_gameState.CurrentActor].Name;
                }
                List<Ak> allowedActions = _gameState.GetAllowedActions(GameDef);
                state += ", actions: " + ActionKindListToString(allowedActions);
            }
            else
            {
                state = "Game over";
            }
        }

        private static string ActionToClearText(PokerAction response)
        {
            switch (response.Kind)
            {
                case Ak.f:
                    return "F";
                case Ak.c:
                    return "C";
                case Ak.r:
                    return "R";
            }
            return "";
        }

        static string ActionKindListToString(List<Ak> list)
        {
            string s = "";
            foreach (Ak ak in list)
            {
                s += ak.ToString() + " ";
            }
            return s;
        }


        #endregion
        #region Data

        private static readonly string _ranks = "AKQJT98765432";
        private static readonly string _suits = "cdhs";

        private PlayerInfo _playerInfo;
        public int _position = 0;
        public GameState _gameState;
        public GameRecord _gameRecord;
        public CardSet _dealtCards = new CardSet();
        /// <summary>
        /// History ID of the current game.
        /// </summary>
        int _gameHistoryId = -1;

        int _historyPoint = 0;
        List<History> _history = new List<History>();

        #endregion

    }
}
