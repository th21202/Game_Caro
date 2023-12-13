using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Caro_vovanlinh
{
    public partial class FrmLinh : Form
    {
        #region Properties
        private bool isGameStarted = false;
        private bool playerNameEntered = false; // Biến để theo dõi xem người chơi đã nhập tên hay chưa

        Chessboard ChessBoard;
        private object Player;
        private bool playerHasMoved = false;
        private Timer tmCoolDown = new Timer(); // Đặt tên biến thành tmCoolDown
        private object pctMark;
        
        #endregion
        public FrmLinh()
        {
            InitializeComponent();
            tmCoolDown.Interval = cons.COOL_DOWN_INTERVAL;
            tmCoolDown.Interval = 1000; // Đặt độ trễ của đồng hồ đếm thời gian thành 1 giây
            tmCoolDown.Tick += tmCoolDown_Tick;


            //Chessboard chessboard = new Chessboard(pnlChessBoard, txtPlayerName, pctMark);
            ChessBoard = new Chessboard(pnlChessBoard, txtPlayerName, pctMark, tmCoolDown, prcbCoolDown, pnlChessBoard);

            // Thiết lập IsGameStarted của Chessboard
            ChessBoard.IsGameStarted = false; // Trò chơi chưa bắt đầu
            ChessBoard.EndedGame += Chessboard_EndedGame;
            ChessBoard.PlayerMarked += Chessboard_PlayerMarked;
            ChessBoard.DrawChessBoard();
            
            prcbCoolDown.Step = cons.COOL_DOWN_STEP;
            prcbCoolDown.Maximum = cons.COOL_DOWN_TIME;
            prcbCoolDown.Value = 0;
          

            // Tắt Timer khi form khởi đầu
            tmCoolDown.Stop();

        }
        private void btnStartGame_Click(object sender, EventArgs e)
        {

          
            // Kiểm tra xem người chơi đã nhập tên hay chưa
            if (!playerNameEntered)
            {
                string playerName = PromptForPlayerName("Nhập tên người chơi 1:");
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    MessageBox.Show("Vui lòng nhập tên người chơi 1.");
                    return;
                }
                ChessBoard.Player[0].Name = playerName;
                playerNameEntered = true;
            }

            string player2Name = PromptForPlayerName("Nhập tên người chơi 2:");
            if (string.IsNullOrWhiteSpace(player2Name))
            {
                playerNameEntered = false; // Đặt lại biến playerNameEntered để yêu cầu nhập lại tên
                btnStartGame_Click(sender, e); // Gọi lại btnStartGame_Click để nhập lại tên
                return;
            }


            // Cập nhật tên của hai người chơi vào TextBoxes
            //ChessBoard.Player[0].Name = playerName;
            ChessBoard.Player[1].Name = player2Name;



            // Bắt đầu trò chơi
           
            ChessBoard.StartGame();
            ChessBoard.IsGameStarted = true; // Đánh dấu rằng trò chơi đã bắt đầu
           //tmCoolDown.Start();
         

        }

       
        private void btnClear_Click(object sender, EventArgs e)
        {
            ChessBoard.ClearChessboard(); // Gọi phương thức ClearChessboard từ đối tượng ChessBoard
            ChessBoard.ResetCoolDownTimer(); // Đặt lại thời gian đếm ngược
            ChessBoard.StartGame();
            playerHasMoved = false; // Đặt lại trạng thái đánh cờ về false
                                    // Reset giá trị thời gian về COOL_DOWN_TIME
                                    // remainingTime = cons.COOL_DOWN_TIME;

            prcbCoolDown.Value = 0;
            tmCoolDown.Stop();
            
        }
        private string PromptForPlayerName(string prompt)
        {
            string playerName = Microsoft.VisualBasic.Interaction.InputBox(prompt, "Nhập tên người chơi", "Người chơi");

            // Kiểm tra nếu người chơi nhấn "Cancel" (hoặc đóng cửa sổ) và trả về null
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return null;
            }

            return playerName;
        }
       

       
        void EndGame()
        {
            tmCoolDown.Stop();
            pnlChessBoard.Enabled = false;
            button1.Enabled = false;
            MessageBox.Show("Kết thúc Game! CHức mừng bạn đã chiến thắng ");
        }
        private void Chessboard_EndedGame(object sender, EventArgs e)
        {
            EndGame();
        }
        private void ResetCountdown()
        {
            prcbCoolDown.Value = 0; // Đặt giá trị ProgressBar về 0 để đặt lại thời gian đếm ngược
        }
        private void Chessboard_PlayerMarked(object sender, EventArgs e)
        {
            // Kiểm tra xem người chơi đã đánh cờ chưa trước khi bắt đầu đếm thời gian
            /* if (ChessBoard.CurrentPlayer == 0 || ChessBoard.CurrentPlayer == 1)
             {
             tmCoolDown.Start();
             prcbCoolDown.Value = 0;
             }
             tmCoolDown.Start();
             prcbCoolDown.Value = 0;
             if (!playerHasMoved)
             {
                 // Người chơi đã đánh cờ, bắt đầu đếm thời gian
                 tmCoolDown.Start();
                 prcbCoolDown.Value = 0;
                 playerHasMoved = true; // Đã có người chơi đánh cờ
             }*/

            // Kiểm tra xem người chơi đã đánh cờ chưa trước khi bắt đầu đếm thời gian
            if (ChessBoard.CurrentPlayer == 0 || ChessBoard.CurrentPlayer == 1)
            {
                tmCoolDown.Start();
                prcbCoolDown.Value = 0;
            }

            if (!playerHasMoved)
            {
                // Người chơi đã đánh cờ, bắt đầu đếm thời gian
                tmCoolDown.Start();
                prcbCoolDown.Value = 0;
                playerHasMoved = true; // Đã có người chơi đánh cờ
            }
            else
            {
                // Đặt lại thời gian đếm ngược và giá trị ProgressBar khi người chơi thực hiện xong nước đi
                ResetCountdown();
            }
        }
        public void tmCoolDown_Tick(object sender, EventArgs e)
        {
            prcbCoolDown.PerformStep();
            if(prcbCoolDown.Value >= prcbCoolDown.Maximum)
            {
               
                EndGame();
                //tmCoolDown.Stop(); // Dừng Timer khi trò chơi kết thúc

            }

           
            
        }
          void Undo(object sender, EventArgs e)
        {
            ChessBoard.Undo();
        }
    }
}
