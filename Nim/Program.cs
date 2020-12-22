using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nim {
    class Program {
        static void Main(string[] args) {
            const int numOfPiles = 5;
            const int numOfRocks = 9;
            Game game = new Game(numOfPiles, numOfRocks);
            AI computer = new AI(game);
            for(int i = 0; i < 100000; i++) {
                while (!Utility.EndOfGame(game.rep)) {
                    game.RemoveRocks(computer.MakeMove(game.rep));
                }
                computer.AssignValues();
                game = new Game(numOfPiles, numOfRocks);
            }
            bool myMove = true;
            while (true) {
                myMove = myMove ? false : true;
                game = new Game(numOfPiles, numOfRocks);
                PrintGame(game.rep);
                while (!Utility.EndOfGame(game.rep)) {
                    if (myMove) {
                        string input = "";
                        do {
                            Console.Write("enter input (pile, num):");
                            input = Console.ReadLine().Trim();
                        } while (!Utility.CorrectInput(game,input));
                        game.RemoveRocks(input);
                        myMove = false;
                    } else {
                        game.RemoveRocks(computer.MakeMove(game.rep));
                        myMove = true;
                    }
                    PrintGame(game.rep);
                }
                Console.WriteLine("{0} won the game", myMove ? "You" : "Computer");
            }
     
        }


        public static void PrintGame(List<int> pilesOfRocks) {
            for (int i = 0; i < pilesOfRocks.Count(); i++) {
                string temp = new string('*', pilesOfRocks[i]);
                Console.WriteLine(temp + i.ToString().PadLeft(pilesOfRocks[0] - temp.Length + 3)  + "   "  + pilesOfRocks[i].ToString());
            }
            Console.WriteLine("-----------------------------------");
        }
    }

    class Game {
        public readonly int numOfPiles;
        public readonly int numOfRocks;
        public List<int> rep;

     

        public Game(int piles, int rocks) {
            numOfPiles = piles;
            numOfRocks = rocks;
            rep = new List<int>();
            for (int i = 0; i < piles; i++) {
                rep.Add(rocks);
            }
        }

        /// <summary>
        /// removes the rocks from the perspective pile
        /// </summary>
        /// <param name="move"> move = ("{0} {1}",pilenum,rocknum)</param>
        public void RemoveRocks(string move) {
            int pile = Convert.ToInt32(move.Substring(0, move.IndexOf(' ')));
            int rocks = Convert.ToInt32(move.Substring(move.IndexOf(' ') + 1));
            rep[pile] -= rocks;
            int[] temp = rep.ToArray();
            Array.Sort(temp);
            Array.Reverse(temp);
            rep = new List<int>(temp);
        }
    }

    static class Utility {

        /// <summary>
        /// returns the elements of the list with spaces between them
        /// </summary>
        /// <param name="before"></param>
        /// <returns></returns>
        public static string ListToString(List<int> before) {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < before.Count(); i++) {
                str.Append(before[i].ToString() + " ");
            }
            return str.ToString().Trim();
        }

        /// <summary>
        /// returns the elemsnts with a space between them
        /// </summary>
        /// <param name="before"></param>
        /// <returns></returns>
        public static string ArrayToString(int[] before) {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < before.Count(); i++) {
                str.Append(before[i].ToString() + " ");
            }
            return str.ToString().Trim();
        }

        /// <summary>
        /// returns true when only one pile with rocks
        /// </summary>
        /// <param name="rep"></param>
        public static bool EndOfGame(List<int> rep) {
            int count = 0;
            for(int i = 0; i < rep.Count(); i++) {
                if(rep[i] > 0) {
                    count++;
                }
            }
            return count < 2;
        }

        /// <summary>
        /// input has spacs trimmed on the ends
        /// </summary>
        /// <param name="game"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CorrectInput(Game game, string input) {
            if(input.Length  == 0 || !input.Contains(' ')) {
                return false;
            }
            bool correctInput = false;
            try {
                int pile = Convert.ToInt32(input.Substring(0, input.IndexOf(' ')));
                int rocks = Convert.ToInt32(input.Substring(input.IndexOf(' ') + 1));
                correctInput = game.numOfPiles > pile;
                return correctInput ? rocks <= game.rep[pile] : correctInput;
            } catch (Exception) {
                return false;
            }
        }
    }


    class AI {
        /// <summary>
        /// A dictionary
        /// keys = possible inputs
        /// values = dictionary [possible outputs, vaue]
        /// </summary>
        public IDictionary<string, IDictionary<string, int>> DecisionMap;

        private int Player;

        private int MovesMade;
        /// <summary>
        /// stores the moves of the computer
        /// </summary>
        private Queue<string[]> PlayerOneMoves;
        private Queue<string[]> PlayerTwoMoves;
        private Random chooseMove;

        /// <summary>
        /// initilize DecisionMap based off the dimensions of the game;
        /// </summary>
        /// <param name="game"></param>
        public AI(Game game) {
            DecisionMap = new Dictionary<string, IDictionary<string, int>>();
            Queue<List<int>> input = GetAllInput(game);
            while(input.Count() > 0) {
                List<int> tempInput = input.Dequeue();
                Dictionary<string, int> tempOutput = GetAllOutput(tempInput);
                string str = Utility.ListToString(tempInput);
                DecisionMap.Add(str, tempOutput);
            }
            PlayerOneMoves = new Queue<string[]>();
            PlayerTwoMoves = new Queue<string[]>();
            Player = 1;
            chooseMove = new Random();
        }


        /// <summary>
        /// makes the respective output dictionary for an input
        /// |input| = game.numOfPiles
        /// </summary>
        /// <param name="dict"> the dictionary to return</param>
        /// <param name="input"> aray containing the values of the piles in non increasing order</param>
        /// @returns dict with all values set to 0 and all possible output for the input
        private Dictionary<string, int> GetAllOutput(List<int> input) {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            HashSet<int> numOfRocksInPile = new HashSet<int>();
            for (int pileChosen = 0; pileChosen < input.Count(); pileChosen++) {
                if (!numOfRocksInPile.Contains(input[pileChosen])) {
                    for (int stonesRemoved = 1; stonesRemoved < input[pileChosen] + 1; stonesRemoved++) {
                        dict.Add(Utility.ArrayToString(new[] { pileChosen, stonesRemoved }), 0);
                    }
                }
                numOfRocksInPile.Add(input[pileChosen]);
            }
            return dict;
        }

        /// <summary>
        /// gets all possible input from a game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private Queue<List<int>> GetAllInput(Game game) {
            Queue<List<int>> Inputs = new Queue<List<int>>();
            if (game.numOfPiles > 1) {//base case is 1 pile
                Queue<List<int>> tempQueue = GetAllInput(new Game(game.numOfPiles - 1, game.numOfRocks));
                while (tempQueue.Count() > 0) {
                    List<int> temp = tempQueue.Dequeue();
                    for (int i = 0; i < temp[temp.Count() - 1] + 1; i++) {
                        List<int> newList = new List<int>();
                        //copy array
                        for (int j = 0; j < temp.Count(); j++) {
                            newList.Add(temp[j]);
                        }
                        //add new element
                        newList.Add(i);
                        Inputs.Enqueue(newList);
                    }
                }
            } else {
                for (int i = 0; i < game.numOfRocks + 1; i++/*num of rocks*/) {
                    List<int> temp = new List<int>();
                    temp.Add(i);
                    Inputs.Enqueue(temp);
                }
            }

            return Inputs;
        }

        /// <summary>
        /// considers the board and selectes the best ouput
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string MakeMove(List<int> input) {
            string newInput = Utility.ListToString(input);
            IDictionary<string, int> possibleOutputs = this.DecisionMap[newInput];
            string maxIndex = "0 1";   //this is always a possible output
            List<string> bestPossibilities = new List<string>();
            bestPossibilities.Add(maxIndex);
            foreach (string output in possibleOutputs.Keys){
                if(possibleOutputs[bestPossibilities[0]] == possibleOutputs[output]) {
                    bestPossibilities.Add(output);
                } else if (possibleOutputs[bestPossibilities[0]] < possibleOutputs[output]){
                    bestPossibilities.Clear();
                    bestPossibilities.Add(output);
                }
            }
            maxIndex = bestPossibilities[chooseMove.Next(bestPossibilities.Count())];
            string[] temp = { newInput, maxIndex };
            if(Player == 1) {
                PlayerOneMoves.Enqueue(temp);
                Player = 2;
            } else {
                PlayerTwoMoves.Enqueue(temp);
                Player = 1;
            }
            MovesMade++;
            return maxIndex;
        }

        /// <summary>
        /// adjusts the values accordingly to who won
        /// this is the reward/punishment after a game
        /// </summary>
        /// clears playerOneMoves and playerTwoMoves
        public void AssignValues() {
            //value of player is the Winner
            if (Player == 2) {
                while (PlayerOneMoves.Count() > 0) {
                    string[] badMove = PlayerOneMoves.Dequeue();
                    DecisionMap[badMove[0]][badMove[1]]--;
                }
                while (PlayerTwoMoves.Count() > 0) {
                    string[] goodMove = PlayerTwoMoves.Dequeue();
                    DecisionMap[goodMove[0]][goodMove[1]]++;
                }
            } else {
                while(PlayerTwoMoves.Count() > 0) {
                    string[] badMove = PlayerTwoMoves.Dequeue();
                    DecisionMap[badMove[0]][badMove[1]]--;
                }
                while(PlayerOneMoves.Count() > 0){
                    string[] goodMove = PlayerOneMoves.Dequeue();
                    DecisionMap[goodMove[0]][goodMove[1]]++;
                }
            }
        }
    }

}

