using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        bool AmIWhite = board.IsWhiteToMove;
        Move[] moves = board.GetLegalMoves();

        List<MoveEvaluation> BestMoves = new List<MoveEvaluation>();
        BestMoves.Add(new MoveEvaluation(moves[0], -100000));
        foreach (Move m in moves) {
            board.MakeMove(m);
            Move[] TestMoves = board.GetLegalMoves();

            if (board.IsInCheckmate()) {
                BestMoves.Clear();
                BestMoves.Add(new MoveEvaluation(m, 10000000));
                board.UndoMove(m);
                break;
            }

            MoveEvaluation WorstEval = new MoveEvaluation(m, 1000000);
            foreach (Move tm in TestMoves) {
                board.MakeMove(tm);
                int TestEval;
                if (board.IsInCheckmate()) {

                }
                TestEval = EvaluateBoard(board, AmIWhite);
                if (TestEval < WorstEval.eval) {
                    WorstEval.SetEval(TestEval);
                }
                board.UndoMove(tm);
            }
            board.UndoMove(m);

            if (WorstEval.eval > BestMoves[0].eval) {
                BestMoves.Clear();
                BestMoves.Add(WorstEval);
            } else if (WorstEval.eval == BestMoves[0].eval) {
                BestMoves.Add(WorstEval);
            }
        }
        BestMoves[0].SetEval(EvaluateMove(BestMoves[0].move, board, AmIWhite));
        MoveEvaluation FinalBestMove = BestMoves[0];
        foreach (MoveEvaluation m in BestMoves) {
            m.SetEval(EvaluateMove(m.move, board, AmIWhite));
            if (m.eval > FinalBestMove.eval) {
                FinalBestMove = m;
            }
        }
        return FinalBestMove.move;
    }

    int EvaluateBoard(Board b, bool white) {
        int Eval = (b.GetPieceList(PieceType.Rook, white).Count * 5) + (b.GetPieceList(PieceType.Bishop, white).Count * 3) + (b.GetPieceList(PieceType.Knight, white).Count * 3) + (b.GetPieceList(PieceType.Pawn, white).Count * 1) + (b.GetPieceList(PieceType.Queen, white).Count * 9);
        Eval -= (b.GetPieceList(PieceType.Rook, !white).Count * 5) + (b.GetPieceList(PieceType.Bishop, !white).Count * 3) + (b.GetPieceList(PieceType.Knight, !white).Count * 3) + (b.GetPieceList(PieceType.Pawn, !white).Count * 1) + (b.GetPieceList(PieceType.Queen, !white).Count * 9);
        return Eval;
    }

    int EvaluateMove(Move move, Board board, bool white) {
        int Eval = -(int)Math.Round(new Vector2(3.5f - move.TargetSquare.Rank, 3.5f - move.TargetSquare.File).Length());
        int BoardEval = EvaluateBoard(board, white);

        board.MakeMove(move);
        if (board.IsInStalemate()) {
            if (BoardEval < 0) {
                Eval += 100;
            } else {
                Eval += -5;
            }
        }
        if (board.IsInCheck()) {
            Eval += 7;
        }
        board.UndoMove(move);
        return Eval;
    }

    public class MoveEvaluation {
        public Move move;
        public int eval;

        public MoveEvaluation(Move m, int e = 0) {
            move = m;
            eval = e;
        }

        public void SetEval(int NewEval) {
            eval = NewEval;
        }
    }
}