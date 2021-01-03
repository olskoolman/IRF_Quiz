﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRF_Quiz.Entities
{
    class QuizQuestions
    {
        public long QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string Answer1Text { get; set; }
        public string Answer2Text { get; set; }
        public string Answer3Text { get; set; }
        public int AnswerID { get; set; }
    }
}
