﻿using IRF_Quiz.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IRF_Quiz
{
    /// <summary>
    /// Kérdések forrása: http://napikviz.tutioldal.hu/index.php?kvizkerdesek=valaszok
    /// </summary>
    public partial class Quizes : UserControl
    {
        private Splitter splitter1;
        private ComboBox cbUser;
        private Button btnStart;
        private Splitter splitter2;
        private Timer timer1;
        private System.ComponentModel.IContainer components;
        private Label lblQuestion;
        private Label lblAnswer1;
        private Label lblAnswer2;
        private Label lblAnswer3;
        private Button btnAnswer1;
        private Button btnAnswer2;
        private Button btnAnswer3;
        private Label lblResult;
        private Label lblCouner;

        QuizEntities context = new QuizEntities();
        List<QuizQuestions> quizQuestions = new List<QuizQuestions>();
        List<QuizAnswers> quizAnswers = new List<QuizAnswers>();

        private int CurrentCorrectAnswer;
        private int CurrentPlayer;
        private int CountDown;
        private int NumOfQuestions;
        private Label lbltrue;
        private Label lbltrueCount;
        private Label lblfalse;
        private Label lblfalsecounter;
        private long CurrentGameID;
        private int trueans;
        private int falseans;

        public Quizes()
        {
            quizAnswers.Clear();
            InitializeComponent();
            QuizHide();
            NumOfQuestions = 10;
        }

        private void Quizes_Load_1(object sender, EventArgs e)
        {
            context.Quizs.Load();
            context.Players.Load();
            context.Questions.Load();
            context.Answers.Load();
            context.Categories.Load();

            cbUser.DataSource = context.Players.Local;
            cbUser.DisplayMember = "PlayerName";

            timer1.Enabled = false;
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            trueans = 0;
            falseans = 0;

            quizQuestions.Clear();
            lblResult.Visible = false;

            Random r = new Random();
            CurrentGameID = r.Next(10000, 99999); //nem biztosított az egyediség -> UUID vagy külön tábla

            var player = (Player)cbUser.SelectedItem;
            CurrentPlayer = player.PlayerID;
            
            FillUpQuestions();
            var currentQ = (QuizQuestions)GetCurrentQ();
            timer1.Enabled = true;
            ShowQuestions(currentQ);
        }

        private void FillUpQuestions()
        {
            Random r = new Random();

            int[] rndQuestions = new int[NumOfQuestions];

            for (int i = 0; i < rndQuestions.Length; i++)
            {
                int rInt = r.Next(1171, 2340); //Ha marad kapacitás itt lehet egy első és utolsó ID lekérdezést beépíteni
                //for (int j = 0; j < i + 1; j++)
                //{
                //    if (rndQuestions[j] == rInt)
                //    {
                //        return;
                //        i--;
                //    }
                //}
                rndQuestions[i] = rInt;
            }

            for (int i = 0; i < rndQuestions.Length; i++)
            {

                int qID = rndQuestions[i];

                var question = from x in context.Questions
                               where x.QuestionID.Equals(qID)
                               select new { QuestionID = x.QuestionID, QuestionText = x.QuestionText };
                var answer = from x in context.Answers
                             where x.QuestionID.Equals(qID)
                             select new { Answer1 = x.A1Text, Answer2 = x.A2Text, Answer3 = x.A3Text, Solution = x.CorrectAnswer };

                QuizQuestions qq = new QuizQuestions();

                foreach (var item in question)
                {
                    qq.QuestionID = item.QuestionID;
                    qq.QuestionText = item.QuestionText;
                }

                foreach (var item in answer)
                {
                    qq.Answer1Text = item.Answer1;
                    qq.Answer2Text = item.Answer2;
                    qq.Answer3Text = item.Answer3;
                    qq.AnswerID = item.Solution;
                }

                quizQuestions.Add(qq);
            }
        }

        private object GetCurrentQ()
        {
            int index = NumOfQuestions - 1;
            var currentQ = quizQuestions.ElementAt(index);
            return currentQ;
        }

        private void ShowQuestions(QuizQuestions currentQ)
        {
            lblResult.Visible = false;

            NumOfQuestions--;

            lblQuestion.Text = currentQ.QuestionText;
            lblAnswer1.Text = currentQ.Answer1Text;
            lblAnswer2.Text = currentQ.Answer2Text;
            lblAnswer3.Text = currentQ.Answer3Text;
            CurrentCorrectAnswer = currentQ.AnswerID;
            QuizShow();
            CountDown = 15;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CountDown--;
            lblCouner.Text = CountDown.ToString();
            if (CountDown <= 0)
            {
                bool timeout = true;

                foreach (var item in quizAnswers)
                {
                    if (item.QuestionID == quizQuestions[NumOfQuestions].QuestionID)
                    {
                        timeout = false;
                    }
                }

                if (timeout)
                {
                    bool isItRight = IsItRight(4);
                    StoreAnswer(isItRight, 4);
                }

                lblCouner.Visible = false;
                if (NumOfQuestions <=0)
                {
                    QuizHide();
                    timer1.Enabled = false;
                    WriteToDB();
                }
                else
                {
                    var currentQ = (QuizQuestions)GetCurrentQ();
                    ShowQuestions(currentQ);
                }
            }
        }

        private void WriteToDB()
        {
            foreach (var item in quizAnswers)
            {
                Quiz currentA = new Quiz();
                currentA.GameID = item.GameID;
                currentA.PlayerFK = item.PlayerID;
                currentA.QuestionFK = item.QuestionID;
                currentA.Result = item.Result;
                currentA.Answer = item.AnswerID;
                currentA.Date = item.Date;

                context.Quizs.Add(currentA);

                try
                {
                    context.SaveChanges();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnAnswer1_Click(object sender, EventArgs e)
        {
            int AnswerNumber = 1;
            bool isItRight = IsItRight(AnswerNumber);
            StoreAnswer(isItRight, AnswerNumber);
            CountDown = 0;
        }

        private bool IsItRight(int answerNumber)
        {
            if (CurrentCorrectAnswer == answerNumber)
            {
                lblResult.Text = "Helyes";
                lblResult.ForeColor = Color.Green;
                trueans++;
                lbltrueCount.Text = trueans.ToString();
                lblResult.Visible = true;
                return true;
            }
            else
            {
                lblResult.Text = "Rossz";
                lblResult.ForeColor = Color.Red;
                falseans++;
                lblfalsecounter.Text = falseans.ToString();
                lblResult.Visible = true;
                return false;
            }
        }

        private void StoreAnswer(bool isItRight, int answerNumber)
        {
            QuizAnswers qa = new QuizAnswers();
            qa.AnswerID = answerNumber;
            qa.Date = DateTime.Today;
            qa.GameID = CurrentGameID;
            qa.PlayerID = CurrentPlayer;
            qa.QuestionID = quizQuestions[NumOfQuestions].QuestionID;
            qa.Result = isItRight;
            qa.AnswerID = answerNumber;

            quizAnswers.Add(qa);
        }

        private void btnAnswer2_Click(object sender, EventArgs e)
        {
            int AnswerNumber = 2;
            bool isItRight = IsItRight(AnswerNumber);
            StoreAnswer(isItRight, AnswerNumber);
            CountDown = 0;
        }

        private void btnAnswer3_Click(object sender, EventArgs e)
        {
            int AnswerNumber = 3;
            bool isItRight = IsItRight(AnswerNumber);
            StoreAnswer(isItRight, AnswerNumber);
            CountDown = 0;
        }
        private void QuizShow()
        {
            lblCouner.Visible = true;
            lblQuestion.Visible = true;
            lblAnswer1.Visible = true;
            lblAnswer2.Visible = true;
            lblAnswer3.Visible = true;
            btnAnswer1.Visible = true;
            btnAnswer2.Visible = true;
            btnAnswer3.Visible = true;
            lblfalsecounter.Visible = true;
            lbltrueCount.Visible = true;
            lbltrue.Visible = true;
            lblfalse.Visible = true;
        }

        private void QuizHide()
        {
            lblCouner.Visible = false;
            lblQuestion.Visible = false;
            lblAnswer1.Visible = false;
            lblAnswer2.Visible = false;
            lblAnswer3.Visible = false;
            lblResult.Visible = false;
            btnAnswer1.Visible = false;
            btnAnswer2.Visible = false;
            btnAnswer3.Visible = false;
            lblfalsecounter.Visible = false;
            lbltrueCount.Visible = false;
            lbltrue.Visible = false;
            lblfalse.Visible = false;

        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.cbUser = new System.Windows.Forms.ComboBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblCouner = new System.Windows.Forms.Label();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblQuestion = new System.Windows.Forms.Label();
            this.lblAnswer1 = new System.Windows.Forms.Label();
            this.lblAnswer2 = new System.Windows.Forms.Label();
            this.lblAnswer3 = new System.Windows.Forms.Label();
            this.btnAnswer1 = new System.Windows.Forms.Button();
            this.btnAnswer2 = new System.Windows.Forms.Button();
            this.btnAnswer3 = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.lbltrue = new System.Windows.Forms.Label();
            this.lbltrueCount = new System.Windows.Forms.Label();
            this.lblfalse = new System.Windows.Forms.Label();
            this.lblfalsecounter = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 593);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // cbUser
            // 
            this.cbUser.FormattingEnabled = true;
            this.cbUser.Location = new System.Drawing.Point(34, 20);
            this.cbUser.Name = "cbUser";
            this.cbUser.Size = new System.Drawing.Size(121, 21);
            this.cbUser.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(34, 92);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblCouner
            // 
            this.lblCouner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCouner.AutoSize = true;
            this.lblCouner.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCouner.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblCouner.Location = new System.Drawing.Point(584, 20);
            this.lblCouner.Name = "lblCouner";
            this.lblCouner.Size = new System.Drawing.Size(51, 37);
            this.lblCouner.TabIndex = 4;
            this.lblCouner.Text = "10";
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(3, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 593);
            this.splitter2.TabIndex = 5;
            this.splitter2.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblQuestion
            // 
            this.lblQuestion.AutoSize = true;
            this.lblQuestion.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuestion.Location = new System.Drawing.Point(29, 189);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(98, 25);
            this.lblQuestion.TabIndex = 6;
            this.lblQuestion.Text = "Question";
            // 
            // lblAnswer1
            // 
            this.lblAnswer1.AutoSize = true;
            this.lblAnswer1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnswer1.Location = new System.Drawing.Point(115, 265);
            this.lblAnswer1.Name = "lblAnswer1";
            this.lblAnswer1.Size = new System.Drawing.Size(67, 24);
            this.lblAnswer1.TabIndex = 7;
            this.lblAnswer1.Text = "Answ1";
            // 
            // lblAnswer2
            // 
            this.lblAnswer2.AutoSize = true;
            this.lblAnswer2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnswer2.Location = new System.Drawing.Point(115, 322);
            this.lblAnswer2.Name = "lblAnswer2";
            this.lblAnswer2.Size = new System.Drawing.Size(67, 24);
            this.lblAnswer2.TabIndex = 8;
            this.lblAnswer2.Text = "Answ2";
            // 
            // lblAnswer3
            // 
            this.lblAnswer3.AutoSize = true;
            this.lblAnswer3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnswer3.Location = new System.Drawing.Point(115, 381);
            this.lblAnswer3.Name = "lblAnswer3";
            this.lblAnswer3.Size = new System.Drawing.Size(67, 24);
            this.lblAnswer3.TabIndex = 9;
            this.lblAnswer3.Text = "Answ3";
            // 
            // btnAnswer1
            // 
            this.btnAnswer1.Location = new System.Drawing.Point(34, 265);
            this.btnAnswer1.Name = "btnAnswer1";
            this.btnAnswer1.Size = new System.Drawing.Size(75, 23);
            this.btnAnswer1.TabIndex = 10;
            this.btnAnswer1.Text = "Válasz";
            this.btnAnswer1.UseVisualStyleBackColor = true;
            this.btnAnswer1.Click += new System.EventHandler(this.btnAnswer1_Click);
            // 
            // btnAnswer2
            // 
            this.btnAnswer2.Location = new System.Drawing.Point(34, 325);
            this.btnAnswer2.Name = "btnAnswer2";
            this.btnAnswer2.Size = new System.Drawing.Size(75, 23);
            this.btnAnswer2.TabIndex = 11;
            this.btnAnswer2.Text = "Válasz";
            this.btnAnswer2.UseVisualStyleBackColor = true;
            this.btnAnswer2.Click += new System.EventHandler(this.btnAnswer2_Click);
            // 
            // btnAnswer3
            // 
            this.btnAnswer3.Location = new System.Drawing.Point(34, 384);
            this.btnAnswer3.Name = "btnAnswer3";
            this.btnAnswer3.Size = new System.Drawing.Size(75, 23);
            this.btnAnswer3.TabIndex = 12;
            this.btnAnswer3.Text = "Válasz";
            this.btnAnswer3.UseVisualStyleBackColor = true;
            this.btnAnswer3.Click += new System.EventHandler(this.btnAnswer3_Click);
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResult.Location = new System.Drawing.Point(198, 92);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(78, 25);
            this.lblResult.TabIndex = 13;
            this.lblResult.Text = "Helyes";
            // 
            // lbltrue
            // 
            this.lbltrue.AutoSize = true;
            this.lbltrue.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbltrue.Location = new System.Drawing.Point(317, 16);
            this.lbltrue.Name = "lbltrue";
            this.lbltrue.Size = new System.Drawing.Size(175, 25);
            this.lbltrue.TabIndex = 14;
            this.lbltrue.Text = "Helyes válaszok:";
            // 
            // lbltrueCount
            // 
            this.lbltrueCount.AutoSize = true;
            this.lbltrueCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbltrueCount.Location = new System.Drawing.Point(498, 16);
            this.lbltrueCount.Name = "lbltrueCount";
            this.lbltrueCount.Size = new System.Drawing.Size(24, 25);
            this.lbltrueCount.TabIndex = 15;
            this.lbltrueCount.Text = "0";
            // 
            // lblfalse
            // 
            this.lblfalse.AutoSize = true;
            this.lblfalse.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblfalse.Location = new System.Drawing.Point(317, 60);
            this.lblfalse.Name = "lblfalse";
            this.lblfalse.Size = new System.Drawing.Size(169, 25);
            this.lblfalse.TabIndex = 16;
            this.lblfalse.Text = "Rossz válaszok:";
            // 
            // lblfalsecounter
            // 
            this.lblfalsecounter.AutoSize = true;
            this.lblfalsecounter.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblfalsecounter.Location = new System.Drawing.Point(498, 60);
            this.lblfalsecounter.Name = "lblfalsecounter";
            this.lblfalsecounter.Size = new System.Drawing.Size(24, 25);
            this.lblfalsecounter.TabIndex = 17;
            this.lblfalsecounter.Text = "0";
            // 
            // Quizes
            // 
            this.Controls.Add(this.lblfalsecounter);
            this.Controls.Add(this.lblfalse);
            this.Controls.Add(this.lbltrueCount);
            this.Controls.Add(this.lbltrue);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnAnswer3);
            this.Controls.Add(this.btnAnswer2);
            this.Controls.Add(this.btnAnswer1);
            this.Controls.Add(this.lblAnswer3);
            this.Controls.Add(this.lblAnswer2);
            this.Controls.Add(this.lblAnswer1);
            this.Controls.Add(this.lblQuestion);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.lblCouner);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.cbUser);
            this.Controls.Add(this.splitter1);
            this.Name = "Quizes";
            this.Size = new System.Drawing.Size(651, 593);
            this.Load += new System.EventHandler(this.Quizes_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


    }
}
