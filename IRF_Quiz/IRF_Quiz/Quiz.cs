//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IRF_Quiz
{
    using System;
    using System.Collections.Generic;
    
    public partial class Quiz
    {
        public long QuizID { get; set; }
        public long GameID { get; set; }
        public int PlayerFK { get; set; }
        public long QuestionFK { get; set; }
        public bool Result { get; set; }
        public long Answer { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual Player Player { get; set; }
        public virtual Question Question { get; set; }
    }
}
