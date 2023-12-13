using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
namespace Caro_vovanlinh
{
    public class Chessboard
    {


        #region Properties
        private Timer tmCoolDown;
        private ProgressBar prcbCoolDown; // Thêm biến này
        private List<Point> winningCells;

        private Panel pnlChessBoard;

        private Panel chessBoard;
         public Panel ChessBoard
         {
             get { return chessBoard; }
             set { chessBoard = value; }
         }

       
        private List<PLayer> player;
         public List<PLayer> Player
         {
             get { return player; }
             set { player = value; }
         }
        private TextBox playerName;
        public TextBox PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }
       
        //bien luu lai nguoi nao dnag danh 
        private int currentPlayer;
         public int CurrentPlayer
         {
             get { return currentPlayer; }
             set { currentPlayer =  value; }
         }
     

        private List<List<Button>> matrix;
        private List<List<Button>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }
        private event EventHandler playerMarked;
        public event EventHandler PlayerMarked
        {
            add
            {
                playerMarked += value;
            }
            remove
            {
                playerMarked -= value;
            }
        }
        private event EventHandler endedGame;
        public event EventHandler EndedGame
        {
            add
            {
                endedGame += value;
            }
            remove
            {
                endedGame -= value;
            }
        }

        private bool isGameStarted;
        public bool IsGameStarted
        {
            get { return isGameStarted; }
            set { isGameStarted = value; }
        }
        private Stack<Playinfo> playTimeLine;
        public Stack<Playinfo> PlayTimeLine
        {
            get { return playTimeLine; }
            set { playTimeLine = value; }
        }
        #endregion


        #region Initialize
        public Chessboard(Panel chessBoard, TextBox PlayerName, object pctMark, Timer tmCoolDown, ProgressBar prcbCoolDown, Panel pnlChessBoard)
        {
            // Khởi tạo và cấu hình đối tượng Timer

            // Gán tmCoolDown từ đối số vào biến tmCoolDown của lớp Chessboard
            this.tmCoolDown = tmCoolDown;
            this.ChessBoard = chessBoard;
            this.PlayerName = PlayerName;
            this.prcbCoolDown = prcbCoolDown;
            this.pnlChessBoard = pnlChessBoard;
            this.Player = new List<PLayer>()
             {
                 new PLayer("", Image.FromFile(Application.StartupPath + "\\Resources\\hinhx.jpg")),
                 new PLayer("", Image.FromFile(Application.StartupPath + "\\Resources\\HINHO.png"))
             };
             CurrentPlayer = 0;
             ChangePlayer();

            PlayTimeLine = new Stack<Playinfo>();

        }
        #endregion

        #region Methods
        public void DrawChessBoard()
        {
            ChessBoard.Enabled = true;
            Matrix = new List<List<Button>>();
            Button oldButton = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < cons.CHESS_BOARD_HEIGHT; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < cons.CHESS_BOARD_WIDTH; j++)
                {

                    Button btn = new Button()
                    {
                        Width = cons.CHESS_WIDTH,
                        Height = cons.CHESS_HEIGHT,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch, 
                        Tag = i.ToString()

                    };
                    btn.Click += btn_Click;

                    ChessBoard.Controls.Add(btn);
                    Matrix[i].Add(btn);  
                    oldButton = btn;
                }
                oldButton.Location = new Point(0, oldButton.Location.Y + cons.CHESS_HEIGHT);
                oldButton.Width = 0;
                oldButton.Height = 0;

            }
            // Gọi hàm StartGame() ở đây để khởi động trò chơi khi vẽ xong bàn cờ
            StartGame();
        }
      
        void btn_Click(object sender, EventArgs e)
        {
            if (!isGameStarted)
            {
                MessageBox.Show("Vui lòng bắt đầu trò chơi trước khi đánh cờ.");
                return;
            }
            Button btn = sender as Button;
            if (btn.BackgroundImage != null)
                return;
            btn.BackgroundImage = Player[CurrentPlayer].Mark;

            Mark(btn);
            PlayTimeLine.Push(new Playinfo(GetChessPoint(btn), currentPlayer));
            
            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

            ChangePlayer();
            if (playerMarked != null)
            
                playerMarked(this, new EventArgs());
            
            if (isEndGame(btn))
            {
                EndGame();
            }
           

        }


       




        // Thêm phương thức ClearChessBoard
        public void ClearChessboard()
        {


            foreach (var row in Matrix)
            {
                foreach (var button in row)
                {
                    button.BackgroundImage = null;
                }
            }
            /*
            // Đặt lại thời gian đếm ngược
            ResetCoolDownTimer();

            // Kiểm tra xem có quân cờ trên bàn cờ
            bool hasChessPiece = Matrix.Any(row => row.Any(button => button.BackgroundImage != null));

            if (!hasChessPiece)
            {
                // Nếu không có quân cờ, tắt thời gian đếm ngược
                tmCoolDown.Stop();
            }*/
            // Kiểm tra xem có người chơi nào đã đánh cờ chưa
            bool playerHasMoved = Matrix.Any(row => row.Any(button => button.BackgroundImage != null));

            if (playerHasMoved)
            {
                // Nếu có người chơi đã đánh cờ, đặt lại thời gian đếm ngược
                ResetCoolDownTimer();
            }
            else
            {
                // Nếu không có người chơi nào đã đánh cờ, không đếm thời gian
                tmCoolDown.Stop();
                prcbCoolDown.Value = 0;
            }

        }
        public void ResetTimer()
        {
            if (tmCoolDown != null)
            {
                tmCoolDown.Stop();
                tmCoolDown.Interval = 1000;
                tmCoolDown.Start();
            }
        }
        public void ResetCoolDownTimer()
        {
            if (tmCoolDown != null)
            {
                tmCoolDown.Stop();
                tmCoolDown.Interval = cons.COOL_DOWN_INTERVAL;
               prcbCoolDown.Value = 0; // Đặt giá trị thời gian ngược về 0
                tmCoolDown.Start();
            }
        }
        // Thêm sự kiện xử lý cho nút "Clear"
        public void ClearButton_Click(object sender, EventArgs e)
        {
            ClearChessboard();
        }
      
        public void StartGame()
        {
            // Bật chức năng vẽ các ô cờ trên bàn cờ
            // (Chú ý: Phần này đã được gọi trong hàm DrawChessBoard(), không cần gọi lại ở đây)
           
            // Cho phép người chơi thực hiện nước đầu tiên
            ChessBoard.Enabled = true;

            // Hiển thị tên người chơi đầu tiên
            ChangePlayer();
           

        }
        public bool Undo()
        {
            if (PlayTimeLine.Count <= 0)
                return false;
            Playinfo oldPoint = PlayTimeLine.Pop();
            Button btn = Matrix[oldPoint.Point.Y][oldPoint.Point.X];
            
            btn.BackgroundImage = null;
            
            if(PlayTimeLine.Count <= 0)
            {
               
                CurrentPlayer = 0;
            }else
            {
                oldPoint = PlayTimeLine.Peek();
                CurrentPlayer = oldPoint.CurrentPlayer == 1 ? 0 : 1;
            }    
            ChangePlayer();
            return true;

        }
        public void EndGame()
        {
            if(endedGame != null)
            {
                
                endedGame(this, new EventArgs());
                // Hiệu ứng làm nổi bật dòng kết quả
                
            }
            // Hiệu ứng làm nổi bật dòng kết quả
           
          
        }
        private void HighlightWinner()
        {
            if (winningCells != null && winningCells.Count > 0)
            {
                // Xác định dòng chiến thắng và làm nổi bật
                DrawWinningLine();
            }
        }
        private void DrawWinningLine()
        {
            if (winningCells == null || winningCells.Count == 0)
                return;

            // Xác định màu nền mới cho ô đánh cờ chiến thắng
            Color highlightColor = Color.Yellow;

            // Vẽ dòng chiến thắng bằng cách thay đổi màu nền của các ô trên dòng
            foreach (var cell in winningCells)
            {
                Button btn = Matrix[cell.Y][cell.X];
                btn.BackColor = highlightColor;
            }
        }

        private void ClearWinningLine()
        {
            if (winningCells == null || winningCells.Count == 0)
                return;

            // Đặt lại màu nền ban đầu cho các ô đã được làm nổi bật
            Color originalCellColor = Color.White;
            foreach (var cell in winningCells)
            {
                Button btn = Matrix[cell.Y][cell.X];
                btn.BackColor = originalCellColor;
            }
        }

        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimary(btn) || isEndSub(btn);
        }
        private Point GetChessPoint(Button btn)
        {
           
            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = Matrix[vertical].IndexOf(btn);
            Point point = new Point(horizontal, vertical);
            return point;
        }
        private bool isEndHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countleft = 0;
            for(int i = point.X; i >= 0 ; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countleft++;
                }
                else
                    break;
            }
            int countRight = 0;
            for (int i = point.X+1; i < cons.CHESS_BOARD_WIDTH; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                    break;
            }
            return countleft + countRight == 5;
        }
        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }
            int countBottom = 0;
            for (int i = point.Y + 1; i < cons.CHESS_BOARD_HEIGHT; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        private bool isEndPrimary(Button btn)
        {

            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = 0; i<= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }
            int countBottom = 0;
            for (int i = 1; i <= cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= cons.CHESS_BOARD_HEIGHT|| point.X + i >= cons.CHESS_BOARD_WIDTH)
                    break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        private bool isEndSub(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i > cons.CHESS_BOARD_WIDTH || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }
            int countBottom = 0;
            for (int i = 1; i <= cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= cons.CHESS_BOARD_HEIGHT || point.X - i < 0)
                    break;
                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        private void Mark(Button btn)
        {
            btn.BackgroundImage = Player[CurrentPlayer].Mark;
            //CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
        }
        private void ChangePlayer()
        {
            PlayerName.Text = Player[CurrentPlayer].Name;
        }
        #endregion
    }
}