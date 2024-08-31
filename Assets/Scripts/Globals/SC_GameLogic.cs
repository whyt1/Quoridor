using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyQuoridorApp;
using static MyQuoridorApp.GlobalConstants;
using com.shephertz.app42.gaming.multiplayer.client;
public class SC_GameLogic : MonoBehaviour
{
    #region Singleton
    private static SC_GameLogic instance;
    public static SC_GameLogic Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject _colorsManager = GameObject.FindGameObjectWithTag("GameLogic");
                if (_colorsManager == null)
                {
                    _colorsManager = new GameObject("SC_GameLogic", typeof(SC_GameLogic));
                }
                instance = _colorsManager.InitComponent<SC_GameLogic>();
            }
            return instance;
        }
    }
    #endregion
    
    #region Variables

    [Header("Managers")]
    [SerializeField]
    BoardManager board = null;
    [SerializeField]
    public PlayersManager players = null;
    [SerializeField]
    HistoryManager history = null;
    
    // Flags
    public bool IsMyTurn => !IsGameOver && players != null && (players.Current == players.myPlayer);
    [SerializeField]
    bool IsGameOver => Winner != null;
    public bool isMultiplayer = false;

    // Properties
    public SC_PlayerLogic MyPlayer => players?.myPlayer;

    [SerializeField]
    public SC_PlayerLogic Winner = null;

    [SerializeField]
    public SC_BarrierLogic PlacingBarrier = null;
    
    HashSet<SC_SquareLogic> possibleMoves = new();
    /// <summary>
    /// Gets a list of all possible moves for current player.
    /// </summary>
    public HashSet<SC_SquareLogic> PossibleMoves 
    {
        get
        {
            if (IsMyTurn && possibleMoves.Count == 0)
            {
                possibleMoves = board.PossibleMoves(players.Current.Position);
            }
            return possibleMoves;
        }
    }

    [SerializeField]
    private List<SC_BarrierLogic> PlacedBarriers = new();
    HashSet<Barrier> CheckedPlacementsCache = new();
    #endregion

    #region Events

    public static Action OnGameOver;
    public float BotDelay = 1;

    void Subscribe()
    {
        SC_PlayerLogic.OnPawnClick += OnPawnClick;
        SC_SquareLogic.OnSquareClick += OnSquareClick;
        SC_BarrierLogic.OnBarrierPlaced += OnBarrierPlaced;
        SC_BarrierLogic.OnBarrierClick += OnBarrierClick;

        SC_MenuController.OnEndGame += ClearGameBoard;
        SC_MenuController.OnStartGame += OnStartGameSingle;
        SC_MenuController.OnUnDo += OnUnDo;


        WarpListener.OnGameStarted += OnStartGameMulti;
        WarpListener.OnMoveCompleted += TryPlay;
    }

    void UnSubscribe()
    {        
        SC_PlayerLogic.OnPawnClick -= OnPawnClick;
        SC_SquareLogic.OnSquareClick -= OnSquareClick;
        SC_BarrierLogic.OnBarrierPlaced -= OnBarrierPlaced;
        SC_BarrierLogic.OnBarrierClick -= OnBarrierClick;
        
        SC_MenuController.OnEndGame -= ClearGameBoard;
        SC_MenuController.OnStartGame -= OnStartGameSingle;
        SC_MenuController.OnUnDo -= OnUnDo;

        WarpListener.OnGameStarted -= OnStartGameMulti;
        WarpListener.OnMoveCompleted -= TryPlay;
    }

    private void OnStartGameSingle()
    {
        StartGameSingle(SC_GameData.Instance.NumOfPlayers);
        StartCoroutine(RunSinglePlayerGame());
    }


    // callback for startGame
    private void OnStartGameMulti(string nextTurn)
    {
        SC_MenuController.OnChangeScreen(Screens.Game);
        StartGameMulti(SC_GameData.Instance.NumOfPlayers);
        while (nextTurn != players.NextTurn().name);
        Debug.Log("Starting game, IsMyTurn: "+IsMyTurn+" current player: "+players.Current.name+" my player "+players.myPlayer.name);
        SC_GameData.Instance.timerFill.color = players.Current.Color.GetRGB();
        SC_GameData.Instance.timer.StartTimer();
    }


    private void OnPawnClick()
    {
        if (!IsMyTurn) return;
        CancelPlacement();
        foreach (SC_SquareLogic squareLogic in PossibleMoves)
        {
            squareLogic.ToggleHighlight();
        }
    }

    private void OnSquareClick(Square square)
    {
        if (IsMyTurn && square != null)
        {
            PlayMove(square);
        }
    }

    private void OnBarrierClick(SC_BarrierLogic barrier)
    {
        if (barrier != null)
        {
            ClearPossibleMoves();
            PlacingBarrier = barrier;
        }
    }
    private void OnBarrierPlaced(SC_BarrierLogic barrier)
    {
        if (barrier != null)
        {
            PlacingBarrier = null;
            PlayPlace(barrier);
        }
    }
    #endregion

    #region Movement Logic

    bool TryMove(string targetStr) // TODO move to event logic 
    {
        Square target = null; 
        Move move = null; 
        if (!Square.TryParse(targetStr, out target) && !Move.TryParse(targetStr, out move)) 
        {
            Debug.Log($"Failed to parse string '{targetStr}' in to a valid Square");
            return false;
        }
        if (target == null && move != null) { target = move.Target; }
        // // Move Validation is done before the move is sent
        // if (!board.IsValidMove(curPos, curPos.GetDirection(target), out _, out _)) 
        // {
        //     Debug.Log($"'{curPos}' to '{targetStr}' is not a valid move in the current board state.");
        //     return false;
        // }
        SC_SquareLogic.OnSquareClick.Invoke(null); // to make the sound
        PlayMove(target);
        return true;
    }

    private void PlayMove(Square square) 
    {
        if (square == null) 
        {
            Debug.LogError("Failed to move, target square is null!");
            return; 
        }
        if (isMultiplayer && IsMyTurn)
        {
            Debug.Log(players.Current.name+" "+"sending "+square);
            WarpClient.GetInstance().sendMove(square.ToString(), players.Current.name);
        }
        history.LastPlay = new Move(players.Current.Position, square).ToString();
        board.MovePlayer(players.Current.Position, square);
        players.Current.Position = square;
        EndTurn();
    }

    #endregion

    #region Placement Logic

    private bool TryPlace(string barrierStr) // TODO move to event logic
    {
        if (!Barrier.TryParse(barrierStr, out Barrier barrier))
        {
            Debug.LogError($"Failed to parse {barrierStr} into a barrier");
            return false;
        }
        if (!IsValidPlacement(barrier))
        {
            Debug.LogError($"Failed to place {barrier}, not a valid placement");
            return false;
        }
        if (PlacingBarrier == null)
        {
            SC_BarrierLogic barrierLogic = players.Current.barriers[0];
            PlacingBarrier = barrierLogic;
            barrierLogic.Position = barrier.Position;
            barrierLogic.Direction = barrier.Direction;
        }
        SC_BarrierLogic.OnBarrierPlaced.Invoke(null); // to make the sound
        PlayPlace(PlacingBarrier);
        return true;
    }

    private void PlayPlace(SC_BarrierLogic barrier)
    {
        if (barrier == null) 
        {
            Debug.LogError("Failed to place, Barrier is null!");
            return;
        }
        players.Current.barriers.Remove(barrier);
        PlacedBarriers.Add(barrier);
        barrier.ToggleCollider();
        barrier.SetMaterial(0);
        PlacingBarrier = null;
        Barrier _barrier = (barrier.Position, barrier.Direction);
        if (isMultiplayer && IsMyTurn)
        {
            Debug.Log("sending "+_barrier);
            WarpClient.GetInstance().sendMove(_barrier.ToString(), players.Current.name);
        }
        board.PlaceBarrier(_barrier);
        history.LastPlay = _barrier.ToString();
        EndTurn();
    }

    /// <summary>
    /// Checks if a given player can reach its goal based on Best First Search
    /// </summary>
    /// <param name="p">player to check for</param>
    /// <returns><c>true</c> - if player can reach goal</returns>
    private bool HasPathToGoal(SC_PlayerLogic p, Barrier barrier) {
        BoardManager _board = board.Clone() as BoardManager;
        _board.PlaceBarrier(barrier);
        return new BestFirstSearch(p, _board).UntilCompleted();
    }
    
    #region API

    public bool IsValidPlacement(Barrier barrier)
    {
        if (barrier == null) 
        {
            return false;
        }
        if (CheckedPlacementsCache.Contains(barrier))
        {
            return true;
        }
        if (!board.IsValidPlacement(barrier))
        {
            return false;
        }
        if (!players.All(p=>HasPathToGoal(p, barrier)))
        {
            board.AddBlockedPlacement(barrier);
            return false;
        }
        CheckedPlacementsCache.Add(barrier);
        return true;
    }

    #endregion

    #endregion
    
    #region Game Logic 
    public void ClearGameBoard() // TODO move to view
    {
        Debug.Log("Clearing game board");
        SC_GameData.Instance.PickedColors.Clear();
        if (players != null && players.Count > 0)
        {
            foreach (var player in players) {
                if (player != null) {
                    foreach (var barrier in player.barriers) {
                        if (barrier != null)
                            GameObject.Destroy(barrier.gameObject);
                    }
                    GameObject.Destroy(player.gameObject);
                }
            }
        }
        if (PlacedBarriers != null && PlacedBarriers.Count > 0)
        {
            foreach (var barrier in PlacedBarriers) {
                if (barrier != null)
                    GameObject.Destroy(barrier.gameObject);
            }
        }
        PlacedBarriers.Clear();
        CheckedPlacementsCache.Clear();
        possibleMoves.Clear();
        board = null;
        players = null;
        history = null;
        Winner = null;
    }

    bool StartGameSingle(int numOfPlayers)
    {
        ClearGameBoard();
        if (numOfPlayers <= 0 || numOfPlayers > MAX_PLAYERS) 
        {
            Debug.LogError($"Failed to start game! invalid number of players ({numOfPlayers})");
            return false;
        }
        isMultiplayer = false;
        board = new BoardManager(numOfPlayers);
        players = new PlayersManager(numOfPlayers);
        history = new HistoryManager(numOfPlayers);
        SC_GameData.Instance.timerFill.color = players.Current.Color.GetRGB();
        SC_GameData.Instance.timer.StartTimer();
        return true;
    }

    void StartGameMulti(int numOfPlayers)
    {
        // cleargameboard in multiplayer logic on room connect
        if (numOfPlayers < 2 || MAX_PLAYERS < numOfPlayers) 
        {
            Debug.LogError($"Failed to start game! invalid players number ({MAX_PLAYERS < numOfPlayers})");
            return;
        }
        isMultiplayer = true;
        board = new BoardManager(numOfPlayers);
        history = new HistoryManager(numOfPlayers);
    }
    
    IEnumerator RunSinglePlayerGame()
    {
        while (!IsGameOver)
        {
            if (IsMyTurn) 
            {
                // wait for player action
                yield return new WaitUntil(() => !IsMyTurn);; 
            }
            else
            {
                // do ai action
                yield return new WaitForSeconds(BotDelay);
                if (!IsMyTurn) TryPlayAI();
                yield return null;
            }
        }
        yield return new WaitForSeconds(1);
    }
    
    void ClearPossibleMoves()
    {
        foreach (SC_SquareLogic squareLogic in PossibleMoves)
        {
            squareLogic.UnHighlight();
        }
        PossibleMoves.Clear();
    }

    void CancelPlacement()
    {
        if (PlacingBarrier != null) 
        {
            PlacingBarrier.CancelPlacement();
            PlacingBarrier = null;
        } 
    }

    private void ClearCache()
    {
        CheckedPlacementsCache = new();
        ClearPossibleMoves();
        CancelPlacement();
    }
    private void EndTurn()
    {
        SC_GameData.Instance.timer.StopTimer();
        ClearCache();
        if (!CheckWinner())
            players.NextTurn();
    }

    private bool CheckWinner()
    {
        if (players.Current.ReachedGoal) 
        {
            history.LastPlay = "Game Over.";
            Winner = players.Current;
            Debug.Log($"Winner {players.Current}, Color {Winner.Color}");
            SC_GameData.Instance.timer.StopTimer();
            OnGameOver?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// AI Logic used to determine and play the moves and placements for the ai and player if timed out before executing move.
    /// </summary>
    /// <returns><c>true</c>- if play was determined and executed successfully</returns>
    public void TryPlayAI() // TODO move to AI logic 
    {
        if (isMultiplayer && !IsMyTurn) { return; } // dont play for other players
        CancelPlacement();
        var searches = BestFirstSearch.ParallelSearch(players, board.Clone() as BoardManager);
        BestFirstSearch mySearch = searches[players.Current];
        BestFirstSearch minSearch = searches.Values.Min();
        // if my path is shortest, make a move.
        string myNextMove = mySearch.NextMove;
        if (mySearch.Cost == minSearch.Cost && TryMove(myNextMove)) {
            return;
        }
        // if not try blocking shortest path.
        searches.Remove(players.Current);
        minSearch = searches.Values.Min();
        SC_PlayerLogic minPlayer = searches.FirstOrDefault(ps => ps.Value == minSearch).Key;
        while (players.Current.HasBarriers && searches.Count > 0 && minSearch != null) {
            while (minSearch.HasMoves) { 
                if (Barrier.TryParse(minSearch.NextBlockingBarrier, out Barrier currBarrier) &&
                    !mySearch.IsBlockingMe(currBarrier))
                {
                    if (TryPlace(currBarrier.ToString())) 
                    {
                        return;
                    }
                }
            }
            searches.Remove(minPlayer);
            minSearch = searches.Values.Min();
            minPlayer = searches.FirstOrDefault(ps => ps.Value == minSearch).Key;
        }  
        // if failed to block all players, make my move.
        if (TryMove(myNextMove)) {     
            return; 
        }
        // if failed to make my move, make any of the possible moves
        foreach (SC_SquareLogic move in board.PossibleMoves(players.Current.Position))
        {
            if (TryMove(move.Position.ToString())) { 
                return; 
            }
        }
        // if all fails skip turn to avoid getting the game stuck
        EndTurn();
    }

    void TryPlay(string play, string nextTurn)
    {
        // CancelPlacement();
        if (!TryPlace(play)) { TryMove(play); }
        if (!IsGameOver)
            while (nextTurn != players.NextTurn().name);
            Debug.Log("current: "+players.Current.name+" nextTurn: "+nextTurn);
    }

    void OnUnDo()
    {
        if (isMultiplayer) { return; } // TODO think about how to do undo, maybe sendChat?
        // undo only active if there are moves in history
        if (history.Count < 2) { return; }
        do
        {
            ClearCache();
            players.PrevTurn();
            string lastPlay = history.LastPlay;
            Debug.Log("Undoing "+players.Current+" last play, "+lastPlay);
            if (Barrier.TryParse(lastPlay, out Barrier barrier))
            {
                foreach (var barrierLogic in PlacedBarriers)
                {
                    if ((barrierLogic.Position, barrierLogic.Direction) == barrier)
                    {
                        board.RemoveBarrier((barrierLogic.Position, barrierLogic.Direction));
                        (barrierLogic.Position, barrierLogic.Direction) = barrierLogic.Home;
                        players.Current.barriers.Add(barrierLogic);
                        PlacedBarriers.Remove(barrierLogic);
                        barrierLogic.ToggleCollider();
                        ClearCache();
                        break;
                    }
                }
            }
            if (Move.TryParse(lastPlay, out Move move))
            {
                board.MovePlayer(players.Current.Position, move.Source);
                players.Current.Position = move.Source;
                ClearCache();
            } 
        // undo runs until its my turn again
        } while (!IsMyTurn);
    }

    #endregion

    #region MonoBehaviour

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        UnSubscribe();
    }
    
    #endregion
}
